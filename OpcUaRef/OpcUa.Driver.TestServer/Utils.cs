using System.Reflection;
using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Server;
using System.Text;

namespace OpcUa.Driver.TestServer;
/// <summary>
/// The error code why the application exit.
/// </summary>
public enum ExitCode : int
{
	Ok = 0,
	ErrorNotStarted = 0x80,
	ErrorRunning = 0x81,
	ErrorException = 0x82,
	ErrorStopping = 0x83,
	ErrorCertificate = 0x84,
	ErrorInvalidCommandLine = 0x100
}

/// <summary>
/// An exception that occured and caused an exit of the application.
/// </summary>
[Serializable]
public class ErrorExitException : Exception
{
	public ExitCode ExitCode { get; }

	public ErrorExitException(ExitCode exitCode)
	{
		ExitCode = exitCode;
	}

	public ErrorExitException()
	{
		ExitCode = ExitCode.Ok;
	}

	public ErrorExitException(string message) : base(message)
	{
		ExitCode = ExitCode.Ok;
	}

	public ErrorExitException(string message, ExitCode exitCode) : base(message)
	{
		ExitCode = exitCode;
	}

	public ErrorExitException(string message, Exception innerException) : base(message, innerException)
	{
		ExitCode = ExitCode.Ok;
	}

	public ErrorExitException(string message, Exception innerException, ExitCode exitCode) : base(message, innerException)
	{
		ExitCode = exitCode;
	}
}

/// <summary>
/// A dialog which asks for user input.
/// </summary>
public class ApplicationMessageDlg : IApplicationMessageDlg
{
	private TextWriter m_output;
	private string m_message = string.Empty;
	private bool m_ask;

	public ApplicationMessageDlg(TextWriter output)
	{
		m_output = output;
	}

	public override void Message(string text, bool ask)
	{
		m_message = text;
		m_ask = ask;
	}

	public override async Task<bool> ShowAsync()
	{
		if (m_ask)
		{
			var message = new StringBuilder(m_message);
			message.Append(" (y/n, default y): ");
			m_output.Write(message.ToString());

			try
			{
				var result = Console.ReadKey();
				m_output.WriteLine();
				return await Task.FromResult((result.KeyChar == 'y') ||
					(result.KeyChar == 'Y') || (result.KeyChar == '\r')).ConfigureAwait(false);
			}
			catch
			{
				// intentionally fall through
			}
		}
		else
		{
			m_output.WriteLine(m_message);
		}

		return await Task.FromResult(true).ConfigureAwait(false);
	}
}

public class Utilities
{
	private static IList<INodeManagerFactory> m_nodeManagerFactories;

	/// <summary>
	/// The property with available node manager factories.
	/// </summary>
	public static ReadOnlyList<INodeManagerFactory> NodeManagerFactories
	{
		get
		{
			if (m_nodeManagerFactories == null)
			{
				m_nodeManagerFactories = GetNodeManagerFactories();
			}
			return new ReadOnlyList<INodeManagerFactory>(m_nodeManagerFactories);
		}
	}
	/// <summary>
	/// Applies custom settings to quickstart servers for CTT run.
	/// </summary>
	/// <param name="server"></param>
	public static void ApplyCTTMode(TextWriter output, StandardServer server)
	{
		var methodsToCall = new CallMethodRequestCollection();
		var alarms = "http://test.org/UA/Alarms/";
		var index = server.CurrentInstance.NamespaceUris.GetIndex(alarms);
		if (index > 0)
		{
			try
			{
				methodsToCall.Add(
					// Start the Alarms with infinite runtime
					new CallMethodRequest
					{
						MethodId = new NodeId("Alarms.Start", (ushort)index),
						ObjectId = new NodeId("Alarms", (ushort)index),
						InputArguments = new VariantCollection() { new Variant((UInt32)UInt32.MaxValue) }
					});
				var requestHeader = new RequestHeader()
				{
					Timestamp = DateTime.UtcNow,
					TimeoutHint = 10000
				};
				var context = new OperationContext(requestHeader, RequestType.Call);
				server.CurrentInstance.NodeManager.Call(context, methodsToCall, out CallMethodResultCollection results, out DiagnosticInfoCollection diagnosticInfos);
				foreach (var result in results)
				{
					if (ServiceResult.IsBad(result.StatusCode))
					{
						Opc.Ua.Utils.LogError("Error calling method {0}.", result.StatusCode);
					}
				}
				output.WriteLine("The Alarms for CTT mode are active.");
				return;
			}
			catch (Exception ex)
			{
				Opc.Ua.Utils.LogError(ex, "Failed to start alarms for CTT.");
			}
		}
		output.WriteLine("The alarms could not be enabled for CTT, the namespace does not exist.");
	}

	/// <summary>
	/// Enumerates all node manager factories.
	/// </summary>
	/// <returns></returns>
	private static IList<INodeManagerFactory> GetNodeManagerFactories()
	{
		var assembly = typeof(Utils).Assembly;
		var nodeManagerFactories = assembly.GetExportedTypes().Select(type => IsINodeManagerFactoryType(type)).Where(type => type != null);
		return nodeManagerFactories.ToList();
	}

	/// <summary>
	/// Helper to determine the INodeManagerFactory by reflection.
	/// </summary>
	private static INodeManagerFactory IsINodeManagerFactoryType(Type type)
	{
		var nodeManagerTypeInfo = type.GetTypeInfo();
		if (nodeManagerTypeInfo.IsAbstract ||
			!typeof(INodeManagerFactory).IsAssignableFrom(type))
		{
			return null;
		}
		return Activator.CreateInstance(type) as INodeManagerFactory;
	}

}
