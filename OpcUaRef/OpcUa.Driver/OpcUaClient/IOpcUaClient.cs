using Opc.Ua;
using Opc.Ua.Client;

namespace OpcUa.Driver;

public partial interface IOpcUaClient
{
	ISession Session { get; }

	Task SubscribeAsync(IEnumerable<SubscriptionNode> nodes, int publishingInterval);
	CancellationTokenSource ReadCyclic(IList<NodeId> nodes, int interval, Action<DataValueCollection> handler);

}
