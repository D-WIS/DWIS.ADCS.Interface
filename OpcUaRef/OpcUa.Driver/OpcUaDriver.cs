using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using OpcUa.Driver.Client;

namespace OpcUa.Driver;

public class OpcUaDriver : IOpcUaDriver
{
	private ReverseConnectManager _reverseConnectManager;
	private ApplicationInstance _application;

	public OpcUaDriver(ILogger Logger)
	{
		this.Logger = Logger;
	}

	public ILogger Logger { get; init; }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="appName">make sure the appName is the same as {appName}.Config.xml that is in the same folder as the application</param>
	/// <param name="password"></param>
	/// <param name="renewCertificate">Delete the previous application certificates: in the configuration xml file: SecurityConfiguration > ApplicationCertificate and the TrustedPeerCertificates</param>
	/// <param name="reverseConnectUrlString"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public virtual async Task Init(string appName, string? password = null, bool renewCertificate = false,
		string? reverseConnectUrlString = null)
	{
		Utils.SetLogger(Logger);

		//ApplicationInstance.MessageDlg = new ApplicationMessageDlg(output);
		var passwordProvider = new CertificatePasswordProvider(password);
		_application = new ApplicationInstance
		{
			ApplicationName = appName,
			ApplicationType = ApplicationType.Client,
			ConfigSectionName = appName,// $"{appName}.Config.xml",
			CertificatePasswordProvider = passwordProvider
		};

		// silent:true => no ApplicationInstance.MessageDlg to show loading error detail, if can not loading configuration
		var config = await _application.LoadApplicationConfiguration(silent: true).ConfigureAwait(false);

		// override logfile
		var logFilePath = config.TraceConfiguration.OutputFilePath;
		if (string.IsNullOrEmpty(logFilePath))
		{
			config.TraceConfiguration.OutputFilePath =
				$"%LOCALAPPDATA%/Schlumberger/OpcUa/OpcUaDriver/Logs/{appName}.OpcUa.log.txt";
		}

		//config.TraceConfiguration.DeleteOnLoad = true;
		config.TraceConfiguration.ApplySettings();

		if (renewCertificate)
		{
			await _application.DeleteApplicationInstanceCertificate().ConfigureAwait(false);
		}

		// check the application certificate.
		var haveAppCertificate = await _application.CheckApplicationInstanceCertificate(true, minimumKeySize: 0)
			.ConfigureAwait(false);

		if (!haveAppCertificate)
		{
			throw new Exception("Application instance certificate invalid!");
		}

		if (reverseConnectUrlString != null)
		{
			// start the reverse connection manager
			Logger.LogInformation("Create reverse connection endpoint at {0}.", reverseConnectUrlString);
			_reverseConnectManager = new ReverseConnectManager();
			_reverseConnectManager.AddEndpoint(new Uri(reverseConnectUrlString));
			_reverseConnectManager.StartService(config);
		}

	}

	public async Task<IOpcUaClient> Connect(string serverUrl, bool autoAccept = true, bool useSecurity = true, string? username = null, string? userPassword = null, CancellationToken token = default)
	{
		var opcUaConnection = new OpcUaConnection(_application.ApplicationConfiguration, _reverseConnectManager, Logger,
				ClientBase.ValidateResponse) { AutoAccept = autoAccept, SessionLifeTime = 60_000, };

		// set user identity
		if (!string.IsNullOrEmpty(username))
		{
			opcUaConnection.UserIdentity = new UserIdentity(username, userPassword ?? string.Empty);
		}

		var connected = await opcUaConnection.ConnectAsync(serverUrl, useSecurity, token)
			.ConfigureAwait(false);
		if (connected)
		{
			Logger.LogInformation("OpcUa Connected!");

			// enable subscription transfer
			opcUaConnection.ReconnectPeriod = 1000;
			opcUaConnection.ReconnectPeriodExponentialBackoff = 10000;
			opcUaConnection.Session.MinPublishRequestCount = 3;
			opcUaConnection.Session.TransferSubscriptionsOnReconnect = true;

			var client = new OpcUaClient(opcUaConnection, Logger);
			return client;
		}

		throw new Exception("Connection not setup!");
	}

	public void Deconstruct(out ILogger Logger)
	{
		Logger = this.Logger;
	}

}