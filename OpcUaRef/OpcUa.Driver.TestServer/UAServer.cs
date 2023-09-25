using System.Text;
using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Server;

namespace OpcUa.Driver.TestServer;

public class UAServer<T> where T : StandardServer, new()
{
	public ApplicationInstance Application => _application;
	public ApplicationConfiguration Configuration => _application.ApplicationConfiguration;

	public bool AutoAccept { get; set; }
	public string Password { get; set; }

	public ExitCode ExitCode { get; private set; }
	public T Server => _server;

	/// <summary>
	/// Ctor of the server.
	/// </summary>
	/// <param name="writer">The text output.</param>
	public UAServer(TextWriter writer)
	{
		_Output = writer;
	}

	/// <summary>
	/// Load the application configuration.
	/// </summary>
	public async Task LoadAsync(string applicationName, string configSectionName)
	{
		try
		{
			ExitCode = ExitCode.ErrorNotStarted;

			ApplicationInstance.MessageDlg = new ApplicationMessageDlg(_Output);
			var PasswordProvider = new CertificatePasswordProvider(Password);
			_application = new ApplicationInstance {
				ApplicationName = applicationName,
				ApplicationType = ApplicationType.Server,
				ConfigSectionName = configSectionName,
				CertificatePasswordProvider = PasswordProvider
			};

			// load the application configuration.
			await _application.LoadApplicationConfiguration(false).ConfigureAwait(false);

		}
		catch (Exception ex)
		{
			throw new ErrorExitException(ex.Message, ExitCode);
		}
	}

	/// <summary>
	/// Load the application configuration.
	/// </summary>
	public async Task CheckCertificateAsync(bool renewCertificate)
	{
		try
		{
			var config = _application.ApplicationConfiguration;
			if (renewCertificate)
			{
				await _application.DeleteApplicationInstanceCertificate().ConfigureAwait(false);
			}

			// check the application certificate.
			var haveAppCertificate = await _application.CheckApplicationInstanceCertificate(false, minimumKeySize: 0).ConfigureAwait(false);
			if (!haveAppCertificate)
			{
				throw new ErrorExitException("Application instance certificate invalid!");
			}

			if (!config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
			{
				config.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
			}
		}
		catch (Exception ex)
		{
			throw new ErrorExitException(ex.Message, ExitCode);
		}
	}

	/// <summary>
	/// Create server instance and add node managers.
	/// </summary>
	public void Create(IList<INodeManagerFactory> nodeManagerFactories)
	{
		try
		{
			// create the server.
			_server = new T();
			if (nodeManagerFactories != null)
			{
				foreach (var factory in nodeManagerFactories)
				{
					_server.AddNodeManager(factory);
				}
			}
		}
		catch (Exception ex)
		{
			throw new ErrorExitException(ex.Message, ExitCode);
		}
	}

	/// <summary>
	/// Start the server.
	/// </summary>
	public async Task StartAsync()
	{
		try
		{
			// create the server.
			_server = _server ?? new T();

			// start the server
			await _application.Start(_server).ConfigureAwait(false);

			// save state
			ExitCode = ExitCode.ErrorRunning;

			// print endpoint info
			var endpoints = _application.Server.GetEndpoints().Select(e => e.EndpointUrl).Distinct();
			foreach (var endpoint in endpoints)
			{
				_Output.WriteLine(endpoint);
			}

			// start the status thread
			_status = Task.Run(StatusThreadAsync);

			// print notification on session events
			_server.CurrentInstance.SessionManager.SessionActivated += EventStatus;
			_server.CurrentInstance.SessionManager.SessionClosing += EventStatus;
			_server.CurrentInstance.SessionManager.SessionCreated += EventStatus;
		}
		catch (Exception ex)
		{
			throw new ErrorExitException(ex.Message, ExitCode);
		}
	}

	/// <summary>
	/// Stops the server.
	/// </summary>
	public async Task StopAsync()
	{
		try
		{
			if (_server != null)
			{
				using (var server = _server)
				{
					// Stop status thread
					_server = null;
					await _status.ConfigureAwait(false);

					// Stop server and dispose
					server.Stop();
				}
			}

			ExitCode = ExitCode.Ok;
		}
		catch (Exception ex)
		{
			throw new ErrorExitException(ex.Message, ExitCode.ErrorStopping);
		}
	}

	/// <summary>
	/// The certificate validator is used
	/// if auto accept is not selected in the configuration.
	/// </summary>
	private void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
	{
		if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
		{
			if (AutoAccept)
			{
				_Output.WriteLine("Accepted Certificate: [{0}] [{1}]", e.Certificate.Subject, e.Certificate.Thumbprint);
				e.Accept = true;
				return;
			}
		}
		_Output.WriteLine("Rejected Certificate: {0} [{1}] [{2}]", e.Error, e.Certificate.Subject, e.Certificate.Thumbprint);
	}

	/// <summary>
	/// Update the session status.
	/// </summary>
	private void EventStatus(Session session, SessionEventReason reason)
	{
		_lastEventTime = DateTime.UtcNow;
		PrintSessionStatus(session, reason.ToString());
	}

	/// <summary>
	/// Output the status of a connected session.
	/// </summary>
	private void PrintSessionStatus(Session session, string reason, bool lastContact = false)
	{
		var item = new StringBuilder();
		lock (session.DiagnosticsLock)
		{
			item.AppendFormat("{0,9}:{1,20}:", reason, session.SessionDiagnostics.SessionName);
			if (lastContact)
			{
				item.AppendFormat("Last Event:{0:HH:mm:ss}", session.SessionDiagnostics.ClientLastContactTime.ToLocalTime());
			}
			else
			{
				if (session.Identity != null)
				{
					item.AppendFormat(":{0,20}", session.Identity.DisplayName);
				}
				item.AppendFormat(":{0}", session.Id);
			}
		}
		_Output.WriteLine(item.ToString());
	}

	/// <summary>
	/// Status thread, prints connection status every 10 seconds.
	/// </summary>
	private async Task StatusThreadAsync()
	{
		while (_server != null)
		{
			if (DateTime.UtcNow - _lastEventTime > TimeSpan.FromMilliseconds(10000))
			{
				var sessions = _server.CurrentInstance.SessionManager.GetSessions();
				for (var ii = 0; ii < sessions.Count; ii++)
				{
					var session = sessions[ii];
					PrintSessionStatus(session, "-Status-", true);
				}
				_lastEventTime = DateTime.UtcNow;
			}
			await Task.Delay(1000).ConfigureAwait(false);
		}
	}

	#region Private Members
	private readonly TextWriter _Output;
	private ApplicationInstance _application;
	private T _server;
	private Task _status;
	private DateTime _lastEventTime;
	#endregion
}