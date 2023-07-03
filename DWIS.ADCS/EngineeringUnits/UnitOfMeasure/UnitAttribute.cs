namespace DWIS.EngineeringUnits;

[System.AttributeUsage(AttributeTargets.Class)]
public class UnitAttribute : Attribute
{
	public double Factor;
	public readonly double Bias;

	public UnitAttribute(double factor, double bias = 0)
	{
		Factor = factor;
		Bias = bias;
	}
}