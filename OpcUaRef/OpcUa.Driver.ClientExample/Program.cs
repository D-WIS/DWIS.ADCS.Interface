using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpcUa.Driver;
using OpcUa.Driver;
using OpcUa.Driver.Client;
using OpcUa.Driver.ClientExample;

using var host = Host.CreateDefaultBuilder(args).Build();

var logger = host.Services.GetRequiredService<ILogger<OpcUaDriver>>();
logger.LogInformation("Client Application Started.");

await run();
await host.RunAsync();

IOpcUaClient client;
async Task run()
{

	var opcUaDriver = new OpcUaDriver(logger);
	await opcUaDriver.Init("OpcUa.Client");

	var serverUrl = "opc.tcp://localhost:62541/Quickstarts/ReferenceServer";
	do
	{
		try
		{
			client = await opcUaDriver.Connect(serverUrl,true,false).ConfigureAwait(false);
			break;
		}
		catch (Exception ex)
		{
			await Task.Delay(500);
			logger.LogError(ex.Message);
		}
	} while (true);

	//var app = new DownlinkRequest(logger, client);
	var app = new ExampleApp(logger, client);

	app.Run();
}