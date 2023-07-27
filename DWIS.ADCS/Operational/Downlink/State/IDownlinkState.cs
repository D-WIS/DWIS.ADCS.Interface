namespace DWIS.ADCS.Operational.Downlink;

/// <summary>
/// Type of Interface: Persistent
/// Interface is guaranteed to exist on all D-WIS compliant systems.
/// Interface is used to observe the current operational state of the downlink requests.
/// </summary>
public interface IDownlinkState
{
	public DownlinkStateData CurrentDownlinkStateData { get; }

	public ADCS Listener { get; set; }
}