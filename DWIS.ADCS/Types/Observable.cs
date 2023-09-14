using DWIS.EngineeringUnits;

namespace DWIS.Types;

public record Observable<TValue, TUnit> : Measure<TValue,TUnit>, IObservable<TValue, TUnit> where TUnit : IUnit, new()
{
	public Observable(TValue value, Timestamp timestamp = null, StatusType status = StatusType.Good): base(value)
	{
		Timestamp = timestamp ?? Timestamp.Now;
		Status = status;
	}

	public new TValue Value
	{
		get => base.Value;
		private set => base.Value = value;
	}
	public string Unit => typeof(TUnit).Name;
	public Timestamp Timestamp { get; }
	public StatusType Status { get; }
}