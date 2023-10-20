using Opc.Ua;
using OpcUa.Driver.Timer;

namespace OpcUa.Driver.Client;

public partial class OpcUaClient
{
	readonly List<MmTimer> _timers = new (); // to keep reference of timers
	public async Task ReadCyclicAsync(IEnumerable<NodeId> nodes, int interval, Action<DataValueCollection> handler, CancellationToken token = default)
	{
		var timer = new MmTimer(interval);
		_timers.Add(timer);
		timer.Elapsed += async (o, e) =>
		{
			var result = await Session.ReadValuesAsync(nodes.ToList(), token).ConfigureAwait(false);
			handler(result.Item1);
		};
		timer.Start();
		token.Register(()=>
		{
			timer.Stop();
			_timers.Remove(timer);
		});

	}
}