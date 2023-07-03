namespace DWIS.EngineeringUnits;

public abstract record Energy : IEngineeringUnit
{
	[Unit(1)]
	public record joule : Energy { }
}