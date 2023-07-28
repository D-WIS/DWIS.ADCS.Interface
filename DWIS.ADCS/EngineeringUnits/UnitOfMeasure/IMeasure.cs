using DWIS.EngineeringUnits;

namespace DWIS.ADCS.EngineeringUnits;

public interface IMeasure<T, out TUnit> where TUnit : IUnit
{
	T Value { get; set; }
	void SetValue<TValueUnit>(T value) where TValueUnit : IUnit, new();
	Measure<T, TNewUnit> ToUnit<TNewUnit>() where TNewUnit : IUnit, new();
}