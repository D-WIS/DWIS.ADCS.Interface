using System.Collections;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;

namespace OpcUa.Driver;

/// <summary>
/// OPC UA Client
/// </summary>
public class OpcUaConnection : IOpcUaConnection, IDisposable
{
	#region Constructors
	/// <summary>
	/// Initializes a new instance of the OpcUaConnection class.
	/// </summary>
	public OpcUaConnection(ApplicationConfiguration configuration, ILogger logger, Action<IList, IList> validateResponse)
	{
		_validateResponse = validateResponse;
		_configuration = configuration;
		_logger = logger;
		_configuration.CertificateValidator.CertificateValidation += CertificateValidation;
		_reverseConnectManager = null;
	}

	/// <summary>
	/// Initializes a new instance of the OpcUaConnection class for reverse connections.
	/// </summary>
	public OpcUaConnection(ApplicationConfiguration configuration, ReverseConnectManager reverseConnectManager, ILogger logger, Action<IList, IList> validateResponse)
	{
		_validateResponse = validateResponse;
		_configuration = configuration;
		_configuration.CertificateValidator.CertificateValidation += CertificateValidation;
		_reverseConnectManager = reverseConnectManager;
		_logger = logger;
	}
	#endregion

	#region IDisposable
	/// <summary>
	/// Dispose objects.
	/// </summary>
	public void Dispose()
	{
		Utils.SilentDispose(_session);
		_configuration.CertificateValidator.CertificateValidation -= CertificateValidation;
		GC.SuppressFinalize(this);
	}
	#endregion

	#region Public Properties
	/// <summary>
	/// Action used 
	/// </summary>
	Action<IList, IList> ValidateResponse => _validateResponse;

	/// <summary>
	/// Gets the client session.
	/// </summary>
	public ISession Session => _session;

	/// <summary>
	/// The session keepalive interval to be used in ms.
	/// </summary>
	public int KeepAliveInterval { get; set; } = 5000;

	/// <summary>
	/// The reconnect period to be used in ms.
	/// </summary>
	public int ReconnectPeriod { get; set; } = 5000;

	/// <summary>
	/// The reconnect period exponential backoff to be used in ms.
	/// </summary>
	public int ReconnectPeriodExponentialBackoff { get; set; } = 15000;

	/// <summary>
	/// The session lifetime.
	/// </summary>
	public uint SessionLifeTime { get; set; } = 60 * 1000;

	/// <summary>
	/// The user identity to use to connect to the server.
	/// </summary>
	public IUserIdentity UserIdentity { get; set; } = new UserIdentity();

	/// <summary>
	/// Auto accept untrusted certificates.
	/// </summary>
	public bool AutoAccept { get; set; } = false;

	/// <summary>
	/// The file to use for log output.
	/// </summary>
	public string LogFile { get; set; }
	#endregion

	#region Public Methods
	/// <summary>
	/// Creates a session with the UA server
	/// </summary>
	public async Task<bool> ConnectAsync(string serverUrl, bool useSecurity = true, CancellationToken cancellationToken = default)
	{
		if (serverUrl == null) throw new ArgumentNullException(nameof(serverUrl));

		try
		{
			if (_session is { Connected: true })
			{
				_logger.LogInformation("Session already connected!");
				return true;
			}

			ITransportWaitingConnection? connection = null;
			EndpointDescription? endpointDescription = null;
			if (_reverseConnectManager != null)
			{
				_logger.LogInformation("Waiting for reverse connection to.... {0}", serverUrl);
				do
				{
					using var cts = new CancellationTokenSource(30_000);
					using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

					connection = await _reverseConnectManager.WaitForConnection(new Uri(serverUrl), null, linkedCts.Token).ConfigureAwait(false);
					if (connection == null)
					{
						throw new ServiceResultException(StatusCodes.BadTimeout, "Waiting for a reverse connection timed out.");
					}

					if (endpointDescription == null)
					{
						_logger.LogInformation("Discover reverse connection endpoints....");
						endpointDescription = CoreClientUtils.SelectEndpoint(_configuration, connection, useSecurity);
						connection = null;
					}
				} while (connection == null);
			}
			else
			{
				_logger.LogInformation("Connecting to... {0}", serverUrl);
				endpointDescription = CoreClientUtils.SelectEndpoint(_configuration, serverUrl, useSecurity);
			}

			// Get the endpoint by connecting to server's discovery endpoint.
			// Try to find the first endpoint with security.
			var endpointConfiguration = EndpointConfiguration.Create(_configuration);
			var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

			// Create the session
			var session = await Opc.Ua.Client.Session.Create(
				_configuration,
				connection,
				endpoint,
				connection == null,
				false,
				_configuration.ApplicationName,
				SessionLifeTime,
				UserIdentity,
				null
			).ConfigureAwait(false);

			// Assign the created session
			if (session is { Connected: true })
			{
				_session = session;

				// override keep alive interval
				_session.KeepAliveInterval = KeepAliveInterval;

				// support transfer
				_session.DeleteSubscriptionsOnClose = false;
				_session.TransferSubscriptionsOnReconnect = true;

				// set up keep alive callback.
				_session.KeepAlive += Session_KeepAlive;

				// prepare a reconnect handler
				_reconnectHandler = new SessionReconnectHandler(true, ReconnectPeriodExponentialBackoff);

				// Session created successfully.
				_logger.LogInformation("New Session Created with SessionName = {0}", session.SessionName);

				return true;
			}

			_logger.LogWarning("Can not create session: SessionName = {0}", session.SessionName);

			return false;

		}
		catch (Exception ex)
		{
			// Log Error
			_logger.LogError("Create Session Error : {0}", ex.Message);
			return false;
		}
	}

	/// <summary>
	/// Disconnects the session.
	/// </summary>
	public void Disconnect()
	{
		try
		{
			if (_session != null)
			{
				_logger.LogInformation("Disconnecting...");

				lock (_lock)
				{
					_session.KeepAlive -= Session_KeepAlive;
					_reconnectHandler?.Dispose();
					_reconnectHandler ??= null;
				}

				_session.Close();
				_session.Dispose();
				_session = null;

				// Log Session Disconnected event
				_logger.LogInformation("Session Disconnected.");
			}
			else
			{
				_logger.LogWarning("Session not created!");
			}
		}
		catch (Exception ex)
		{
			// Log Error
			_logger.LogError($"Disconnect Error : {ex.Message}");
		}
	}
	/// <summary>
	/// Handles a keep alive event from a session and triggers a reconnect if necessary.
	/// </summary>
	private void Session_KeepAlive(ISession session, KeepAliveEventArgs e)
	{
		try
		{
			// check for events from discarded sessions.
			if (!ReferenceEquals(session, _session))
			{
				return;
			}

			if (!ServiceResult.IsBad(e.Status)) return;

			// start reconnect sequence on communication error.
			if (ReconnectPeriod <= 0)
			{
				Utils.LogWarning("KeepAlive status {0}, but reconnect is disabled.", e.Status);
				return;
			}

			var state = _reconnectHandler?.BeginReconnect(_session, _reverseConnectManager, ReconnectPeriod, Client_ReconnectComplete);
			if (state == SessionReconnectHandler.ReconnectState.Triggered)
			{
				Utils.LogInfo("KeepAlive status {0}, reconnect status {1}, reconnect period {2}ms.", e.Status, state, ReconnectPeriod);
			}
			else
			{
				Utils.LogInfo("KeepAlive status {0}, reconnect status {1}.", e.Status, state);
			}
		}
		catch (Exception exception)
		{
			Utils.LogError(exception, "Error in OnKeepAlive.");
		}
	}

	/// <summary>
	/// Called when the reconnect attempt was successful.
	/// </summary>
	private void Client_ReconnectComplete(object sender, EventArgs e)
	{
		// ignore callbacks from discarded objects.
		if (!ReferenceEquals(sender, _reconnectHandler))
		{
			return;
		}

		lock (_lock)
		{
			// if session recovered, Session property is null
			if (_reconnectHandler.Session == null)
			{
				_logger.LogInformation("--- RECONNECT KeepAlive recovered ---");
				return;
			}

			// after reactivate, the same session instance may be returned
			if (ReferenceEquals(_session, _reconnectHandler.Session))
			{
				_logger.LogInformation("--- REACTIVATED SESSION --- {0}", _reconnectHandler.Session.SessionId);
				return;
			}

			_logger.LogInformation("--- RECONNECTED TO NEW SESSION --- {0}", _reconnectHandler.Session.SessionId);
			var session = _session;
			_session = _reconnectHandler.Session;
			// dispose old session
			Utils.SilentDispose(session);
		}
	}
	#endregion

	#region Protected Methods
	/// <summary>
	/// Handles the certificate validation event.
	/// This event is triggered every time an untrusted certificate is received from the server.
	/// </summary>
	protected virtual void CertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
	{
		var certificateAccepted = false;

		// ****
		// Implement a custom logic to decide if the certificate should be
		// accepted or not and set certificateAccepted flag accordingly.
		// The certificate can be retrieved from the e.Certificate field
		// ***

		var error = e.Error;
		_logger.LogError("Certificate validation error: {0}", error);
		if (error.StatusCode == StatusCodes.BadCertificateUntrusted && AutoAccept)
		{
			certificateAccepted = true;
		}

		if (certificateAccepted)
		{
			_logger.LogInformation("Untrusted Certificate accepted. Subject = {0}", e.Certificate.Subject);
			e.Accept = true;
		}
		else
		{
			_logger.LogInformation("Untrusted Certificate rejected. Subject = {0}", e.Certificate.Subject);
		}
	}
	#endregion

	#region Private Fields
	private readonly object _lock = new ();
	private readonly ReverseConnectManager? _reverseConnectManager;
	private readonly ApplicationConfiguration _configuration;
	private readonly ILogger _logger;
	private SessionReconnectHandler? _reconnectHandler;
	private ISession? _session;
	private readonly Action<IList, IList> _validateResponse;
	#endregion
}