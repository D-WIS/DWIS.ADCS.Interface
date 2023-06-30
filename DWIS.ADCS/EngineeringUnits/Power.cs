namespace DWIS.EngineeringUnits;

public record Power : IEngineeringUnit
{
	public record watt:Power { }
	public record joule:Power { }
	public record horsepower :Power{ }
}
