using Opc.Ua;
using static Opc.Ua.TypeInfo;

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
				Name = "DownlinkSymbolsArray", Description = "20: Requested symbols", DataType = DataTypeIds.Float,
				ValueRank = ValueRanks.OneDimension, ArrayDimensions = new UInt32Collection(new List<uint> { 0 })
			},
			new Argument()
			{
				Name = "DownlinkIndex", Description = "Index of desired downlink from a 2 dimensional Symbol_Table shared in advance, i.e., shared via a file in USB disk.", DataType = DataTypeIds.UInt16,
				ValueRank = ValueRanks.Scalar
			}
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
		timer = new Timer(o =>
		{
			var a = Convert.ToInt32(permission.Value) + 1;
			permission.Value = (UInt16)(a % 5);
			permission.Timestamp = DateTime.UtcNow;
			permission.ClearChangeMasks(SystemContext, false);
		}, null, 1000, 1000);
	}

	private Timer timer;
	private BaseDataVariableState permission;
	private BaseDataVariableState RequestedDownlinkId;

	private void DownlinkStateDate()
	{
		// DownlinkStateData
		var downlinkStateDateObj = CreateObject(root, "DownlinkStateData", "DownlinkStateData");
		RequestedDownlinkId = CreateVariable(downlinkStateDateObj, "RequestedDownlinkId", "RequestedDownlinkId", BuiltInType.UInt32,
			ValueRanks.Scalar);
		permission = CreateVariable(downlinkStateDateObj, "Permission", "Permission", BuiltInType.UInt16,
			ValueRanks.Scalar);
		CreateVariable(downlinkStateDateObj, "DownlinkStatus", "DownlinkStatus", BuiltInType.UInt16,
			ValueRanks.Scalar);
		CreateVariable(downlinkStateDateObj, "PercentComplete", "PercentComplete", BuiltInType.Float,
			ValueRanks.Scalar);
		CreateVariable(downlinkStateDateObj, "DurationRemainingSeconds", "DurationRemainingSeconds", BuiltInType.Float,
			ValueRanks.Scalar);
	}

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
			UInt16 op1 = (UInt16)inputArguments[0];
			UInt16 op2 = (UInt16)inputArguments[1];
			var v = (UInt32)(op1 * op2);
			outputArguments[0] = v;
			RequestedDownlinkId.Value = v;

			permission.Value = 0;
			permission.Timestamp = DateTime.UtcNow;
			permission.ClearChangeMasks(SystemContext, false);
			// set output parameter
			//var n = $"DownlinkRequest{op1 * op2}";

			//var downlinkObj = CreateObject(root, n, n);
			outputArguments[4] = 1;
			return ServiceResult.Good;
		}
		catch
		{
			return new ServiceResult(StatusCodes.BadInvalidArgument);
		}
	}

	#endregion
}