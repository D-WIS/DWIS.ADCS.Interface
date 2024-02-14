using ADCS.Interface.Share;
using Opc.Ua;
using Spectre.Console;

namespace OpcUa.Driver.ClientExample;

internal partial class DownlinkRequest
{
	public void CallRequestDownlink()
	{
		// Parent node
		var objectId = new NodeId("ns=2;s=DownlinkRequest");
		// Method node
		var methodId = new NodeId("ns=2;s=SendDownlinkRequest");
		// Arguments
		var f = new float[]
		{
			(float)2000, (float)3000, (float)2000, (float)3000, (float)2000, (float)2000, (float)3000, (float)2000,
			(float)3300,
		};
		var inputArguments = new object[]
			{ (UInt16)0, Convert.ToUInt16(_notification.Value.Value), (float)8, (float)6, (float)5, (Int16)(-1), f };
		var o = CallMethod(objectId, methodId, inputArguments);

		var status = ParseDownlinkStateData(o);

		var json = ConsoleExt.GetJsonText(status);
		AnsiConsole.Write(
			new Panel(json)
				.Header("Received Response of Downlink Request")
				.Collapse()
				.RoundedBorder()
				.BorderColor(Color.Yellow));
	}
}