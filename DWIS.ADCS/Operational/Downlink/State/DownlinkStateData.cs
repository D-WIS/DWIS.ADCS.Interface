
namespace DWIS.ADCS.Operational.Downlink;

public class DownlinkStateData
{
	//unique identifier of downlink request
	public uint RequestedDownlinkId { get; set; }
	public Permission Permission { get; set; }
	public DownlinkStatus DownlinkStatus { get; set; }
	// 0 to 100
	public float PercentComplete { get; set; }
	public float DurationRemainingSeconds { get; set; }
}