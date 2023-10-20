using Opc.Ua;

namespace OpcUa.Driver.Client;

public record ReadNode(NodeId NodeId, uint Attribute = Attributes.Value) { }
