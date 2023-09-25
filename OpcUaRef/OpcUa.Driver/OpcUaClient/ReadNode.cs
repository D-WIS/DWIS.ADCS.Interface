using Opc.Ua;

namespace OpcUa.Driver;

public record ReadNode(NodeId NodeId, uint Attribute = Attributes.Value) { }
