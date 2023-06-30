types in https://github.com/D-WIS/ADCSSoftwareInterface/blob/main/ADCSSoftwareInterface.md#59-units


## Unit Converion Design

> valueTo = ValueFrom * Factor + Bias

Unit attribute applied on unit i.e.,
```
public abstract record Length : IEngineeringUnit
{
	[Unit(1)]
	public record meter: Length
	{
	}

	[Unit(0.01)]
	public record centimeter : Length
	{
	}

	[Unit(0.0254)]
	public record inch : Length
	{
	}
```

the factor on `inch` is from inch to meter
the factor on `centimeter` is from contimenter to meter

> so to converter from inch to centimeter: we will do `inch -> meter-> centimeter`:
> ((value * 0.254 + 0) - 0)/0.01
