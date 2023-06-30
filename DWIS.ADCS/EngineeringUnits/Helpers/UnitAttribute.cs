namespace DWIS.EngineeringUnits;
[System.AttributeUsage(System.AttributeTargets.Class)
]
public class UnitAttribute : System.Attribute
{
	public double Factor;
	public readonly double Bias;

	public UnitAttribute(double factor, double bias = 0)
	{
		Factor = factor;
		Bias = bias;
	}
}