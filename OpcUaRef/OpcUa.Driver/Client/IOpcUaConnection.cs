using Opc.Ua.Client;

namespace OpcUa.Driver.Client;

public interface IOpcUaConnection
{
    /// <summary>
    /// The session to use.
    /// The client handler may reconnect and the Session
    /// property may be updated during operation.
    /// </summary>
    ISession Session { get; }
}