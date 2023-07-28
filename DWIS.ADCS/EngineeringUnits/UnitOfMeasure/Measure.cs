using DWIS.EngineeringUnits;
using System.Reflection;

namespace DWIS.ADCS.EngineeringUnits;

public record Measure<T, TUnit> : IMeasure<T,TUnit> where TUnit : IUnit, new()
{
	public Measure()
	{
		
	}
	public Measure(T value)
	{
		Value = value;
	}
	public T Value { get; set; }

	public void SetValue<TValueUnit>(T value) where TValueUnit : IUnit, new()
	{
		Value = ConvertUnit<TValueUnit, TUnit>(value);
	}

	public Measure<T, TNewUnit> ToUnit<TNewUnit>() where TNewUnit : IUnit, new()
	{
		return new Measure<T, TNewUnit>
		{
			Value = ConvertUnit<TUnit, TNewUnit>(Value)
		};

	}

	private static T ConvertUnit<TUnitFrom, TUnitTo>(T value) where TUnitTo : IUnit, new() where TUnitFrom : IUnit, new()
	{
		// rely on runtime check.
		// bad C#, why not provide more type constrain ways in type parameter
		var fromEu = typeof(TUnitFrom);
		var toEu = typeof(TUnitTo);
		if (toEu.BaseType != fromEu.BaseType)
			throw new($"can not convert {toEu} to {fromEu}");

		if(fromEu == toEu) return value;

		var toAttr = toEu.GetCustomAttribute<UnitAttribute>()!;
		var fromAttr = fromEu.GetCustomAttribute<UnitAttribute>()!;

		dynamic a = ((Convert.ToDouble(value) * fromAttr.Factor + fromAttr.Bias) - toAttr.Bias) / toAttr.Factor;
		return (T)a;
	}
}
