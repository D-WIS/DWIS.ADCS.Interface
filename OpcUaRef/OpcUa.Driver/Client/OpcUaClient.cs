using Microsoft.Extensions.Logging;
using Opc.Ua.Client;

namespace OpcUa.Driver.Client;

public interface IT : ISession
{

}

public partial class OpcUaClient: IOpcUaClient
{
	private readonly IOpcUaConnection _connection;
	private readonly ILogger _logger;
	public ISession Session => _connection.Session;

	public OpcUaClient(IOpcUaConnection connection, ILogger logger)
	{
		_connection = connection;
		_logger = logger;
	}

}