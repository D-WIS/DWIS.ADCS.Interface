namespace DWIS.EngineeringUnits;

public abstract record Pressure : IUnit
{
	[Unit(1)]
	public record pascal:Pressure { }
	[Unit(100_000)]
	public record bar:Pressure { }
	[Unit(101_325)]
	public record at:Pressure { }
	[Unit(101_325)]
	public record atm:Pressure { }
	[Unit(133.322)]
	public record torr:Pressure { }
}
