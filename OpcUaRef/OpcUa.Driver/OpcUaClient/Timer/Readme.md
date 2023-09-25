```csharp
var timer = new MMTimer(5);
timer.Elapsed += Timer_Tick;

//Optional. Defaults to 1ms
timer.Resolution = TimeSpan.FromMilliseconds(2);
timer.Start();
```