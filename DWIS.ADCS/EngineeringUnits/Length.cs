namespace DWIS.EngineeringUnits;

public abstract record Length : IUnit
{
	[Unit(1)]
	public record meter: Length
	{
	}

	[Unit(0.3048)]
	public record feet : Length
	{
	}

	[Unit(0.0254)]
	public record inch : Length
	{
	}

	[Unit(0.01)]
	public record centimeter : Length
	{
	}

	[Unit(0.001)]
	public record millimeter : Length
	{
	}

	[Unit(1_609.344)]
	public record mile : Length
	{
	}

	[Unit(0.9144)]
	public record yard : Length
	{
	}
}
