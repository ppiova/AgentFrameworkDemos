using System.ComponentModel;
using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

// Minimal MCP integration sample: wrap an MCP client as an agent tool
// NOTE: This sample uses a simple mock MCP client so it compiles without external deps.
// Replace MockMcpClient with a real MCP client implementation if you have one available.

IMcpClient mcp = new MockMcpClient();

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "";
var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "";
if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(deployment))
{
	Console.WriteLine("Set AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_DEPLOYMENT_NAME env vars before running.");
	return;
}

var client = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential());
IChatClient chat = client
	.GetChatClient(deployment)
	.AsIChatClient();

// Tool wrapper via reflection: forward a tool call to the MCP client by name with JSON args
McpTools.Client = mcp;
var mcpTool = AIFunctionFactory.Create(McpTools.CallMcpTool);

var agent = chat.CreateAIAgent(
	instructions: "Sos 'GauchoGuide', un asistente en español con un toque argentino sutil. Podés invocar herramientas mediante 'CallMcpTool'. Si necesitás capacidades externas (como echo o time), elegí la herramienta adecuada.",
	name: "McpAgent",
	tools: [ mcpTool ]);

Console.WriteLine("=== MCP integration (mock) ===\n");

// Show available tools (from mock)
var tools = await mcp.ListToolsAsync(default);
Console.WriteLine("MCP tools available: " + string.Join(", ", tools.Select(t => t.Name)) + "\n");

// Ask the agent to use MCP echo tool
var prompt = "Usá MCP para hacer echo del texto 'hola MCP' llamando a CallMcpTool si hace falta, che.";
Console.WriteLine(await agent.RunAsync(prompt));

// Direct tool call via the agent is also possible if the model decides to use it during reasoning.

// --------- Minimal interfaces and mock client ---------
public interface IMcpClient
{
	Task<IReadOnlyList<McpToolInfo>> ListToolsAsync(CancellationToken cancellationToken);
	Task<string> CallToolAsync(string toolName, string argsJson, CancellationToken cancellationToken);
}

public sealed record McpToolInfo(string Name, string Description);

public sealed class MockMcpClient : IMcpClient
{
	private static readonly IReadOnlyList<McpToolInfo> _tools = new[]
	{
		new McpToolInfo("echo", "Echo text back"),
		new McpToolInfo("time", "Return current UTC time"),
	};

	public Task<IReadOnlyList<McpToolInfo>> ListToolsAsync(CancellationToken cancellationToken)
		=> Task.FromResult(_tools);

	public Task<string> CallToolAsync(string toolName, string argsJson, CancellationToken cancellationToken)
	{
		toolName = toolName?.Trim().ToLowerInvariant() ?? string.Empty;
		try
		{
			using var doc = string.IsNullOrWhiteSpace(argsJson) ? null : JsonDocument.Parse(argsJson);
			return Task.FromResult(toolName switch
			{
				"echo" => Echo(doc),
				"time" => DateTimeOffset.UtcNow.ToString("O"),
				_ => $"Unknown MCP tool: {toolName}"
			});
		}
		catch (Exception ex)
		{
			return Task.FromResult($"Invalid args JSON or tool error: {ex.Message}");
		}

		static string Echo(JsonDocument? doc)
		{
			var text = doc?.RootElement.TryGetProperty("text", out var v) == true ? v.GetString() : "";
			return text ?? string.Empty;
		}
	}
}

static class McpTools
{
	public static IMcpClient Client { get; set; } = default!;

	[Description("Call a tool on the configured MCP client.")]
	public static async Task<string> CallMcpTool(
		[Description("Name of the MCP tool")] string toolName,
		[Description("JSON string of parameters to pass to the MCP tool")] string argsJson)
	{
		return await Client.CallToolAsync(toolName, argsJson, CancellationToken.None);
	}
}
