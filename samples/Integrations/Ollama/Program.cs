// Sample básico con Microsoft Agent Framework usando un modelo local de Ollama
// Basado en: https://github.com/microsoft/agent-framework/blob/main/dotnet/samples/GettingStarted/AgentProviders/Agent_With_Ollama/Program.cs
// Prerrequisitos:
//   1) Instalar Ollama: https://ollama.com/download
//   2) Descargar el modelo: `ollama pull gpt-oss:20b` (primera vez tarda)
//   3) Variables de entorno: OLLAMA_ENDPOINT y OLLAMA_MODEL_NAME
// Ejecutar: dotnet run --project samples/Integrations/Ollama/AgentDemos.Integrations.Ollama.csproj

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;

var endpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT")?.Trim();
if (string.IsNullOrWhiteSpace(endpoint)) endpoint = "http://localhost:11434";

var modelName = Environment.GetEnvironmentVariable("OLLAMA_MODEL_NAME")?.Trim();
if (string.IsNullOrWhiteSpace(modelName)) modelName = "gpt-oss:20b";

Console.WriteLine($"Conectando a Ollama en {endpoint} con el modelo {modelName}...\n");

// Get a chat client for Ollama and use it to construct an AIAgent.
AIAgent agent = new OllamaApiClient(new Uri(endpoint), modelName)
	.CreateAIAgent(instructions: "Sos 'OllamaBasic', un asistente claro y directo. Respondé en español rioplatense con ejemplos concretos cuando sirva.", name: "OllamaBasic");

Console.WriteLine("> Prompt: Escribí un verso breve, estilo milonga, sobre un agente corriendo en tu máquina con Ollama.\n");
var reply = await agent.RunAsync("Escribí un verso breve, estilo milonga, sobre un agente corriendo en tu máquina con Ollama.");
Console.WriteLine(reply);

Console.WriteLine("\n> Streaming prompt: Dame 1 frase ingeniosa sobre depurar código, como si lo explicaras con un mate en la mano.\n");
await foreach (var update in agent.RunStreamingAsync("Dame 1 frase ingeniosa sobre depurar código, como si lo explicaras con un mate en la mano."))
{
	Console.Write(update);
}
Console.WriteLine();

Console.WriteLine("\n> Prompt: Dame 3 tips concisos para preparar una demo offline. Usá viñetas.\n");
var tips = await agent.RunAsync("Dame 3 tips concisos para preparar una demo offline. Usá viñetas.");
Console.WriteLine(tips);
