namespace DWIS.EngineeringUnits;

public abstract record Velocity : IUnit
{
	[Unit(1)]
	public record meters_per_second : IUnit { }
	[Unit(3600)]
	public record meters_per_hour : IUnit { }
	[Unit(0.3048)]
	public record feet_per_second : IUnit { }
	[Unit(1_097.28)]
	public record feet_per_hour : IUnit { }
	[Unit(0.447)]
	public record miles_per_hour : IUnit { }
	[Unit(0.277778)]
	public record kilometers_per_hour : IUnit { }
}
