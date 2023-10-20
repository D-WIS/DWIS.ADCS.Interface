using Opc.Ua;
using OpcUa.Driver.Timer;

namespace OpcUa.Driver.Client;

public partial class OpcUaClient
{
	readonly List<MmTimer> _timers = new(); // to keep reference of timers
	public async Task ReadCyclicAsync(IEnumerable<NodeId> nodes, int interval, Action<DataValueCollection> handler, bool register = true, CancellationToken token = default)
	{
		var nds = nodes.ToArray();
		var ids = NodeIdCollection.ToNodeIdCollection(nds);
		if (register)
		{
			var resp = await Session.RegisterNodesAsync(null, ids, token).ConfigureAwait(false);
			ids = resp.RegisteredNodeIds;
		}


		var timer = new MmTimer(interval);
		_timers.Add(timer);
		timer.Elapsed += async (o, e) =>
		{
			var result = await Session.ReadValuesAsync(ids, token).ConfigureAwait(false);
			handler(result.Item1);
		};
		timer.Start();
		token.Register(() =>
		{
			timer.Stop();
			_timers.Remove(timer);
			if (register)
			{
				Session.UnregisterNodesAsync(null, ids, token);
			}
		});

	}
}