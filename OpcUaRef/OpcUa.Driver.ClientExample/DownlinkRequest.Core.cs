using Microsoft.Extensions.Logging;
using Opc.Ua.Client;
using Opc.Ua;
using DWIS.ADCS.Operational.Downlink;
using OpcUa.Driver.Client;
using Spectre.Console;

namespace OpcUa.Driver.ClientExample;

internal partial class DownlinkRequest
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
		await SubscribeNotificationAsync();
		await Task.Run(async () =>
		{
			while (true)
			{
				var keyinfo = Console.ReadKey();

				if (keyinfo.Key == ConsoleKey.S)
				{
					CallRequestDownlink();
				}
				else if (keyinfo.Key == ConsoleKey.A)
				{
					// abort the last active downlink request
					CallAbortDownlinkRequest();
				}

				await Task.Delay(500);
			}
		});
	}

	public async Task SubscribeNotificationAsync()
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

	private static DownlinkStateData ParseDownlinkStateData(IList<object> o)
	{
		var status = new DownlinkStateData();
		status.RequestedDownlinkId = uint.Parse(o[0].ToString());
		status.Permission = (Permission)Enum.Parse<Permission>(o[1].ToString());
		status.DownlinkStatus = (DownlinkStatus)Enum.Parse<DownlinkStatus>(o[2].ToString());
		status.PercentComplete = float.Parse(o[3].ToString());
		status.DurationRemainingSeconds = float.Parse(o[4].ToString());
		return status;
	}

	public IList<object> CallMethod(NodeId parentNodeId, NodeId methodId, object[] args)
	{
		if (!_client.Session.Connected)
		{
			var msg = "Session not connected!";
			_logger.LogInformation(msg);
			throw new Exception(msg);
		}

		try
		{
			if (_notification == null) return null;
			var msg = $"Calling Method: {methodId} for node {parentNodeId} ...";
			_logger.LogInformation(msg);
			var o = _client.Session.Call(parentNodeId, methodId, args);
			return o;

		}
		catch (Exception ex)
		{
			_logger.LogError("Method call error: {0}", ex.Message);
			throw;
		}
	}

	private DownlinkStateData _DownlinkStateData = new();
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
			.Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new SpinnerColumn())
			.StartAsync(async ctx =>
			{
				// Define tasks
				var task1 = ctx.AddTask("[green]S:send;A:abort[/]");
				//var task2 = ctx.AddTask("[green]Folding space[/]");

				while (!ctx.IsFinished)
				{
					_DonwlinkStatusUpdated.WaitOne(2000);

					// Simulate some work
					//await Task.Delay(250);
					var time = TimeSpan.FromSeconds(_DownlinkStateData.DurationRemainingSeconds);
					task1.Description = $"[gray]S:send;A:abort[/] [green][[Permission: {_DownlinkStateData.Permission}; DownlinkStatus: {_DownlinkStateData.DownlinkStatus}]][/] [blue]{time}[/]";
					// Increment
					var v = _DownlinkStateData.PercentComplete;
					task1.Value = v;

					//task1.RemainingTime = TimeSpan.FromSeconds(_DownlinkStateData.DurationRemainingSeconds);
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
			if (id == nameof(_DownlinkStateData.Permission))
			{
				_DownlinkStateData.Permission = Enum.Parse<Permission>(value);
			}
			else if (id == nameof(_DownlinkStateData.DownlinkStatus))
			{
				_DownlinkStateData.DownlinkStatus = Enum.Parse<DownlinkStatus>(value);
			}
			else if (id == nameof(_DownlinkStateData.PercentComplete))
			{
				var v = float.Parse(value);
				_DownlinkStateData.PercentComplete = v;
			}
			else if (id == nameof(_DownlinkStateData.DurationRemainingSeconds))
			{
				_DownlinkStateData.DurationRemainingSeconds = float.Parse(value);
			}
			else if (id == nameof(_DownlinkStateData.RequestedDownlinkId))
			{
				_DownlinkStateData.RequestedDownlinkId = UInt32.Parse(value);
			}
			else
			{
				throw new Exception("unhandled OnMonitoredItemNotification ");
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