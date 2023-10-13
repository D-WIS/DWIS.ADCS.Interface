using ADCS.Interface.Share;
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

			new( "ns=2;s=RequestedDownlinkId", OnMonitoredItemNotification, "RequestedDownlinkId"),
			new( "ns=2;s=DownlinkStatus", OnMonitoredItemNotification, "DownlinkStatus"),
			new( "ns=2;s=PercentComplete", OnMonitoredItemNotification, "PercentComplete"),
			new( "ns=2;s=DurationRemainingSeconds", OnMonitoredItemNotification, "DurationRemainingSeconds"),

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
			var o = _client.Session.Call(objectId, methodId, inputArguments);

			//_logger.LogInformation("Method call returned {0} output argument(s):", outputArguments.Count);
			//foreach (var outputArgument in outputArguments)
			//{
			//	//_logger.LogInformation("     OutputValue = {0}", outputArgument.ToString());

			//}
			var status = new DownlinkStateData();
			status.RequestedDownlinkId = uint.Parse(o[0].ToString());
			status.Permission = (Permission)Enum.Parse<Permission>(o[1].ToString());
			status.DownlinkStatus = (DownlinkStatus)Enum.Parse<DownlinkStatus>(o[2].ToString());
			status.PercentComplete = float.Parse(o[3].ToString());
			status.DurationRemainingSeconds = float.Parse(o[4].ToString());

			var json = ConsoleExt.GetJsonText(status);
			AnsiConsole.Write(
				new Panel(json)
					.Header("Received Downlink Request")
					.Collapse()
					.RoundedBorder()
					.BorderColor(Color.Yellow));
		}
		catch (Exception ex)
		{
			_logger.LogError("Method call error: {0}", ex.Message);
		}
	}

	private DownlinkStateData _downlinkStateData = new DownlinkStateData();
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
				//new RemainingTimeColumn(),      // Remaining time
				new SpinnerColumn(),            // Spinner
			})
			.StartAsync(async ctx =>
			{
				// Define tasks
				var task1 = ctx.AddTask("[green]S:send[/]");
				//var task2 = ctx.AddTask("[green]Folding space[/]");

				while (!ctx.IsFinished)
				{

					_DonwlinkStatusUpdated.WaitOne(2000);

					// Simulate some work
					//await Task.Delay(250);
					var time = TimeSpan.FromSeconds(_downlinkStateData.DurationRemainingSeconds);
					task1.Description = $"[gray]S:send[/] [green][[Permission: {_downlinkStateData.Permission}; DownlinkStatus: {_downlinkStateData.DownlinkStatus}]][/] [blue]{time}[/]";
					// Increment
						var v =_downlinkStateData.PercentComplete;
						task1.Value = v;

					//task1.RemainingTime = TimeSpan.FromSeconds(_downlinkStateData.DurationRemainingSeconds);
					//task2.Increment(4.5);
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
			var id = (string)monitoredItem.ResolvedNodeId.Identifier;
			var value = _notification!.Value!.Value.ToString();
			if ( id == nameof(_downlinkStateData.Permission))
			{
				_downlinkStateData.Permission= Enum.Parse<Permission>(value);
			} else if (id == nameof(_downlinkStateData.DownlinkStatus))
			{
				_downlinkStateData.DownlinkStatus= Enum.Parse<DownlinkStatus>(value);
			} else if (id == nameof(_downlinkStateData.PercentComplete))
			{
					var v= float.Parse(value);
					_downlinkStateData.PercentComplete = v;


			}
			else if (id == nameof(_downlinkStateData.DurationRemainingSeconds))
			{
				_downlinkStateData.DurationRemainingSeconds = float.Parse(value);
			}

			_DonwlinkStatusUpdated.Set();

			//_logger.LogInformation("Notification: {0} \"{1}\" and Value = {2}.",
			//_notification!.Message.SequenceNumber, monitoredItem.ResolvedNodeId, value);

		}
		catch (Exception ex)
		{
			_logger.LogInformation("OnMonitoredItemNotification error: {0}", ex.Message);
		}
	}

}