using DWIS.EngineeringUnits;
using Xunit;

namespace DWIS.Types.Tests;

public class ControllableTests
{
	[Fact()]
	public void setTargetValueTest()
	{
		// none unit
		var u = new Controllable<bool, UnitLess>();
		u.TargetValue = true;
		Assert.True(u.TargetValue);

		var s = new Controllable<double, Length.meter>();
		s.SetTargetValue<Length.centimeter>(3);
		Assert.True(Math.Abs(s.TargetValue - 0.03) < double.Epsilon, "double Controllable not works");

		var s1 = new Controllable<long, Length.meter>();
		s1.SetTargetValue<Length.centimeter>(308);

		Assert.True(Math.Abs(s1.TargetValue - 3) < double.Epsilon, "long Controllable not works");
	}

	[Fact()]
	public void ToUnitTest()
	{
		var s = new Controllable<double, Length.meter>();
		s.SetTargetValue<Length.centimeter>(3);
		var b = s.ToUnit<Length.millimeter>();
		Assert.True(Math.Abs(b.TargetValue - 30) < double.Epsilon, "Controllable.ToUnit not work");
	}
}