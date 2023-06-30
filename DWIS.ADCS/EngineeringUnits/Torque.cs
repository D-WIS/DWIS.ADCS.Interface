namespace DWIS.EngineeringUnits;

public record Torque : IEngineeringUnit
{
	public record newton_meters:Torque { }
	public record foot_pounds:Torque { }
	public record foot_poundals:Torque { }
	public record inch_pounds:Torque { }
	public record meter_kilogram:Torque { }
}
