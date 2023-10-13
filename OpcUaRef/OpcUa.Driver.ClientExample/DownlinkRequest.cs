using Microsoft.Extensions.Logging;
using Opc.Ua.Client;
using Opc.Ua;
using OpcUa.Driver;
using DWIS.ADCS.Operational.Downlink;
using Spectre.Console;

namespace OpcUa.Driver.ClientExample;

internal class DownlinkRequest
{
	private readonly ILogger _logger;
	private readonly IOpcUaClient _client;

	public DownlinkRequest(ILogger logger, IOpcUaClient client)
	{
		_logger = logger;
		_client = client;
	}

	public async Task Run()
	{
		DownlinkStatusMonitor();
		await SubscribeAsync();

		await Task.Run(async () =>
		{
			while (true)
			{
				var keyinfo = Console.ReadKey();

				if (keyinfo.Key == ConsoleKey.S)
				{
					CallRequestDownlink();
				}

				await Task.Delay(500);
			}
		});
	}

	public async Task SubscribeAsync()
	{
		var nodes = new List<SubscriptionNode>()
		{
			//new( "ns=2;s=Scalar_Simulation_Int32",  OnMonitoredItemNotification, "Int32 Variable"),
			//new( "ns=2;s=Scalar_Simulation_Float", OnMonitoredItemNotification, "Float Variable"),
			new( "ns=2;s=Permission", OnMonitoredItemNotification, "Permission"),
		};

		await _client.SubscribeAsync(nodes, 1000).ConfigureAwait(false);
	}

	public void CallRequestDownlink()
	{
		if (!_client.Session.Connected)
		{
			_logger.LogInformation("Session not connected!");
			return;
		}

		try
		{
			if (_notification == null) return;

			// Parent node
			var objectId = new NodeId("ns=2;s=DownlinkRequest");
			// Method node
			var methodId = new NodeId("ns=2;s=SendDownlinkRequest");
			var f = new float[] { (float)2000, (float)3000, (float)2000, (float)3000, (float)2000, (float)2000, (float)3000, (float)2000, (float)3300, };
			var inputArguments = new object[] { (UInt16)0, Convert.ToUInt16(_notification.Value.Value), (float)8, (float)6, (float)5, (Int16)(-1), f };

			_logger.LogInformation("Calling SendDownlinkRequest for node {0} ...", methodId);
			var outputArguments = _client.Session.Call(objectId, methodId, inputArguments);

			_logger.LogInformation("Method call returned {0} output argument(s):", outputArguments.Count);
			foreach (var outputArgument in outputArguments)
			{
				_logger.LogInformation("     OutputValue = {0}", outputArgument.ToString());
			}
		}
		catch (Exception ex)
		{
			_logger.LogError("Method call error: {0}", ex.Message);
		}
	}

	private Permission _permition;
	private AutoResetEvent? _DonwlinkStatusUpdated = new(false);
	private void DownlinkStatusMonitor()
	{

		//Task.Run(() => AnsiConsole.Status()
		//	.SpinnerStyle(Color.Green)
		//	.StartAsync("Downlink Status...", ctx =>
		//	{
		//		while (true)
		//		{
		//			_DonwlinkStatusUpdated.WaitOne(2000);
		//			AnsiConsole.MarkupLine("");
		//			ctx.Status($"[[Permission: {_permition}        ]]");
		//			ctx.Spinner = ctx.Spinner == Spinner.Known.Default ? Spinner.Known.Balloon : Spinner.Known.Default;
		//			ctx.SpinnerStyle = new Style(ctx.SpinnerStyle.Foreground == Color.Green ? Color.Yellow : Color.Green);
		//			//ctx.Refresh();
		//		}
		//	}));
		Task.Run(() => AnsiConsole.Progress()
			.Columns(new ProgressColumn[]
			{
				new TaskDescriptionColumn(),    // Task description
				new ProgressBarColumn(),        // Progress bar
				new PercentageColumn(),         // Percentage
				new RemainingTimeColumn(),      // Remaining time
				new SpinnerColumn(),            // Spinner
			})
			.StartAsync(async ctx =>
			{
				// Define tasks
				var task1 = ctx.AddTask("[green]Reticulating splines[/]");
				var task2 = ctx.AddTask("[green]Folding space[/]");

				while (!ctx.IsFinished)
				{
					_DonwlinkStatusUpdated.WaitOne(2000);

					// Simulate some work
					//await Task.Delay(250);

					task1.Description = $"[green][[Permission: {_permition}        ]][/]";
					// Increment
					task1.Increment(1.5);
					task2.Increment(4.5);
					//task1.RemainingTime
				}
			}));
	}

	private MonitoredItemNotification? _notification;

	/// <summary>
	/// Handle DataChange notifications from Server
	/// </summary>
	private void OnMonitoredItemNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
	{
		try
		{
			// Log MonitoredItem Notification event
			_notification = e.NotificationValue as MonitoredItemNotification;
			if ((string)monitoredItem.ResolvedNodeId.Identifier == "Permission")
			{
				var value = (Permission)Enum.Parse<Permission>(_notification!.Value!.Value.ToString());
				_permition = value;
				_DonwlinkStatusUpdated.Set();
			}
			//_logger.LogInformation("Notification: {0} \"{1}\" and Value = {2}.",
			//_notification!.Message.SequenceNumber, monitoredItem.ResolvedNodeId, value);

		}
		catch (Exception ex)
		{
			_logger.LogInformation("OnMonitoredItemNotification error: {0}", ex.Message);
		}
	}

}