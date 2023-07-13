namespace DWIS.ADCS.Operational;

public enum Permission
{
	Granted, Pending, Denied, Invalid
}

public enum DownlinkStatus
{
	Scheduled, Running, Paused, Stopped, Completed, Aborted
}

struct DownlinkStateData
{
	uint requestedDownlinkID;    //unique identifier of dowlink request
	Permission permission;
	DownlinkStatus downlinkStatus;
	float percentComplete;        // 0 to 100%
	float durationRemainingSeconds;
}

public record DownlinkState
{
	public DownlinkStateData currentDownlinkStateData { get; }

	private QoSListner Listener { get; set; }
}

enum RequestStatus
{
	New, Abort, Completed
}
struct DownlinkRequestData
{
	RequestStatus requestStatus; 
	Method method;         //{“Symbol_Script” | Symbol_Table” | “Surface_Equipment”}
	DownlinkTypes type;           //{“OnBottomFlow” | “OnBottomRotation” | “OffBottomFlow” | “OffBottomRotation” | "Other" | "None"}
	float durationSeconds;//Duration of the downlink
	float delaySeconds;   //Requested start time of downlink from receipt of message. ommitted or “0” indicates immediately.
	float delayDepth;     //Requested start depth of downlink from receipt of message. ommitted or “0” indicates immediately.
}

struct DownlinkSymbols
{
	float rampTimeMs;
	float holdTimeMs;
	float amplitude;
	string unit;
}

struct DownlinkSpecificationsData
{
	 DownlinkSymbols[]         downlinkSymbolsArray;   //Requested symbols
	 int          downlinkIndex;          //Index of desired downlink from a 2 dimensional table shared in advance (Symbol_Table method)
}

interface DownlinkRequest
{
	DownlinkRequestData downlinkRequestData;
	DownlinkSpecificationsData downlinkSpecificationsData;

	public QoSListner Listener { get; set; }
	public bool SetDownlinkSymbolTable(DownlinkSymbols[][] symbols);
	public DownlinkStateData SendDownlinkRequest(DownlinkRequestData downlinkRequest, DownlinkSpecificationsData downlinkSpecificationsData);
}
