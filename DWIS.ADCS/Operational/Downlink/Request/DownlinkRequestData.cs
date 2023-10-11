
namespace DWIS.ADCS.Operational.Downlink;

public class DownlinkRequestData
{
	public Method Method { get; set; }
	public DownlinkTypes Type { get; set; }
	// Duration of the downlink
	public float DurationSeconds { get; set; }
	// Optional, Requested start time of downlink from receipt of message. omitted or “0” indicates immediately.
	public float DelaySeconds { get; set; }
	// Optional, Requested start depth of downlink from receipt of message. omitted or “0” indicates immediately.
	public float DelayDepth { get; set; }
	// Index of desired downlink from a 2 dimensional Symbol_Table shared in advance, i.e., shared via a file in USB disk.
	// -1, if this we do not use this way
	public Int16 DownlinkIndex { get; set; }
	// Requested symbols
	public DownlinkSymbol[] DownlinkSymbolsArray { get; set; }
}