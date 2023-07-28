types in https://github.com/D-WIS/ADCSSoftwareInterface/blob/main/ADCSSoftwareInterface.md#59-units

## Unit Of Measure Design

> valueTo = ValueFrom * Factor + Bias

Unit attribute applied on unit i.e.,
```
// an abstract record Dimension: Length
public abstract record Length : IUnit
{
	// an unit derived from the Dimension: Length
	// we chose the SI unit as the base of all the units in the Dimension
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
