namespace DWIS.ADCS.Operational.Downlink;

struct DownlinkSpecificationsData
{
	DownlinkSymbols[] downlinkSymbolsArray;   //Requested symbols
	int downlinkIndex;          //Index of desired downlink from a 2 dimensional table shared in advance (Symbol_Table method)
}