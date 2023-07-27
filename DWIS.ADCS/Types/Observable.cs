namespace DWIS.Types;

/// <summary>
/// Observable Features are variables/values that can be read and/or subscribed to. They contain several properties to provide contextual information about the value.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface Observable<T>
{
	//todo: should this be a enum?
	string QuantityClass { get; }
	Timestamp Timestamp { get; }
	StatusType Status { get; }
	T Value { get; }
}
