namespace DWIS.ADCS.Operational.Downlink;

public class DownlinkStateData
{
	//unique identifier of downlink request
	public uint RequestedDownlinkId;    
	public Permission Permission;
	public DownlinkStatus DownlinkStatus;
	// 0 to 100%
	public float PercentComplete;        
	public float DurationRemainingSeconds;
}