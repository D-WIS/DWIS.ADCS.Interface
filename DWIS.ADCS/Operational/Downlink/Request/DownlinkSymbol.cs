using DWIS.ADCS.EngineeringUnits;
using DWIS.EngineeringUnits;

namespace DWIS.ADCS.Operational.Downlink;



//public class DownlinkSymbol
//{
//    public float RampTimeMs { get; set; }
//    public float HoldTimeMs { get; set; }
//    public float Amplitude { get; set; }
//    public string Unit { get; set; }
//}

public class DownlinkSymbol
{
	public float RampTimeMs { get; set; }
	public float HoldTimeMs { get; set; }
	public IMeasure<float, IUnit> Amplitude { get; set; }
}


public class test
{
	public test()
	{
		var example = new List<DownlinkSymbol>
		{
			new (){RampTimeMs = 6000, HoldTimeMs = 8000, Amplitude = new Measure<float, AngularVelocity.degrees_per_second>(300)},
			new (){RampTimeMs = 6000, HoldTimeMs = 8000, Amplitude = new Measure<float, VolumetricFlow.cubic_meters_per_second>(800)}
		};
	}
}