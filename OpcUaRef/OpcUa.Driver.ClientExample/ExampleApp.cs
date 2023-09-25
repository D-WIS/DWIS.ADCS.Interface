using Microsoft.Extensions.Logging;
using Opc.Ua.Client;
using Opc.Ua;
using OpcUa.Driver;

namespace OpcUa.Driver.ClientExample;

internal class ExampleApp
{
	private readonly ILogger _logger;
	private readonly IOpcUaClient _client;

	public ExampleApp(ILogger logger, IOpcUaClient client)
	{
		_logger = logger;
		_client = client;
	}

	public async Task Run()
	{
		ReadNodes();
		WriteNodes();
		await SubscribeAsync();
		//CyclicRead();
		Browse();
	}

	public void CyclicRead()
	{
		var nodes = new List<NodeId>() { Variables.Server_ServerStatus, Variables.Server_ServerStatus_StartTime };
		_client.ReadCyclic(
			nodes, 1000, t =>
			{
				_logger.LogInformation("cyclic reading:");
				for (var index = 0; index < t.Count; index++)
				{
					var d = t[index];
					_logger.LogInformation($"      : {d.ServerTimestamp} {nodes[index]} {d.Value}");
				}
			});
	}

	/// <summary>
	/// Read a list of nodes from Server
	/// </summary>
	public void ReadNodes()
	{
		var session = _client.Session;
		if (!session.Connected)
		{
			_logger.LogError("Session not connected!");
			return;
		}

		try
		{
			#region Read a node by calling the Read Service

			// build a list of nodes to be read
			var nodesToRead = new ReadValueIdCollection()
				{
                    // Value of ServerStatus
                    new() { NodeId = Variables.Server_ServerStatus, AttributeId = Attributes.Value },
                    // BrowseName of ServerStatus_StartTime
                    new() { NodeId = Variables.Server_ServerStatus_StartTime, AttributeId = Attributes.BrowseName },
                    // Value of ServerStatus_StartTime
                    new() { NodeId = Variables.Server_ServerStatus_StartTime, AttributeId = Attributes.Value }
				};

			// Read the node attributes
			_logger.LogInformation("Reading nodes...");

			// Call Read Service
			session.Read(
				null,
				0,
				TimestampsToReturn.Both,
				nodesToRead,
				out var resultsValues,
				out var diagnosticInfos);

			// Validate the results
			var validateResponse = ClientBase.ValidateResponse;
			validateResponse(resultsValues, nodesToRead);

			// Display the results.
			foreach (var result in resultsValues)
			{
				_logger.LogInformation("Read Value = {0} , StatusCode = {1}", result.Value, result.StatusCode);
			}
			#endregion

			#region Read the Value attribute of a node by calling the Session.ReadValue method
			// Read Server NamespaceArray
			_logger.LogInformation("Reading Value of NamespaceArray node...");
			var namespaceArray = session.ReadValue(Variables.Server_NamespaceArray);
			// Display the result
			_logger.LogInformation($"NamespaceArray Value = {namespaceArray}");
			#endregion
		}
		catch (Exception ex)
		{
			// Log Error
			_logger.LogError($"Read Nodes Error : {ex.Message}.");
		}
	}

	/// <summary>
	/// Write a list of nodes to the Server.
	/// </summary>
	public void WriteNodes()
	{
		var session = _client.Session;
		if (!session.Connected)
		{
			_logger.LogError("Session not connected!");
			return;
		}

		try
		{
			// Write the configured nodes
			var nodesToWrite = new WriteValueCollection();

			// Int32 Node - Objects\CTT\Scalar\Scalar_Static\Int32
			var intWriteVal = new WriteValue
			{
				NodeId = new NodeId("ns=2;s=Scalar_Static_Int32"),
				AttributeId = Attributes.Value,
				Value = new DataValue
				{
					Value = (int)100
				}
			};
			nodesToWrite.Add(intWriteVal);

			// Float Node - Objects\CTT\Scalar\Scalar_Static\Float
			var floatWriteVal = new WriteValue
			{
				NodeId = new NodeId("ns=2;s=Scalar_Static_Float"),
				AttributeId = Attributes.Value,
				Value = new DataValue
				{
					Value = (float)100.5
				}
			};
			nodesToWrite.Add(floatWriteVal);

			// String Node - Objects\CTT\Scalar\Scalar_Static\String
			var stringWriteVal = new WriteValue
			{
				NodeId = new NodeId("ns=2;s=Scalar_Static_String"),
				AttributeId = Attributes.Value,
				Value = new DataValue { Value = "String Test" }
			};
			nodesToWrite.Add(stringWriteVal);

			// Write the node attributes
			_logger.LogInformation("Writing nodes...");

			// Call Write Service
			session.Write(null,
							nodesToWrite,
							out var results,
							out var diagnosticInfos);

			// Validate the response
			var validateResponse = ClientBase.ValidateResponse;
			validateResponse(results, nodesToWrite);

			// Display the results.
			_logger.LogInformation("Write Results :");

			foreach (var writeResult in results)
			{
				_logger.LogInformation("     {0}", writeResult);
			}
		}
		catch (Exception ex)
		{
			// Log Error
			_logger.LogError($"Write Nodes Error : {ex.Message}.");
		}
	}

	public async Task SubscribeAsync()
	{
		var nodes = new List<SubscriptionNode>()
		{
			new( "ns=2;s=Scalar_Simulation_Int32",  OnMonitoredItemNotification, "Int32 Variable"),
			new( "ns=2;s=Scalar_Simulation_Float", OnMonitoredItemNotification, "Float Variable"),
			new( "ns=2;s=Scalar_Simulation_String", OnMonitoredItemNotification, "String Variable"),
		};

		await _client.SubscribeAsync(nodes, 1000).ConfigureAwait(false);
	}

	/// <summary>
	/// Browse Server nodes
	/// </summary>
	public void Browse()
	{
		if (!_client.Session.Connected)
		{
			_logger.LogError("Session not connected!");
			return;
		}

		try
		{
			var browser = new Browser(_client.Session)
			{
				BrowseDirection = BrowseDirection.Forward,
				NodeClassMask = (int)NodeClass.Object | (int)NodeClass.Variable,
				ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
				IncludeSubtypes = true
			};
			var nodeToBrowse = ObjectIds.Server;

			_logger.LogInformation("Browsing {0} node...", nodeToBrowse);
			var browseResults = browser.Browse(nodeToBrowse);

			_logger.LogInformation("Browse returned {0} results:", browseResults.Count);
			foreach (var result in browseResults)
			{
				_logger.LogInformation("     DisplayName = {0}, NodeClass = {1}", result.DisplayName.Text, result.NodeClass);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError($"Browse Error : {ex.Message}.");
		}
	}

	/// <summary>
	/// Call UA method
	/// </summary>
	public void CallMethod()
	{
		if (!_client.Session.Connected)
		{
			_logger.LogInformation("Session not connected!");
			return;
		}

		try
		{
			// Parent node - Objects\CTT\Methods
			var objectId = new NodeId("ns=2;s=Methods");
			// Method node - Objects\CTT\Methods\Add
			var methodId = new NodeId("ns=2;s=Methods_Add");

			var inputArguments = new object[] { (float)10.5, (uint)10 };

			_logger.LogInformation("Calling UAMethod for node {0} ...", methodId);
			var outputArguments = _client.Session.Call(objectId, methodId, inputArguments);

			_logger.LogInformation("Method call returned {0} output argument(s):", outputArguments.Count);
			foreach (var outputArgument in outputArguments)
			{
				_logger.LogInformation("     OutputValue = {0}", outputArgument.ToString());
			}
		}
		catch (Exception ex)
		{
			_logger.LogInformation("Method call error: {0}", ex.Message);
		}
	}

	/// <summary>
	/// Handle DataChange notifications from Server
	/// </summary>
	private void OnMonitoredItemNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
	{
		try
		{
			// Log MonitoredItem Notification event
			var notification = e.NotificationValue as MonitoredItemNotification;
			_logger.LogInformation("Notification: {0} \"{1}\" and Value = {2}.",
				notification!.Message.SequenceNumber, monitoredItem.ResolvedNodeId, notification.Value);
		}
		catch (Exception ex)
		{
			_logger.LogInformation("OnMonitoredItemNotification error: {0}", ex.Message);
		}
	}

}