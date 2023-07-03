namespace DWIS.EngineeringUnits;

public abstract record Power : IEngineeringUnit
{
	[Unit(1)]
	public record watt:Power { }
	[Unit(745.6999)]
	public record horsepower :Power{ }
}