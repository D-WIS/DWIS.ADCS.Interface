using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpcUa.Driver.TestServer;

var autoAccept = true;
string password = null;
var applicationName = "ConsoleReferenceServer";//"OpcUa.Driver.TestServer";
var configSectionName = "Quickstarts.ReferenceServer";//"OpcUa.Driver.TestServer";// 
var shadowConfig = false;
var renewCertificate = false;
var cttMode = false;


using var host = Host.CreateDefaultBuilder(args).Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Client Application Started.");

await run();
await host.RunAsync();
async Task run()
{
	AnsiConsole.MarkupLine("[underline red]DWIS ADCS Interface[/]!");
	var output = Console.Out;
	try
	{
		// create the UA server
		var server = new UAServer<ReferenceServer>(output)
		{
			AutoAccept = autoAccept,
			Password = password
		};

		// load the server configuration, validate certificates
		output.WriteLine("Loading configuration from {0}.", configSectionName);
		await server.LoadAsync(applicationName, configSectionName).ConfigureAwait(false);

		// use the shadow config to map the config to an externally accessible location
		if (shadowConfig)
		{
			output.WriteLine("Using shadow configuration.");
			var shadowPath = Directory.GetParent(Path.GetDirectoryName(
				Opc.Ua.Utils.ReplaceSpecialFolderNames(server.Configuration.TraceConfiguration.OutputFilePath))).FullName;
			var shadowFilePath = Path.Combine(shadowPath, Path.GetFileName(server.Configuration.SourceFilePath));
			if (!File.Exists(shadowFilePath))
			{
				output.WriteLine("Create a copy of the config in the shadow location.");
				File.Copy(server.Configuration.SourceFilePath, shadowFilePath, true);
			}
			output.WriteLine("Reloading configuration from {0}.", shadowFilePath);
			await server.LoadAsync(applicationName, Path.Combine(shadowPath, configSectionName)).ConfigureAwait(false);
		}


		// check or renew the certificate
		output.WriteLine("Check the certificate.");
		await server.CheckCertificateAsync(renewCertificate).ConfigureAwait(false);

		// Create and add the node managers
		server.Create(Utilities.NodeManagerFactories);

		// start the server
		output.WriteLine("Start the server.");
		await server.StartAsync().ConfigureAwait(false);

		// Apply custom settings for CTT testing
		if (cttMode)
		{
			output.WriteLine("Apply settings for CTT.");
			// start Alarms and other settings for CTT test
			Utilities.ApplyCTTMode(output, server.Server);
		}

		output.WriteLine("Server started. Press Ctrl-C to exit...");

	//// stop server. May have to wait for clients to disconnect.
	//output.WriteLine("Server stopped. Waiting for exit...");
	//await server.StopAsync().ConfigureAwait(false);

}
	catch (ErrorExitException eee)
	{
	output.WriteLine("The application exits with error: {0}", eee.Message);
}
}