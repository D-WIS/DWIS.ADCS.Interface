﻿namespace DWIS.ADCS.Operational.Downlink;

public interface IDownlinkFacade
{
	bool SetDownlinkSymbolTable(DownlinkSymbol[][] symbols);
	// would there be several downlinks in parallel
	IDownlinkState MonitorDownlinkState(uint id);
	IDownlinkRequest StartDownlinkRequest();

}