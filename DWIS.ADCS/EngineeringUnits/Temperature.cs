namespace DWIS.EngineeringUnits;

public record Temperature : IEngineeringUnit
{
	[Unit(1)]
	public record celsius : Temperature { }
	[Unit(1, -273.15)]
	public record kelvin:Temperature { }
	[Unit(5/9, -5*32/9)]
	public record fahrenheit:Temperature { }
	[Unit(1.25)]
	public record raemur:Temperature { }
	[Unit(5/9, -273.15)]
	public record rankine:Temperature { }
}
