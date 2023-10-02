
namespace DWIS.ADCS.Operational.Downlink;

public class DownlinkRequestData
{
	public Method Method;
	public DownlinkTypes Type;
	// Duration of the downlink
	public float DurationSeconds;
	// Optional, Requested start time of downlink from receipt of message. omitted or “0” indicates immediately.
	public float DelaySeconds;
	// Optional, Requested start depth of downlink from receipt of message. omitted or “0” indicates immediately.
	public float DelayDepth;
	// Index of desired downlink from a 2 dimensional Symbol_Table shared in advance, i.e., shared via a file in USB disk.
	public UInt16 DownlinkIndex;
	// Requested symbols
	public DownlinkSymbol[] DownlinkSymbolsArray;
}