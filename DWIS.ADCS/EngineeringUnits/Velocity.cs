namespace DWIS.EngineeringUnits;

public record Velocity : IEngineeringUnit
{
	public record meters_per_second:IEngineeringUnit { }
	public record meters_per_hour:IEngineeringUnit { }
	public record feet_per_second:IEngineeringUnit { }
	public record feet_per_hour:IEngineeringUnit { }
	public record miles_per_hour:IEngineeringUnit { }
	public record kilometers_per_hour:IEngineeringUnit { }
}
