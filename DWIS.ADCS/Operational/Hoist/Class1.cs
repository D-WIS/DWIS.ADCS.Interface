using DWIS.EngineeringUnits;
using DWIS.Types;

namespace DWIS.ADCS.Operational.Hoist;

public record HoistStateData(/*replace the State to Status for now*/StatusType State, Timestamp Timestamp)
{
	public Controllable<double, Length.meter> PositionTarget;
	public Controllable<double, Length.meter> PositionActual;
	//public  Limits<Length>                              PositionLimits;
	//public  Types<Velocity::meters_per_second>                 VelocityTarget;
	//public  Types<Velocity::meters_per_second>                 VelocityActual;
	//public  Limits<Velocity>                            VelocityLimits;
	//public  Limits<Velocity::meters_per_second_squared> AccelerationLimits;
	//public HoistStateData()
	//{
	//	PositionTarget.SetTargetValue<Length.feet>(55);
	//}
}

//interface HoistState
//{
//	struct HoistStateData hoistStateData;

//	void SetListener(in QoSListener a_listener);
//	QoSListner GetListener();

//	HoistStateData::State GetState();
//	HoistStateData::Timestamp GetTimestamp();
//	HoistStateData::PositionTarget GetPositionTarget();
//	HoistStateData::PositionActual GetPositionActual();
//	HoistStateData::PositionLimits GetPositionLimits();
//	HoistStateData::VelocityTarget GetVelocityTarget();
//	HoistStateData::VelocityActual GetVelocityActual();
//	HoistStateData::VelocityLimits GetVelocityLimits();
//	HoistStateData::AccelerationLimits GetAccelerationLimits();
//}