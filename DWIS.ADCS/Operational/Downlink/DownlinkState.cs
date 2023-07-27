namespace DWIS.ADCS.Operational.Downlink;

public class DownlinkState
{
	public DownlinkStateData currentDownlinkStateData { get; }

	private QoSListener Listener { get; set; }
}