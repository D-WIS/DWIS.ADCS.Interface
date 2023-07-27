namespace DWIS.ADCS.Operational.Downlink;

interface DownlinkRequest
{
	public DownlinkRequestData RequestData { get; }
	public DownlinkSpecificationsData SpecificationsData { get; }

	public QoSListener Listener { get; set; }
	// should we add this method?
	public bool SetDownlinkSymbolTable(DownlinkSymbols[][] symbols);
	public DownlinkStateData SendDownlinkRequest(DownlinkRequestData downlinkRequest, DownlinkSpecificationsData downlinkSpecificationsData);
}