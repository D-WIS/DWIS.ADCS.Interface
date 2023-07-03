using DWIS.EngineeringUnits;
using System.Reflection;

namespace DWIS.ADCS.EngineeringUnits;

public record Measure<T, U> where U : IEngineeringUnit, new()
{
	public T Value { get; set; }

	public void SetValue<Uv>(T value) where Uv : IEngineeringUnit, new()
	{
		Value = ConvertUnit<Uv, U>(value);
	}

	public Measure<T, Ut> ToUnit<Ut>() where Ut : IEngineeringUnit, new()
	{
		return new Measure<T, Ut>
		{
			Value = ConvertUnit<U, Ut>(Value)
		};

	}
	private static T ConvertUnit<Uf, Ut>(T value) where Ut : IEngineeringUnit, new() where Uf : IEngineeringUnit, new()
	{
		// rely on runtime check.
		// bad C#, why not provide more type constrain ways in type parameter
		var fromEu = typeof(Uf);
		var toEu = typeof(Ut);
		if (toEu.BaseType != fromEu.BaseType)
			throw new($"can not convert {toEu} to {fromEu}");

		var toAttr = toEu.GetCustomAttribute<UnitAttribute>()!;
		var fromAttr = fromEu.GetCustomAttribute<UnitAttribute>()!;

		dynamic a = ((Convert.ToDouble(value) * fromAttr.Factor + fromAttr.Bias) - toAttr.Bias) / toAttr.Factor;
		return (T)a;
	}
}
