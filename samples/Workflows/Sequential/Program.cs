// Simple sequential orchestration between two agents (Researcher -> Writer)
// Requires: AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_DEPLOYMENT_NAME env vars

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
	?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

var baseClient = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
	.GetChatClient(deployment)
	.AsIChatClient();

// Two specialized agents
var researcher = baseClient.CreateAIAgent(
	name: "Researcher",
	instructions: "Sos 'GauchoGuide-Research'. Reuní 3–5 datos clave en viñetas sobre un tema, en español. Mantenelos bien concretos; un guiño argentino sutil está bien.");

var writer = baseClient.CreateAIAgent(
	name: "Writer",
	instructions: "Sos 'GauchoGuide-Writer'. Convertí las viñetas en un párrafo conciso (<= 80 palabras) en español. Conservá un toque argentino sutil, como explicándolo con un mate de por medio.");

// Orchestration: Researcher -> Writer
string topic = "Visual Studio Code";
Console.WriteLine($"> Tema: {topic}\n");

var facts = await researcher.RunAsync($"Listá datos clave sobre {topic} enfocándote en qué es y por qué usarlo.");
Console.WriteLine("[Viñetas del Researcher]\n" + facts.Text + "\n");

var article = await writer.RunAsync($"Convertí estas viñetas en un párrafo breve:\n{facts.Text}");
Console.WriteLine("[Salida del Writer]\n" + article.Text + "\n");
