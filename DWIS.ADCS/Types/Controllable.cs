using DWIS.ADCS.EngineeringUnits;
using DWIS.EngineeringUnits;

namespace DWIS.Types;

public record Controllable<TValue, TUnit> where TUnit : IUnit, new()
{
	Measure<TValue, TUnit> _measure = new ();
	// todo: add this to standard?
	public TValue TargetValue
	{
		get => _measure.Value;
		set => _measure.Value = value;
	}

	public void SetTargetValue<TValueUnit>(TValue value) where TValueUnit : IUnit, new()
	{
		_measure.SetValue<TValueUnit>(value);
	}
	// todo: add this to standard?
	public Controllable<TValue, TNewUnit> ToUnit<TNewUnit>() where TNewUnit : IUnit, new()
	{
		return new Controllable<TValue, TNewUnit>
		{
			_measure = _measure.ToUnit<TNewUnit>()
		};
	}

}

