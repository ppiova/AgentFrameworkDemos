// Sample básico con Microsoft Agent Framework usando un modelo local de Ollama (gpt-oss:20b)
// Prerrequisitos:
//   1) Instalar Ollama: https://ollama.com/download
//   2) Descargar el modelo: `ollama run gpt-oss:20b` (primera vez tarda)
//   3) (Opcional) OLLAMA_ENDPOINT=http://localhost:11434
// Ejecutar: dotnet run --project samples/Integrations/Ollama/AgentDemos.Integrations.Ollama.csproj

using Microsoft.Extensions.AI;
using System.ClientModel;

var model = Environment.GetEnvironmentVariable("OLLAMA_MODEL")?.Trim();
if (string.IsNullOrWhiteSpace(model)) model = "gpt-oss:20b";

var endpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT")?.Trim();
if (string.IsNullOrWhiteSpace(endpoint)) endpoint = "http://localhost:11434";

Console.WriteLine($"Conectando a Ollama en {endpoint} con el modelo {model}...\n");

// Usar el cliente Ollama directo
IChatClient chat = new OllamaChatClient(new Uri(endpoint), model);

var agent = chat.CreateAIAgent(
	instructions: "Sos 'OllamaBasic', un asistente claro y directo. Respondé en español rioplatense con ejemplos concretos cuando sirva.",
	name: "OllamaBasic");

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
