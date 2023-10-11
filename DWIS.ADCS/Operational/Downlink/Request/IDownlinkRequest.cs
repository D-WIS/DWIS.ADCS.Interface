namespace DWIS.ADCS.Operational.Downlink;

/// <summary>
/// Type of Interface: Ephemeral
/// Interface is guaranteed to exist on all D-WIS compliant systems.
/// Interface is used to request a downlink.
/// It contains enough information so the ADCS can display the request to the Driller and usually to wait for his approval.
/// </summary>
public interface IDownlinkRequest
{
	public DownlinkRequestData RequestData { get; }

	public IQoSListener<DownlinkStateData> Listener { get; set; }

	public DownlinkStateData SendDownlinkRequest(DownlinkRequestData downlinkRequest);
	public DownlinkStateData Abort(uint requestedDownlinkID);
	public DownlinkStateData Complete(uint requestedDownlinkID);
}