namespace DWIS.ADCS.Operational.Downlink;

public interface IDownlinkState
{
	public DownlinkStateData CurrentDownlinkStateData { get; }

	public IQoSListener Listener { get; set; }
}