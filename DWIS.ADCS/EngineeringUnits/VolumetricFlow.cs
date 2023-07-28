namespace DWIS.EngineeringUnits;

public abstract record VolumetricFlow : IUnit
{
	[Unit(1)]
	public record cubic_meters_per_second : VolumetricFlow
	{
	}

	[Unit(0.001)]
	public record liters_per_second : VolumetricFlow
	{
	}

	[Unit(0.003785)]
	public record gallons_us_per_second : VolumetricFlow
	{
	}
}