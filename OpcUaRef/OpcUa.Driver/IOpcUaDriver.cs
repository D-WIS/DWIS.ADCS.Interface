using Opc.Ua;
using OpcUa.Driver.Client;

namespace OpcUa.Driver;

public interface IOpcUaDriver
{
	Task Init(string appName, string? password = null, bool renewCertificate = false,
		string? reverseConnectUrlString = null);
	Task<IOpcUaClient> Connect(string serverUrl, bool autoAccept = true, bool useSecurity = true,
		string? username = null, string? userPassword = null, CancellationToken token = default);

}