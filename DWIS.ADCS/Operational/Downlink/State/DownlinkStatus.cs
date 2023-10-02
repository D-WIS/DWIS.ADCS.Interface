namespace DWIS.ADCS.Operational.Downlink;

public enum DownlinkStatus : UInt16
{
	Scheduled, Running, Paused, Stopped, Completed, Aborted
}