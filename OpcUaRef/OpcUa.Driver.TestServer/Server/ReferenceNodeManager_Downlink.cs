using ADCS.Interface.Share;
using Opc.Ua;
using DWIS.ADCS.Operational.Downlink;
using DWIS.EngineeringUnits;

namespace OpcUa.Driver.TestServer;

public partial class ReferenceNodeManager
{
	#region INodeManager Members


	private void DownlinkConfig()
	{
		var downlinkObj = CreateObject(root, "DownlinkRequest", "DownlinkRequest");

		ConfigSendDownlinkRequestMethod(downlinkObj);
		ConfigAbortDownlinkRequestMethod(downlinkObj);
		DownlinkStateDate();
		//timer = new System.Threading.Timer(o =>
		//{
		//	var a = Convert.ToInt32(permission.Value) + 1;
		//	permission.Value = (UInt16)(a % 5);
		//	permission.Timestamp = DateTime.UtcNow;
		//	permission.ClearChangeMasks(SystemContext, false);
		//}, null, 1000, 1000);
	}
	private void ConfigSendDownlinkRequestMethod(BaseObjectState downlinkObj)
	{
		var sendDownlinkRequest = CreateMethod(downlinkObj, "SendDownlinkRequest", "SendDownlinkRequest");
		// set input arguments
		sendDownlinkRequest.InputArguments = new PropertyState<Argument[]>(sendDownlinkRequest);
		sendDownlinkRequest.InputArguments.NodeId =
			new NodeId(sendDownlinkRequest.BrowseName.Name + "InArgs", NamespaceIndex);
		sendDownlinkRequest.InputArguments.BrowseName = BrowseNames.InputArguments;
		sendDownlinkRequest.InputArguments.DisplayName = sendDownlinkRequest.InputArguments.BrowseName.Name;
		sendDownlinkRequest.InputArguments.TypeDefinitionId = VariableTypeIds.PropertyType;
		sendDownlinkRequest.InputArguments.ReferenceTypeId = ReferenceTypeIds.HasProperty;
		sendDownlinkRequest.InputArguments.DataType = DataTypeIds.Argument;
		sendDownlinkRequest.InputArguments.ValueRank = ValueRanks.OneDimension;

		sendDownlinkRequest.InputArguments.Value = new Argument[]
		{
			new()
			{
				Name = "Method", Description = "uint16, {¡°Symbol_Script¡± | Symbol_Table¡± | ¡°Surface_Equipment¡±}",
				DataType = DataTypeIds.UInt16,
				ValueRank = ValueRanks.Scalar
			},
			new()
			{
				Name = "DownlinkTypes",
				Description =
					"uint16, {¡°OnBottomFlow¡± | ¡°OnBottomRotation¡± | ¡°OffBottomFlow¡± | ¡°OffBottomRotation¡± | \"Other\" | \"None\"}. \"Other\" is used for extensibility of custom implementations",
				DataType = DataTypeIds.UInt16,
				ValueRank = ValueRanks.Scalar
			},
			new()
			{
				Name = "DurationSeconds", Description = "Float valueDuration of the downlink", DataType = DataTypeIds.Float,
				ValueRank = ValueRanks.Scalar
			},
			new()
			{
				Name = "DelaySeconds",
				Description =
					" Optional, Requested start time of downlink from receipt of message. omitted or ¡°0¡± indicates immediately.",
				DataType = DataTypeIds.Float,
				ValueRank = ValueRanks.Scalar
			},
			new()
			{
				Name = "DelayDepth",
				Description =
					"Optional, Requested start depth of downlink from receipt of message. omitted or ¡°0¡± indicates immediately.",
				DataType = DataTypeIds.Float,
				ValueRank = ValueRanks.Scalar
			},

			new()
			{
				Name = "DownlinkIndex",
				Description =
					"Index of desired downlink from a 2 dimensional Symbol_Table shared in advance, i.e., shared via a file in USB disk.",
				DataType = DataTypeIds.Int16,
				ValueRank = ValueRanks.Scalar
			},
			new()
			{
				Name = "DownlinkSymbolsArray", Description = "20: Requested symbols", DataType = DataTypeIds.Float,
				ValueRank = ValueRanks.OneDimension, ArrayDimensions = new UInt32Collection(new List<uint> { 0 })
			},
		};

		// set output arguments
		ConfigDownlinkOutputWithStateData(sendDownlinkRequest);

		sendDownlinkRequest.OnCallMethod = OnRequestDownlinkCall;
	}

	private void ConfigDownlinkOutputWithStateData(MethodState methodState)
	{
		methodState.OutputArguments = new PropertyState<Argument[]>(methodState);
		methodState.OutputArguments.NodeId =
			new NodeId(methodState.BrowseName.Name + "OutArgs", NamespaceIndex);
		methodState.OutputArguments.BrowseName = BrowseNames.OutputArguments;
		methodState.OutputArguments.DisplayName = methodState.OutputArguments.BrowseName.Name;
		methodState.OutputArguments.TypeDefinitionId = VariableTypeIds.PropertyType;
		methodState.OutputArguments.ReferenceTypeId = ReferenceTypeIds.HasProperty;
		methodState.OutputArguments.DataType = DataTypeIds.Argument;
		methodState.OutputArguments.ValueRank = ValueRanks.OneDimension;

		methodState.OutputArguments.Value = new Argument[]
		{
			new()
			{
				Name = "RequestedDownlinkId",
				Description = "RequestedDownlinkId",
				DataType = DataTypeIds.UInt32,
				ValueRank = ValueRanks.Scalar
			},
			new()
			{
				Name = "Permission", Description = "Permission", DataType = DataTypeIds.UInt16,
				ValueRank = ValueRanks.Scalar
			},
			new()
			{
				Name = "DownlinkStatus", Description = "DownlinkStatus", DataType = DataTypeIds.UInt16,
				ValueRank = ValueRanks.Scalar
			},
			new()
			{
				Name = "PercentComplete", Description = "PercentComplete", DataType = DataTypeIds.Float,
				ValueRank = ValueRanks.Scalar
			},
			new()
			{
				Name = "DurationRemainingSeconds", Description = "DurationRemainingSeconds", DataType = DataTypeIds.Float,
				ValueRank = ValueRanks.Scalar
			}
		};
	}

	private void ConfigAbortDownlinkRequestMethod(BaseObjectState downlinkObj)
	{
		var abortDownlinkRequest = CreateMethod(downlinkObj, "AbortDownlinkRequest", "AbortDownlinkRequest");
		// set input arguments
		abortDownlinkRequest.InputArguments = new PropertyState<Argument[]>(abortDownlinkRequest);
		abortDownlinkRequest.InputArguments.NodeId =
			new NodeId(abortDownlinkRequest.BrowseName.Name + "InArgs", NamespaceIndex);
		abortDownlinkRequest.InputArguments.BrowseName = BrowseNames.InputArguments;
		abortDownlinkRequest.InputArguments.DisplayName = abortDownlinkRequest.InputArguments.BrowseName.Name;
		abortDownlinkRequest.InputArguments.TypeDefinitionId = VariableTypeIds.PropertyType;
		abortDownlinkRequest.InputArguments.ReferenceTypeId = ReferenceTypeIds.HasProperty;
		abortDownlinkRequest.InputArguments.DataType = DataTypeIds.Argument;
		abortDownlinkRequest.InputArguments.ValueRank = ValueRanks.OneDimension;

		abortDownlinkRequest.InputArguments.Value = new Argument[]
		{
			new()
			{
				Name = "RequestedDownlinkId",
				Description = "RequestedDownlinkId",
				DataType = DataTypeIds.UInt32,
				ValueRank = ValueRanks.Scalar
			}
		};

		// set output arguments
		ConfigDownlinkOutputWithStateData(abortDownlinkRequest);

		abortDownlinkRequest.OnCallMethod = OnAbortDownlinkCall;
	}

	private System.Threading.Timer timer;
	private BaseDataVariableState permission;
	private BaseDataVariableState requestedDownlinkId;
	private BaseDataVariableState downlinkStatus;

	private BaseDataVariableState percentComplete;
	private BaseDataVariableState durationRemainingSeconds;


	private void DownlinkStateDate()
	{
		// DownlinkStateData
		var downlinkStateDateObj = CreateObject(root, "DownlinkStateData", "DownlinkStateData");
		requestedDownlinkId = CreateVariable(downlinkStateDateObj, "RequestedDownlinkId", "RequestedDownlinkId", BuiltInType.UInt32,
			ValueRanks.Scalar);
		permission = CreateVariable(downlinkStateDateObj, "Permission", "Permission", BuiltInType.UInt16,
			ValueRanks.Scalar);
		permission.Value = Permission.Unset;
		permission.Timestamp = DateTime.UtcNow;

		downlinkStatus = CreateVariable(downlinkStateDateObj, "DownlinkStatus", "DownlinkStatus", BuiltInType.UInt16,
			ValueRanks.Scalar);
		downlinkStatus.Value = DownlinkStatus.Unset;
		percentComplete = CreateVariable(downlinkStateDateObj, "PercentComplete", "PercentComplete", BuiltInType.Float,
			ValueRanks.Scalar);
		percentComplete.Value = 0;
		durationRemainingSeconds = CreateVariable(downlinkStateDateObj, "DurationRemainingSeconds", "DurationRemainingSeconds", BuiltInType.Float,
			ValueRanks.Scalar);
		durationRemainingSeconds.Value = 0;
	}

	private int _requestedDownlinkId = 0;
	private CancellationTokenSource _downlinkCancellationTokenSource = new();
	private ServiceResult OnRequestDownlinkCall(
		ISystemContext context,
		MethodState method,
		IList<object> inputArguments,
		IList<object> outputArguments)
	{
		// all arguments must be provided.
		if (inputArguments.Count < 2)
		{
			return StatusCodes.BadArgumentsMissing;
		}

		try
		{
			Interlocked.Increment(ref _requestedDownlinkId);
			outputArguments[0] = _requestedDownlinkId; 
			requestedDownlinkId.Value = _requestedDownlinkId;

			// set output parameter
			outputArguments[4] = 0; // percentComplete

			var requestData = ParseDownlinkRequestData(inputArguments);
			//Task.Run(async () =>
			//{
			UpdatePermission(Permission.Pending);
			ShowReceivedDownlinkRequest(requestData);

			var per = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("Received downloading request, [green] what's your choice as a driller[/]?")
					.PageSize(10)
					.MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
					.AddChoices(new[]
					{
						"Granted", "Denied",
					}));

			//await Task.Delay(500);
			// Echo the fruit back to the terminal
			AnsiConsole.WriteLine($"Driller's answer: {per}!");
			outputArguments[1] = Permission.Denied;

			if (per == "Granted")
			{
				outputArguments[1] = Permission.Granted;
				//	AnsiConsole.Status()
				//		.Spinner(Spinner.Known.Bounce)
				//		.AutoRefresh(true)
				//		.SpinnerStyle(Style.Parse("green bold"))
				//		.Start("Downlink Started...", ctx =>
				//		{
				//			// Simulate some work
				//			AnsiConsole.MarkupLine("Doing some work...");
				//			Thread.Sleep(1000);

				//			// Update the status and spinner
				//			ctx.Status("Thinking some more");
				//			ctx.Spinner(Spinner.Known.Star);
				//			ctx.SpinnerStyle(Style.Parse("green"));

				//			// Simulate some work
				//			AnsiConsole.MarkupLine("Doing some more work...");
				//			Thread.Sleep(5000);
				//			permission.Value = Permission.Unset;
				//			permission.Timestamp = DateTime.UtcNow;
				//			permission.ClearChangeMasks(SystemContext, false);
				//		});
				//}
				Task.Run(
					() => AnsiConsole.Progress()
						.Columns(
							new TaskDescriptionColumn(), // Task description
							new ProgressBarColumn(), // Progress bar
							new PercentageColumn(), // Percentage
							new RemainingTimeColumn(), // Remaining time
							new SpinnerColumn() // Spinner
						)
						//.AutoClear(true)
						.HideCompleted(true)
						.StartAsync(async ctx =>
						{
							UpdatePermission(Permission.Granted);
							UpdateDownlinkStatus(DownlinkStatus.Scheduled);

							// Define tasks
							var task1 = ctx.AddTask("[green]The Downlink Progress[/]");
							var task2 = ctx.AddTask("second");
							var task3 = ctx.AddTask("third");
							_downlinkCancellationTokenSource.Token.Register(() =>
							{
								task3.Value = 100;
								task2.Value = 100;
								task1.Value = 100;
							});
							task3.Value = 100;
							task3.IsIndeterminate = true;
							while (!ctx.IsFinished)
							{
								// delay
								var delay = requestData.DelaySeconds * 1000;
								var symbolsSeconds =
									requestData.DownlinkSymbolsArray.Sum(s => s.HoldTimeMs + s.RampTimeMs) / 1000;
								var totalSeconds = requestData.DelaySeconds + symbolsSeconds;
								task2.Description = $"[green]Delay {requestData.DelaySeconds}s[/]";
								task2.Value = 0;
								int delayed = 0;
								while (delayed < requestData.DelaySeconds)
								{
									await Task.Delay(1000);
									delayed++;
									task2.Increment(1 / requestData.DelaySeconds * 100);
									task1.Value(delayed / totalSeconds * 100);
									UpdatePercentComplete((delayed / totalSeconds) * 100);
									UpdateDurationRemainingSeconds(totalSeconds - delayed);
								}
								UpdateDownlinkStatus(DownlinkStatus.Running);

								// downlink
								double symTimes = 0;
								task3.Value = 0;
								for (int j = 0; j < requestData.DownlinkSymbolsArray.Length; j++)
								{
									task2.Value = 0;
									var sym = requestData.DownlinkSymbolsArray[j];
									task2.Description = $"[blue]Symb:{j};Ampl: {sym.Amplitude.Value}[/]";
									task3.Description = $"[yellow]Ramp: {sym.RampTimeMs}[/]";
									await Task.Delay(Convert.ToInt32(sym.RampTimeMs));

									symTimes += (sym.RampTimeMs) / 1000;
									UpdatePercentComplete(((delayed + symTimes) / totalSeconds) * 100);
									UpdateDurationRemainingSeconds(totalSeconds - delayed - symTimes);

									task1.Value((delayed + symTimes) / totalSeconds * 100);

									task2.Value = sym.RampTimeMs / (sym.RampTimeMs + sym.HoldTimeMs) * 100;
									task3.Description =
										$"[yellow]Hold: {sym.HoldTimeMs}ms[/]";
									await Task.Delay(Convert.ToInt32(sym.HoldTimeMs));

									symTimes += (sym.HoldTimeMs) / 1000;
									UpdatePercentComplete(((delayed + symTimes) / totalSeconds) * 100);
									UpdateDurationRemainingSeconds(totalSeconds - delayed - symTimes);

									task1.Value((delayed + symTimes) / totalSeconds * 100);
									task2.Value = 100;
								}

								task3.Value = 100;
								UpdatePercentComplete(100);
								UpdateDurationRemainingSeconds(0);

								UpdateDownlinkStatus(DownlinkStatus.Completed);
								UpdatePercentComplete(100);
								UpdatePercentComplete(0);
							}
						}), _downlinkCancellationTokenSource.Token);

				//}
			}
			else
			{
				UpdatePermission(Permission.Denied);
			}

			//});
			return ServiceResult.Good;
		}
		catch
		{
			return new ServiceResult(StatusCodes.BadInvalidArgument);
		}
	}

	private void UpdateDurationRemainingSeconds(double value)
	{
		durationRemainingSeconds.Value = value;
		durationRemainingSeconds.ClearChangeMasks(SystemContext, false);
	}

	private void UpdateDownlinkStatus(DownlinkStatus value)
	{
		downlinkStatus.Value = value;
		downlinkStatus.ClearChangeMasks(SystemContext, false);
	}

	private void UpdatePercentComplete(double value)
	{
		percentComplete.Value = value;
		percentComplete.ClearChangeMasks(SystemContext, false);
	}
	private void UpdatePermission(Permission permissionValue)
	{
		permission.Value = permissionValue;
		permission.Timestamp = DateTime.UtcNow;
		permission.ClearChangeMasks(SystemContext, false);
	}

	private static void ShowReceivedDownlinkRequest(DownlinkRequestData requestData)
	{
		var downlink = ConsoleExt.GetJsonText(requestData);
		AnsiConsole.Write(
			new Panel(downlink)
				.Header("Received Downlink Request")
				.Collapse()
				.RoundedBorder()
				.BorderColor(Color.Yellow));
	}


	private static DownlinkRequestData ParseDownlinkRequestData(IList<object> inputArguments)
	{
		var requestData = new DownlinkRequestData()
		{
			Method = (Method)inputArguments[0],
			Type = (DownlinkTypes)inputArguments[1],
			DurationSeconds = Convert.ToSingle(inputArguments[2]),
			DelaySeconds = Convert.ToSingle(inputArguments[3]),
			DelayDepth = Convert.ToSingle(inputArguments[4]),
			DownlinkIndex = Convert.ToInt16(inputArguments[5]),
		};
		var symbols = inputArguments[6] as float[]; //DownlinkSymbol[]
		if (symbols != null)
		{
			var sym = new List<DownlinkSymbol>();
			for (int i = 0; i < symbols.Length / 3; i++)
			{
				var symbol = new DownlinkSymbol()
				{
					RampTimeMs = symbols[i * 3],
					HoldTimeMs = symbols[i * 3 + 1],
					Amplitude = new Measure<float, VolumetricFlow.cubic_meters_per_second>(symbols[i * 3 + 2])
				};
				sym.Add((symbol));
			}

			requestData.DownlinkSymbolsArray = sym.ToArray();
		}

		return requestData;
	}

	private ServiceResult OnAbortDownlinkCall(
		ISystemContext context,
		MethodState method,
		IList<object> inputArguments,
		IList<object> outputArguments)
	{
		// todo
		UpdateDownlinkStatus(DownlinkStatus.Aborted);
		_downlinkCancellationTokenSource.Cancel();
		return ServiceResult.Good;
	}
	#endregion
}