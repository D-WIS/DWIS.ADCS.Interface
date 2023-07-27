namespace DWIS.ADCS.Operational.Downlink;

public class DownlinkRequestData
{
	public RequestStatus RequestStatus;
	public Method Method;
	public DownlinkTypes Type;
	//Duration of the downlink
	public float DurationSeconds;
	// Optional, Requested start time of downlink from receipt of message. omitted or “0” indicates immediately.
	public float DelaySeconds;
	// Optional, Requested start depth of downlink from receipt of message. omitted or “0” indicates immediately.
	public float DelayDepth;
}