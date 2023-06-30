namespace DWIS.Types;

public interface Observable<T>
{
	//todo: should this be a enum?
	string QuantityClass { get; }
	Timestamp Timestamp { get; }
	StatusType Status { get; }
	T Value { get; }
}
