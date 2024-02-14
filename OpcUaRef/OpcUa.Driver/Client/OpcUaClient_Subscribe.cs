using Microsoft.Extensions.Logging;
using Opc.Ua.Client;
using Opc.Ua;

namespace OpcUa.Driver.Client;

public partial class OpcUaClient: IOpcUaClient
{
	/// <summary>
	/// Subscribe to all variables in the list.
	/// </summary>
	/// <param name="nodes">The variables to subscribe.</param>
	/// <param name="publishingInterval"></param>
	/// <param name="fastDataChangeNotification"></param>
	/// <returns></returns>
	public async Task SubscribeAsync(
		IEnumerable<SubscriptionNode> nodes,
		int publishingInterval,
		CancellationToken token = default
	)
	{
		if (!Session.Connected)
		{
			_logger.LogError("Session not connected!");
			return;
		}

		var samplingInterval = publishingInterval;
		uint queueSize = 10;
		// The life time of of the subscription in counts of publish interval.
		// LifetimeCount shall be at least 3*KeepAliveCount.
		uint lifetimeCount = 12; //0
		uint keepAliveCount = 2;

		try
		{
			// Create a subscription for receiving data change notifications

			// test for deferred ack of sequence numbers
			//session.PublishSequenceNumbersToAcknowledge += DeferSubscriptionAcknowledge;

			// set a minimum amount of three publish requests per session
			Session.MinPublishRequestCount = 3;

			// Define Subscription parameters
			var subscription = new Subscription(Session.DefaultSubscription)
			{
				DisplayName = "Client Subscription",
				PublishingEnabled = true,
				PublishingInterval = publishingInterval,
				LifetimeCount = lifetimeCount,
				KeepAliveCount = keepAliveCount,
				//SequentialPublishing = true,
				//RepublishAfterTransfer = true,
				// if enable,  monitoredItem.Notification will not be triggered
				//DisableMonitoredItemCache = true,
				MaxNotificationsPerPublish = 1000,
				MinLifetimeInterval = (uint)Session.SessionTimeout,
				//FastDataChangeCallback = FastDataChangeNotification,
				FastKeepAliveCallback = FastKeepAliveNotification,
			};
			Session.AddSubscription(subscription);

			// Create the subscription on Server side
			await subscription.CreateAsync(token).ConfigureAwait(false);
			_logger.LogInformation("New Subscription created with SubscriptionId = {0}.", subscription.Id);

			// Create MonitoredItems for data changes
			foreach (var item in nodes)
			{
				var monitoredItem = new MonitoredItem(subscription.DefaultItem)
				{
					StartNodeId = item.NodeId,
					AttributeId = Attributes.Value,
					SamplingInterval = samplingInterval,
					DisplayName = item.DisplayName,
					QueueSize = queueSize,
					DiscardOldest = true,
					// CacheQueueSize = 
					//MonitoringMode = MonitoringMode.Reporting,
				};
				monitoredItem.Notification += item.ValueChangeHandler;

				subscription.AddItem(monitoredItem);
				//if (sub.CurrentKeepAliveCount > 1000) break;
			}

			// Create the monitored items on Server side
			await subscription.ApplyChangesAsync(token);
			_logger.LogInformation("MonitoredItems {0} created for SubscriptionId = {1}.", subscription.MonitoredItemCount, subscription.Id);

			token.Register(async () =>
			{
				await subscription.DeleteAsync(true).ConfigureAwait(false);
				subscription?.Dispose();
				subscription = null;
				_logger.LogInformation("Subscription is deleted.");
			});
		}
		catch (Exception ex)
		{
			_logger.LogError("Subscribe error: {0}", ex.Message);
		}
	}

	///// <summary>
	///// Event handler to defer publish response sequence number acknowledge.
	///// </summary>
	//private void DeferSubscriptionAcknowledge(ISession session, PublishSequenceNumbersToAcknowledgeEventArgs e)
	//{
	//	// for testing keep the latest sequence numbers for a while
	//	const int ackDelay = 5;
	//	if (e.AcknowledgementsToSend.Count > 0)
	//	{
	//		// defer latest sequence numbers
	//		var deferredItems = e.AcknowledgementsToSend.OrderByDescending(s => s.SequenceNumber).Take(ackDelay).ToList();
	//		e.DeferredAcknowledgementsToSend.AddRange(deferredItems);
	//		foreach (var deferredItem in deferredItems)
	//		{
	//			e.AcknowledgementsToSend.Remove(deferredItem);
	//		}
	//	}
	//}

	/// <summary>
	/// The fast keep alive notification callback.
	/// </summary>
	private void FastKeepAliveNotification(Subscription subscription, NotificationData notification)
	{
		try
		{
			//_logger.LogInformation("Keep Alive  : Id={0} PublishTime={1} SequenceNumber={2}.",subscription.Id, notification.PublishTime, notification.SequenceNumber);
		}
		catch (Exception ex)
		{
			_logger.LogError("FastKeepAliveNotification error: {0}", ex.Message);
		}
	}

	/// <summary>
	/// The fast data change notification callback.
	/// </summary>
	//private void FastDataChangeNotification(Subscription subscription, DataChangeNotification notification, IList<string> stringTable)
	//{
	//	try
	//	{
	//		_logger.LogInformation("Notification: Id={0} PublishTime={1} SequenceNumber={2} Items={3}.",
	//			subscription.Id, notification.PublishTime,
	//			notification.SequenceNumber, notification.MonitoredItems.Count);
	//	}
	//	catch (Exception ex)
	//	{
	//		_logger.LogInformation("FastDataChangeNotification error: {0}", ex.Message);
	//	}
	//}

}