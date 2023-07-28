namespace DWIS.Types;

/// <summary>
///  time since Unix epoch.
/// </summary>
public record Timestamp
{
	long Sec;
	ulong NanoSec;
	// todo: fix it with UTC?
	public static Timestamp Now => new();
}
