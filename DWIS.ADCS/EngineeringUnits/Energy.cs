namespace DWIS.EngineeringUnits;

public record Energy : IEngineeringUnit
{
	[Unit(1)]
	public record joule : Energy { }
}