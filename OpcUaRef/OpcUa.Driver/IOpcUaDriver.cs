using Opc.Ua;

namespace OpcUa.Driver;

public interface IOpcUaDriver
{
	Task<IOpcUaClient> Connect(string serverUrl, bool autoAccept = true, bool useSecurity = true,
		string? username = null, string? userPassword = null, CancellationToken token = default);

}