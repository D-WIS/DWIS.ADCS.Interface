using Opc.Ua;
using Opc.Ua.Client;

namespace OpcUa.Driver;

public partial interface IOpcUaClient
{
	ISession Session { get; }

	Task SubscribeAsync(IEnumerable<SubscriptionNode> nodes, int publishingInterval);
}
