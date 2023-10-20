using Opc.Ua;
using Opc.Ua.Client;

namespace OpcUa.Driver.Client;

public partial interface IOpcUaClient
{
	ISession Session { get; }

	Task SubscribeAsync(IEnumerable<SubscriptionNode> nodes, int publishingInterval, CancellationToken token = default);
	Task ReadCyclicAsync(IEnumerable<NodeId> nodes, int interval, Action<DataValueCollection> handler, bool register = true, CancellationToken token = default);

}
