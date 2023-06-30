using DWIS.EngineeringUnits;
using Xunit;

namespace DWIS.Types.Tests;

public class ControllableTests
{
	[Fact()]
	public void setTargetValueTest()
	{
		// none unit
		var u = new Controllable<bool, NonUnit>();
		u.SetTargetValue(true);
		Assert.True(u.Value);

		var s = new Controllable<double, Length.meter>();
		s.SetTargetValue<Length.centimeter>(3);
		Assert.True(Math.Abs(s.Value - 0.03) < double.Epsilon, "double Controllable not works");

		var s1 = new Controllable<long, Length.meter>();
		s1.SetTargetValue<Length.centimeter>(308);

		Assert.True(Math.Abs(s1.Value - 3) < double.Epsilon, "long Controllable not works");
	}

	[Fact()]
	public void ToUnitTest()
	{
		var s = new Controllable<double, Length.meter>();
		s.SetTargetValue<Length.centimeter>(3);
		var b = s.ToUnit<Length.millimeter>();
		Assert.True(Math.Abs(b.Value - 30) < double.Epsilon, "Controllable.ToUnit not work");
	}
}