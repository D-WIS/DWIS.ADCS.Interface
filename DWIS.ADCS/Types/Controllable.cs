using System.Reflection;
using DWIS.ADCS.EngineeringUnits;
using DWIS.EngineeringUnits;

namespace DWIS.Types;

public record Controllable<T, Eu> where Eu : IEngineeringUnit, new()
{
	Measure<T, Eu> _measure = new ();
	// todo: add this to standard?
	public T TargetValue
	{
		get => _measure.Value;
		set => _measure.Value = value;
	}

	public void SetTargetValue<Euv>(T value) where Euv : IEngineeringUnit, new()
	{
		_measure.SetValue<Euv>(value);
	}
	// todo: add this to standard?
	public Controllable<T, Eut> ToUnit<Eut>() where Eut : IEngineeringUnit, new()
	{
		return new Controllable<T, Eut>
		{
			_measure = _measure.ToUnit<Eut>()
		};
	}

}

