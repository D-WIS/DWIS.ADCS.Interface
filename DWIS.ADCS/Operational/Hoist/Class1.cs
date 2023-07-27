using DWIS.EngineeringUnits;
using DWIS.Types;

namespace DWIS.ADCS.Operational.Hoist;

record struct HoistStateData(long State, Timestamp Timestamp)
{
    Controllable<double, Length.meter> PositionTarget;
    //Types<Length::meters>                              PositionActual;
    //Limits<Length>                              PositionLimits;
    //Types<Velocity::meters_per_second>                 VelocityTarget;
    //Types<Velocity::meters_per_second>                 VelocityActual;
    //Limits<Velocity>                            VelocityLimits;
    //Limits<Velocity::meters_per_second_squared> AccelerationLimits;
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