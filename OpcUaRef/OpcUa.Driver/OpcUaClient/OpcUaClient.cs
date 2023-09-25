using Microsoft.Extensions.Logging;
using Opc.Ua.Client;

namespace OpcUa.Driver;

public interface IT : ISession
{

}

public partial class OpcUaClient: IOpcUaClient
{
	private readonly ILogger _logger;
	public ISession Session { get; }

	public OpcUaClient(IOpcUaConnection connection, ILogger logger)
	{
		_logger = logger;
		Session = connection.Session;
	}

}