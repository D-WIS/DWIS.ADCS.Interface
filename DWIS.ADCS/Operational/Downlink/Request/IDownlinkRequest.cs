namespace DWIS.ADCS.Operational.Downlink;

/// <summary>
/// Type of Interface: Ephemeral
/// Interface is guaranteed to exist on all D-WIS compliant systems.
/// Interface is used to request a downlink.
/// It contains enough information so the ADCS can display the request to the Driller and usually to wait for his approval.
/// </summary>
interface IDownlinkRequest
{
	public DownlinkRequestData RequestData { get; }
	public DownlinkSpecificationsData SpecificationsData { get; }

	public ADCS Listener { get; set; }
	// should we add this method?
	public bool SetDownlinkSymbolTable(DownlinkSymbol[][] symbols);
	public DownlinkStateData SendDownlinkRequest(DownlinkRequestData downlinkRequest, DownlinkSpecificationsData downlinkSpecificationsData);
}