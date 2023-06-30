namespace DWIS.EngineeringUnits;

public record Pressure : IEngineeringUnit
{
	public record pascal:Pressure { }
	public record bar:Pressure { }
	public record at:Pressure { }
	public record atm:Pressure { }
	public record torr:Pressure { }
}
