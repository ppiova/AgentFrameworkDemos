using System.ComponentModel;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

// Agent-to-Agent as Tools (A2A): expose one agent as a callable tool for another agent

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

// Create the 'MathAgent' that will do math reasoning
var mathAgent = chat.CreateAIAgent(
	instructions: "Sos un asistente de matemáticas cuidadoso. Mostrá un razonamiento breve y devolvé solo el resultado numérico cuando te lo pidan.",
	name: "MathAgent");

// Expose MathAgent as a tool by wrapping it in a static method that AIFunctionFactory can reflect
A2ATools.MathAgent = mathAgent;
var callMathTool = AIFunctionFactory.Create(A2ATools.AskMathAgent);

var writerAgent = chat.CreateAIAgent(
	instructions: "Sos 'GauchoGuide', un redactor argentino ingenioso. Mantené un toque leve (algún 'che' o 'dale' natural). Cuando necesites números precisos, llamá a la herramienta 'AskMathAgent' para calcularlos. Sé conciso y ocurrente.",
	name: "WriterAgent",
	tools: [ callMathTool ]);

Console.WriteLine("=== A2A: WriterAgent calling MathAgent as a tool ===\n");

var userPrompt = "Estoy escribiendo un artículo. ¿Cuál es el resultado de (123 * 45) + 6789? Sumá una oración corta usando ese dato, che.";
Console.WriteLine(await writerAgent.RunAsync(userPrompt));

Console.WriteLine("\n--- Llamada en streaming (WriterAgent) ---\n");
await foreach (var update in writerAgent.RunStreamingAsync("Escribí un verso de una línea que haga guiño a ese número, como si lo explicaras tomando mate."))
{
	Console.Write(update);
}
Console.WriteLine();

static class A2ATools
{
	public static AIAgent MathAgent { get; set; } = default!;

	[Description("Ask the MathAgent to solve a math question and return the result.")]
	public static async Task<string> AskMathAgent(
		[Description("The math question to ask the MathAgent.")] string question)
	{
		var res = await MathAgent.RunAsync(question);
		return res.ToString();
	}
}
