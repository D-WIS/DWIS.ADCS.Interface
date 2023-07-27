using DWIS.ADCS.EngineeringUnits;
using DWIS.EngineeringUnits;

namespace DWIS.Types;

/// <summary>
/// Controllable Features inherit all properties of Observable Features and should have a Target suffix.
/// A Controllable Feature must have a method to set the Value.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TUnit"></typeparam>
public class Controllable<TValue, TUnit> where TUnit : IUnit, new()
{
	Measure<TValue, TUnit> _measure = new ();
	// todo: add this to standard?
	public TValue TargetValue
	{
		get => _measure.Value;
		set => _measure.Value = value;
	}

	/// <summary>
	/// This synchronous method is used to set the value(s) of a Controllable Feature. The result, success or fail, is returned. An example of an unsuccessful method call would be a call when the application does not have control of a ControlGroup that contains the Controllable Feature.
	/// </summary>
	/// <typeparam name="TValueUnit"></typeparam>
	/// <param name="value"></param>
	/// <returns></returns>
	public StatusType SetTargetValue<TValueUnit>(TValue value) where TValueUnit : IUnit, new()
	{
		_measure.SetValue<TValueUnit>(value);
		return StatusType.Good;
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

