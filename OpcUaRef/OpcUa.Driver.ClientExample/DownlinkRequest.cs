using Microsoft.Extensions.Logging;
using Opc.Ua.Client;
using Opc.Ua;
using OpcUa.Driver;
using DWIS.ADCS.Operational.Downlink;

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

	private MonitoredItemNotification notification;
	public void CallRequestDownlink()
	{
		if (!_client.Session.Connected)
		{
			_logger.LogInformation("Session not connected!");
			return;
		}

		try
		{
			if (notification == null) return;
			// Parent node
			var objectId = new NodeId("ns=2;s=DownlinkRequest");
			// Method node
			var methodId = new NodeId("ns=2;s=SendDownlinkRequest");
			var f = new float[] { (float)2, (float)3.0, (float)2, (float)3.0, (float)2, (float)3.0, (float)3.0, (float)2, (float)3.0, };
			var inputArguments = new object[] { (UInt16)0, Convert.ToUInt16(notification.Value.Value), (float)13, (float)23, (float)33, (Int16)(-1), f };

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

	/// <summary>
	/// Handle DataChange notifications from Server
	/// </summary>
	private void OnMonitoredItemNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
	{
		try
		{
			// Log MonitoredItem Notification event
			notification = e.NotificationValue as MonitoredItemNotification;
			object value = notification.Value;
			if ((string)monitoredItem.ResolvedNodeId.Identifier == "Permission")
			{
				value = Enum.Parse<Permission>(notification!.Value!.Value.ToString());

			}
			//_logger.LogInformation("Notification: {0} \"{1}\" and Value = {2}.",
			//notification!.Message.SequenceNumber, monitoredItem.ResolvedNodeId, value);

		}
		catch (Exception ex)
		{
			_logger.LogInformation("OnMonitoredItemNotification error: {0}", ex.Message);
		}
	}

}