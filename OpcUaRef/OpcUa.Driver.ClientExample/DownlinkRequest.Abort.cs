using ADCS.Interface.Share;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Spectre.Console;

namespace OpcUa.Driver.ClientExample;

internal partial class DownlinkRequest
{
	public void CallAbortDownlinkRequest()
	{
		var requestedDownlinkId = _DownlinkStateData.RequestedDownlinkId;
		if (requestedDownlinkId == 0) // valid value starts from 1
		{
			_logger.LogError("No Active Downlink to Abort!");
			return;
		}

		// Parent node
		var objectId = new NodeId("ns=2;s=DownlinkRequest");
		// Method node
		var methodId = new NodeId("ns=2;s=AbortDownlinkRequest");
		var inputArguments = new object[]
		{
			requestedDownlinkId
		};
		var o = CallMethod(objectId, methodId, inputArguments);
		var status = ParseDownlinkStateData(o);

		var json = ConsoleExt.GetJsonText(status);
		AnsiConsole.Write(
			new Panel(json)
				.Header("Received Response of Abort Downlink")
				.Collapse()
				.RoundedBorder()
				.BorderColor(Color.Yellow));
	}
}