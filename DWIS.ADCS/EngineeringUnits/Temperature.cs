namespace DWIS.EngineeringUnits;

public abstract record Temperature : IEngineeringUnit
{
	[Unit(1)]
	public record celsius : Temperature { }
	[Unit(1, -273.15)]
	public record kelvin:Temperature { }
	[Unit(5.0/9, -5.0*32/9)]
	public record fahrenheit:Temperature { }
	[Unit(1.25)]
	public record raemur:Temperature { }
	[Unit(5.0/9, -273.15)]
	public record rankine:Temperature { }
}
