namespace DWIS.EngineeringUnits;

public abstract record Torque : IUnit
{
	[Unit(1)]
	public record newton_meters:Torque { }
	[Unit(1.356)]
	public record foot_pounds:Torque { }
	[Unit(1.356)]
	public record foot_poundals:Torque { }
	[Unit(0.113)]
	public record inch_pounds:Torque { }
	[Unit(9.80665)]
	public record meter_kilogram:Torque { }
}
