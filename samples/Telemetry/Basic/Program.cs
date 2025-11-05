// Telemetry sample: instrument an agent with OpenTelemetry via Microsoft.Extensions.AI
// Requires: AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_DEPLOYMENT_NAME env vars

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenTelemetry.Trace;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
	?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

// Configure OpenTelemetry console exporter
var sourceName = Guid.NewGuid().ToString();
using var tracerProvider = OpenTelemetry.Sdk.CreateTracerProviderBuilder()
	.AddSource(sourceName)
	.AddConsoleExporter()
	.Build();

// Wrap the chat client with telemetry using ChatClientBuilder
IChatClient instrumentedClient = new ChatClientBuilder(
		new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
			.GetChatClient(deployment)
			.AsIChatClient())
	.UseOpenTelemetry(sourceName: sourceName, configure: c => c.EnableSensitiveData = true)
	.Build();

AIAgent agent = instrumentedClient.CreateAIAgent(
	instructions: "Sos 'GauchoGuide', un asistente con un leve toque argentino. Respondé en español y de forma breve; podés usar un 'che' o 'dale' cuando quede natural.",
	name: "TelemeteredBot");

Console.WriteLine(await agent.RunAsync("In one sentence, what is Microsoft Agent Framework?"));

Console.WriteLine("\nCheck the console above for OpenTelemetry spans.");
