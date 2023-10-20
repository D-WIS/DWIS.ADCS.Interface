using System.ComponentModel;
using System.Runtime.InteropServices;

namespace OpcUa.Driver.Timer;
//Used https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke as a guide 
// This was also VERY useful: https://docs.microsoft.com/en-gb/windows/win32/multimedia/timer-operations

public sealed class MmTimer : IComponent
{
	/// <summary>
	/// The timeGetDevCaps function queries the timer device to determine its resolution.
	/// </summary>
	/// <param name="caps"></param>
	/// A pointer to a TIMECAPS structure. This structure is filled with information about the resolution of the timer device.
	/// <param name="sizeOfTimerCaps"></param>
	/// The size, in bytes, of the TIMECAPS structure.
	/// <returns></returns>
	[DllImport("winmm.dll")]
	private static extern int timeGetDevCaps(ref TimerCaps caps, int sizeOfTimerCaps);

	/// <summary>
	/// The timeSetEvent function starts a specified timer event. The multimedia timer runs in its own thread. 
	/// After the event is activated, it calls the specified callback function or sets or pulses the specified event object.
	/// </summary>
	/// <param name="delay"></param>
	/// Event delay, in milliseconds. 
	/// If this value is not in the range of the minimum and maximum event delays supported by the timer, the function returns an error.    
	/// <param name="resolution"></param>
	/// Resolution of the timer event, in milliseconds. 
	/// The resolution increases with smaller values; a resolution of 0 indicates periodic events should occur with the greatest possible accuracy. 
	/// To reduce system overhead, however, you should use the maximum value appropriate for your application.
	/// <param name="proc"></param>
	/// Pointer to a callback function that is called once upon expiration of a single event or periodically upon expiration of periodic events.
	/// If fuEvent specifies the TIME_CALLBACK_EVENT_SET or TIME_CALLBACK_EVENT_PULSE flag, then the lpTimeProc parameter is interpreted as a handle to an event object. 
	/// The event will be set or pulsed upon completion of a single event or periodically upon completion of periodic events.
	/// <param name="user"></param>
	/// User-supplied callback data.
	/// <param name="mode"></param>
	/// MMTimer event type. Oneshot or Periodic
	/// <returns></returns>
	[DllImport("winmm.dll")]
	private static extern int timeSetEvent(int delay, int resolution, TimeProc proc, int user, int mode);

	/// <summary>
	/// The timeKillEvent function cancels a specified timer event.
	/// </summary>
	/// <param name="id"></param>
	/// Identifier of the timer event to cancel. This identifier was returned by the timeSetEvent function when the timer event was set up.
	/// <returns></returns>
	[DllImport("winmm.dll")]
	private static extern int timeKillEvent(int id);

	public event EventHandler Started;
	public event EventHandler Stopped;
	public event EventHandler Elapsed;
	public event EventHandler Disposed;

	static MmTimer()
	{
		timeGetDevCaps(ref s_caps, Marshal.SizeOf<TimerCaps>(MmTimer.s_caps));
	}


	public MmTimer()
	{
		Initialize();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref='MmTimer'/> class, setting the <see cref='Interval'/> property to the specified period.
	/// </summary>
	public MmTimer(double interval)
	{
		Initialize();
		Interval = TimeSpan.FromMilliseconds(interval);
	}

	private void Initialize()
	{
		_mode = TimerMode.Periodic;
		_period = Capabilities.PeriodMin;
		Resolution = TimeSpan.FromMilliseconds(1);
		IsRunning = false;
		_timeProcPeriodic = PeriodicEventCallback;
		_timeProcOneShot = OneShotEventCallback;
		_tickRaiser = OnTick;
	}


	public void Start()
	{
		if (IsRunning)
		{
			return;
		}

		_timerId = timeSetEvent(
			(int)Interval.TotalMilliseconds,
			_resolution,
			Mode == TimerMode.Periodic ? _timeProcPeriodic : _timeProcOneShot,
			0,
			(int)Mode
			);

		if (_timerId == 0)
		{
			throw new Exception("Unable to start multimedia MMTimer.");
		}

		IsRunning = true;
		if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
		{
			SynchronizingObject.BeginInvoke(new TickDelegate(OnStarted), new object[]
			{
				EventArgs.Empty
			});
			return;
		}
		OnStarted(EventArgs.Empty);
	}

	public void Stop()
	{
		if (!IsRunning)
		{
			return;
		}
		timeKillEvent(_timerId);
		IsRunning = false;
		if (SynchronizingObject is { InvokeRequired: true })
		{
			SynchronizingObject.BeginInvoke(new TickDelegate(OnStopped), new object[]
			{
				EventArgs.Empty
			});
			return;
		}
		OnStopped(EventArgs.Empty);
	}

	public ISynchronizeInvoke? SynchronizingObject
	{
		get => _synchronizingObject;
		set => _synchronizingObject = value;
	}

	/// <summary>
	/// Event delay, in milliseconds. 
	/// If this value is not in the range of the minimum and maximum event delays supported by the timer, the function returns an error.
	/// </summary>
	public TimeSpan Interval
	{
		get => TimeSpan.FromMilliseconds(_period);
		set
		{
			if (value.TotalMilliseconds < Capabilities.PeriodMin || value.TotalMilliseconds > Capabilities.PeriodMax)
			{
				throw new ArgumentOutOfRangeException("Delay", value, "Multimedia MMTimer delay out of range.");
			}
			_period = (int)value.TotalMilliseconds;
			if (IsRunning)
			{
				Stop();
				Start();
			}
		}
	}

	public int MinPeriod => Capabilities.PeriodMin;
	public int MaxPeriod => Capabilities.PeriodMax;

	/// <summary>
	/// Resolution of the timer event, in milliseconds. 
	/// The resolution increases with smaller values; a resolution of 0 indicates periodic events should occur with the greatest possible accuracy. 
	/// To reduce system overhead, however, you should use the maximum value appropriate for your application.
	/// </summary>
	public TimeSpan Resolution
	{
		get => TimeSpan.FromMilliseconds(_resolution);
		set
		{
			_resolution = (int)value.TotalMilliseconds;
			if (IsRunning)
			{
				Stop();
				Start();
			}
		}
	}

	public TimerMode Mode
	{
		get
		{
			return _mode;
		}
		set
		{
			_mode = value;
			if (IsRunning)
			{
				Stop();
				Start();
			}
		}
	}

	public bool IsRunning { get; private set; }

	public static TimerCaps Capabilities => s_caps;

	public ISite Site { get; set; }

	private void PeriodicEventCallback(int id, int msg, int user, int param1, int param2)
	{
		if (_synchronizingObject != null)
		{
			_synchronizingObject.BeginInvoke(_tickRaiser, new object[]
			{
				EventArgs.Empty
			});
			return;
		}
		OnTick(EventArgs.Empty);
	}

	private void OneShotEventCallback(int id, int msg, int user, int param1, int param2)
	{
		if (_synchronizingObject != null)
		{
			_synchronizingObject.BeginInvoke(_tickRaiser, new object[]
			{
				EventArgs.Empty
			});
			Stop();
			return;
		}
		OnTick(EventArgs.Empty);
		Stop();
	}


	private void OnStarted(EventArgs e)
	{
		Started?.Invoke(this, e);
	}

	private void OnStopped(EventArgs e)
	{
		Stopped?.Invoke(this, e);
	}

	private void OnTick(EventArgs e)
	{
		Elapsed?.Invoke(this, e);
	}

	public void Dispose()
	{
		if (IsRunning)
		{
			timeKillEvent(_timerId);
		}

		GC.SuppressFinalize(this);
	}


	~MmTimer()
	{
		if (IsRunning)
		{
			timeKillEvent(_timerId);
		}
	}


	private int _timerId;

	private volatile TimerMode _mode;

	private volatile int _period;

	private volatile int _resolution;

	private TimeProc _timeProcPeriodic;

	private TimeProc _timeProcOneShot;

	private TickDelegate _tickRaiser;

	private ISynchronizeInvoke? _synchronizingObject;

	private static TimerCaps s_caps;

	/// <summary>
	/// The TimeProc function callback function that is called once upon the expiration of a single event or periodically upon the expiration of periodic events.
	/// </summary>
	/// <param name="id"></param>
	/// Identifier of the timer event. This identifier was returned by the timeSetEvent function when the timer event was set up.
	/// <param name="msg"></param>
	/// Reserved; do not use.
	/// <param name="user"></param>
	/// User instance data supplied to the dwUser parameter of timeSetEvent.
	/// <param name="param1"></param>
	/// Reserved; do not use.
	/// <param name="param2"></param>
	/// Reserved; do not use.
	private delegate void TimeProc(int id, int msg, int user, int param1, int param2);

	private delegate void TickDelegate(EventArgs e);
}