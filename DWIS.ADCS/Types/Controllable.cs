namespace DWIS.Types;
using EngineeringUnit = System.Enum;

public record struct Controllable<T, Eu>(Eu eu) where Eu : EngineeringUnit
{
	void setTargetValue(T value){}
	public void setTargetValue(T value, Eu unit){}
}
