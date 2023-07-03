using DWIS.EngineeringUnits;
using Xunit;

namespace DWIS.ADCS.EngineeringUnits.Tests;

public class MeasureTests
{
	[Fact()]
	public void ToUnitTest()
	{
		var a = new Measure<double, Temperature.celsius>() { Value = 20 };
		var b = a.ToUnit<Temperature.fahrenheit>();
		Assert.True(b.Value == 68, "This test needs an implementation");
	}
}