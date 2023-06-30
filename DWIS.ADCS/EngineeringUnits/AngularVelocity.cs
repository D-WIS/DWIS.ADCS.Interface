namespace DWIS.EngineeringUnits;

public abstract record AngularVelocity : IEngineeringUnit
{
	[Unit(1)]
	public record degrees_per_second:AngularVelocity { }
	[Unit(57.29578)]
	public record radians_per_second :AngularVelocity{ }
	[Unit(360/60)]
	public record revolutions_per_minute:AngularVelocity { }
}
