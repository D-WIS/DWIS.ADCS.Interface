using System.Text.Json;
using System.Text.Json.Serialization;
using ADCS.Interface.Share;
using Opc.Ua;
using Spectre.Console.Json;
using DWIS.ADCS.Operational.Downlink;
using DWIS.EngineeringUnits;

namespace OpcUa.Driver.TestServer;

public partial class ReferenceNodeManager
{
	#region INodeManager Members


	private void DownlinkConfig()
	{
		var downlinkObj = CreateObject(root, "DownlinkRequest", "DownlinkRequest");

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
			new Argument()
			{
				Name = "Method", Description = "uint16, {¡°Symbol_Script¡± | Symbol_Table¡± | ¡°Surface_Equipment¡±}", DataType = DataTypeIds.UInt16,
				ValueRank = ValueRanks.Scalar
			},
			new Argument()
			{
				Name = "DownlinkTypes", Description = "uint16, {¡°OnBottomFlow¡± | ¡°OnBottomRotation¡± | ¡°OffBottomFlow¡± | ¡°OffBottomRotation¡± | \"Other\" | \"None\"}. \"Other\" is used for extensibility of custom implementations", DataType = DataTypeIds.UInt16,
				ValueRank = ValueRanks.Scalar
			},
			new Argument()
			{
				Name = "DurationSeconds", Description = "Float valueDuration of the downlink", DataType = DataTypeIds.Float,
				ValueRank = ValueRanks.Scalar
			},
			new Argument()
			{
				Name = "DelaySeconds", Description = " Optional, Requested start time of downlink from receipt of message. omitted or ¡°0¡± indicates immediately.", DataType = DataTypeIds.Float,
				ValueRank = ValueRanks.Scalar
			},
			new Argument()
			{
				Name = "DelayDepth", Description = "Optional, Requested start depth of downlink from receipt of message. omitted or ¡°0¡± indicates immediately.", DataType = DataTypeIds.Float,
				ValueRank = ValueRanks.Scalar
			},

			new Argument()
			{
				Name = "DownlinkIndex", Description = "Index of desired downlink from a 2 dimensional Symbol_Table shared in advance, i.e., shared via a file in USB disk.", DataType = DataTypeIds.Int16,
				ValueRank = ValueRanks.Scalar
			},
			new Argument()
			{
				Name = "DownlinkSymbolsArray", Description = "20: Requested symbols", DataType = DataTypeIds.Float,
				ValueRank = ValueRanks.OneDimension, ArrayDimensions = new UInt32Collection(new List<uint> { 0 })
			},
		};

		// set output arguments
		sendDownlinkRequest.OutputArguments = new PropertyState<Argument[]>(sendDownlinkRequest);
		sendDownlinkRequest.OutputArguments.NodeId =
			new NodeId(sendDownlinkRequest.BrowseName.Name + "OutArgs", NamespaceIndex);
		sendDownlinkRequest.OutputArguments.BrowseName = BrowseNames.OutputArguments;
		sendDownlinkRequest.OutputArguments.DisplayName = sendDownlinkRequest.OutputArguments.BrowseName.Name;
		sendDownlinkRequest.OutputArguments.TypeDefinitionId = VariableTypeIds.PropertyType;
		sendDownlinkRequest.OutputArguments.ReferenceTypeId = ReferenceTypeIds.HasProperty;
		sendDownlinkRequest.OutputArguments.DataType = DataTypeIds.Argument;
		sendDownlinkRequest.OutputArguments.ValueRank = ValueRanks.OneDimension;

		sendDownlinkRequest.OutputArguments.Value = new Argument[]
		{
			new Argument()
			{
				Name = "RequestedDownlinkId", Description = "RequestedDownlinkId", DataType = DataTypeIds.UInt32,
				ValueRank = ValueRanks.Scalar
			},
			new Argument()
			{
				Name = "Permission", Description = "Permission", DataType = DataTypeIds.UInt16,
				ValueRank = ValueRanks.Scalar
			},
			new Argument()
			{
				Name = "DownlinkStatus", Description = "DownlinkStatus", DataType = DataTypeIds.UInt16,
				ValueRank = ValueRanks.Scalar
			},
			new Argument()
			{
				Name = "PercentComplete", Description = "PercentComplete", DataType = DataTypeIds.Float,
				ValueRank = ValueRanks.Scalar
			},
			new Argument()
			{
				Name = "DurationRemainingSeconds", Description = "DurationRemainingSeconds", DataType = DataTypeIds.Float,
				ValueRank = ValueRanks.Scalar
			}
		};

		sendDownlinkRequest.OnCallMethod = new GenericMethodCalledEventHandler(OnRequestDownlinkCall);

		DownlinkStateDate();
		//timer = new System.Threading.Timer(o =>
		//{
		//	var a = Convert.ToInt32(permission.Value) + 1;
		//	permission.Value = (UInt16)(a % 5);
		//	permission.Timestamp = DateTime.UtcNow;
		//	permission.ClearChangeMasks(SystemContext, false);
		//}, null, 1000, 1000);
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

	private int _downlinkId = 0;
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
			_downlinkId++;
			outputArguments[0] = _downlinkId;
			requestedDownlinkId.Value = _downlinkId;


			// set output parameter
			//var n = $"DownlinkRequest{op1 * op2}";

			//var downlinkObj = CreateObject(root, n, n);
			outputArguments[4] = 0;


			var requestData = new DownlinkRequestData()
			{
				Method = (Method)inputArguments[0],
				Type = (DownlinkTypes)inputArguments[1],
				DurationSeconds = Convert.ToSingle(inputArguments[2]),
				DelaySeconds = Convert.ToSingle(inputArguments[3]),
				DelayDepth = Convert.ToSingle(inputArguments[4]),
				DownlinkIndex = Convert.ToInt16(inputArguments[5]),
			};
			var symbols = inputArguments[6] as float[];
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

			//Task.Run(async () =>
			//{

			var downlink = ConsoleExt.GetJsonText(requestData);

			permission.Value = Permission.Pending;
			permission.Timestamp = DateTime.UtcNow;
			permission.ClearChangeMasks(SystemContext, false);
			AnsiConsole.Write(
				new Panel(downlink)
					.Header("Received Downlink Request")
					.Collapse()
					.RoundedBorder()
					.BorderColor(Color.Yellow));

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
						.Columns(new ProgressColumn[]
						{
							new TaskDescriptionColumn(), // Task description
							new ProgressBarColumn(), // Progress bar
							new PercentageColumn(), // Percentage
							new RemainingTimeColumn(), // Remaining time
							new SpinnerColumn(), // Spinner
						})
						//.AutoClear(true)
						.HideCompleted(true)
						.StartAsync(async ctx =>
						{
							permission.Value = Permission.Granted;
							//permission.Timestamp = DateTime.UtcNow;
							permission.ClearChangeMasks(SystemContext, false);

							downlinkStatus.Value = DownlinkStatus.Scheduled;
							downlinkStatus.ClearChangeMasks(SystemContext, false);

							// Define tasks
							var task1 = ctx.AddTask("[green]The Downlink Progress[/]");
							var task2 = ctx.AddTask("second");
							var task3 = ctx.AddTask("third");
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
									percentComplete.Value = (delayed / totalSeconds) * 100;
									percentComplete.ClearChangeMasks(SystemContext, false);
									durationRemainingSeconds.Value = totalSeconds - delayed;
									durationRemainingSeconds.ClearChangeMasks(SystemContext, false);

								}
								downlinkStatus.Value = DownlinkStatus.Running;
								downlinkStatus.ClearChangeMasks(SystemContext, false);
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
									percentComplete.Value = ((delayed + symTimes) / totalSeconds) * 100;
									percentComplete.ClearChangeMasks(SystemContext, false);

									durationRemainingSeconds.Value = totalSeconds - delayed - symTimes;
									durationRemainingSeconds.ClearChangeMasks(SystemContext, false);

									task1.Value((delayed + symTimes) / totalSeconds * 100);

									task2.Value = sym.RampTimeMs / (sym.RampTimeMs + sym.HoldTimeMs) * 100;
									task3.Description =
										$"[yellow]Hold: {sym.HoldTimeMs}ms[/]";
									await Task.Delay(Convert.ToInt32(sym.HoldTimeMs));

									symTimes += (sym.HoldTimeMs) / 1000;
									percentComplete.Value = ((delayed + symTimes) / totalSeconds) * 100;
									percentComplete.ClearChangeMasks(SystemContext, false);

									durationRemainingSeconds.Value = totalSeconds - delayed - symTimes;
									durationRemainingSeconds.ClearChangeMasks(SystemContext, false);

									task1.Value((delayed + symTimes) / totalSeconds * 100);
									task2.Value = 100;
								}

								task3.Value = 100;
								percentComplete.Value = 100;
								percentComplete.ClearChangeMasks(SystemContext, false);

								durationRemainingSeconds.Value = 0;
								durationRemainingSeconds.ClearChangeMasks(SystemContext, false);

								downlinkStatus.Value = DownlinkStatus.Completed;
								downlinkStatus.ClearChangeMasks(SystemContext, false);								percentComplete.Value = 100;
								percentComplete.ClearChangeMasks(SystemContext, false);

								percentComplete.Value = 0;
								percentComplete.ClearChangeMasks(SystemContext, false);
							}
						}));

				//}
			}
			else
			{
				permission.Value = Permission.Denied;
				permission.ClearChangeMasks(SystemContext, false);

			}


			//});
			return ServiceResult.Good;
		}
		catch
		{
			return new ServiceResult(StatusCodes.BadInvalidArgument);
		}
	}

	#endregion
}