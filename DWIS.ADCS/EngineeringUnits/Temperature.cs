namespace DWIS.EngineeringUnits;

public record Temperature : IEngineeringUnit
{
	public record kelvin:Temperature { }
	public record celsius:Temperature { }
	public record fahrenheit:Temperature { }
	public record raemur:Temperature { }
	public record rankine:Temperature { }
}
