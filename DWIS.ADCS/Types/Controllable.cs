using System.Reflection;
using DWIS.EngineeringUnits;

namespace DWIS.Types;

public record Controllable<T, Eu> where Eu : IEngineeringUnit, new()
{
	// todo: add this to standard?
	public T TargetValue { get; set; }

	public void SetTargetValue<Euv>(T value) where Euv : IEngineeringUnit, new()
	{
		TargetValue = ConvertUnit<Euv,Eu>(value);
	}
	// todo: add this to standard?
	public Controllable<T, Eut> ToUnit<Eut>() where Eut : IEngineeringUnit, new()
	{
		return new Controllable<T, Eut>
		{
			TargetValue = ConvertUnit<Eu, Eut>(TargetValue)
		};

	}

	private static T ConvertUnit<Euf,Eut>(T value) where Eut : IEngineeringUnit, new() where Euf : IEngineeringUnit, new()
	{
		// rely on runtime check.
		// bad C#, why not provide more type constrain ways in type parameter
		var fromEu = typeof(Euf);
		var toEu = typeof(Eut);
		if (toEu.BaseType != fromEu.BaseType)
			throw new($"can not convert {toEu} to {fromEu}");

		var toAttr = toEu.GetCustomAttribute<UnitAttribute>()!;
		var fromAttr = fromEu.GetCustomAttribute<UnitAttribute>()!;

		dynamic a = ((Convert.ToDouble(value) * fromAttr.Factor + fromAttr.Bias) - toAttr.Bias) / toAttr.Factor;
		return (T)a;
	}
}

