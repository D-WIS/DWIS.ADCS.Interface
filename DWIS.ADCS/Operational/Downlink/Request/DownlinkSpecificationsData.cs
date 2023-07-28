namespace DWIS.ADCS.Operational.Downlink;

public class DownlinkSpecificationsData
{
	// Requested symbols
	public DownlinkSymbol[]? DownlinkSymbolsArray { get; set; }
	//Index of desired downlink from a 2 dimensional table shared in advance (Symbol_Table method)
	// -1, if this we do not use this way
	public int DownlinkIndex { get; set; }
}