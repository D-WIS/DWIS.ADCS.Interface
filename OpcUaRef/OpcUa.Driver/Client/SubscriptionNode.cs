using Opc.Ua;
using Opc.Ua.Client;

namespace OpcUa.Driver.Client;

public record SubscriptionNode(NodeId NodeId, MonitoredItemNotificationEventHandler ValueChangeHandler, string? DisplayName = null) { }

public static class SubscriptionNodeExt
{
	public static List<SubscriptionNode> ToSubscriptionNodeList(this NodeCollection collection, MonitoredItemNotificationEventHandler valueChangeHandler) => collection.Select(n => new SubscriptionNode(n.NodeId.ToString(), valueChangeHandler, n.DisplayName.Text ?? n.BrowseName.Name)).ToList();
}