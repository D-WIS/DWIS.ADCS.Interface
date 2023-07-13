namespace DWIS.EngineeringUnits;

public abstract record Energy : IUnit
{
	[Unit(1)]
	public record joule : Energy { }
}