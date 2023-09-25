using Opc.Ua;
using OpcUa.Driver.Timer;

namespace OpcUa.Driver
{
	public partial class OpcUaClient
	{
		readonly List<MmTimer> _timers = new (); // to keep reference of timers
		public CancellationTokenSource ReadCyclic(IList<NodeId> nodes, int interval, Action<DataValueCollection> handler)
		{
			var cancel = new CancellationTokenSource();

			var timer = new MmTimer(interval);
			_timers.Add(timer);
			timer.Elapsed += async (o, e) =>
			{
				var result = await Session.ReadValuesAsync(nodes, cancel.Token).ConfigureAwait(false);
				handler(result.Item1);
			};
			timer.Start();
			cancel.Token.Register(()=>
			{
				timer.Stop();
				_timers.Remove(timer);
			});

			return cancel;
		}
	}
}