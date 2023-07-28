using DWIS.EngineeringUnits;

namespace DWIS.Types;

/// <summary>
/// Observable Features are variables/values that can be read and/or subscribed to. They contain several properties to provide contextual information about the value.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IObservable<TValue, TUnit> : IMeasure<TValue, TUnit> where TUnit: IUnit, new()
{
	//string QuantityClass { get; }
	string Unit { get; }
	Timestamp Timestamp { get; }
	StatusType Status { get; }
	TValue Value { get; }
}