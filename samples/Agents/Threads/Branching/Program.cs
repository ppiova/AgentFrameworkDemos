// AgentThread Branching sample
// Demonstrates checkpointing a conversation with Serialize() and creating two diverging branches via Deserialize().
// Requires: AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_DEPLOYMENT_NAME env vars

using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
	?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

// Create the agent
AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
	.GetChatClient(deployment)
	.AsIChatClient()
	.CreateAIAgent(
		instructions: "Sos 'GauchoGuide', un planificador de viajes conciso en español con un toque argentino sutil. Mantené sugerencias prácticas y al grano.",
		name: "Planner");

// Start a thread and get an initial plan
AgentThread thread = agent.GetNewThread();
Console.WriteLine("> Semilla: Armá un itinerario de 3 días para Ciudad de México con onda local, dale.\n");
Console.WriteLine(await agent.RunAsync("Armá un itinerario de 3 días para Ciudad de México con onda local.", thread));

// Take a checkpoint (serialize thread state)
JsonElement checkpoint = thread.Serialize();
Console.WriteLine("\n[Checkpoint created]\n");

// Branch A: focus on museums and art
AgentThread branchA = agent.DeserializeThread(checkpoint);
Console.WriteLine("\n=== Branch A: Museums and Art ===\n");
Console.WriteLine(await agent.RunAsync("Refine the plan focusing on museums and art.", branchA));

// Branch B: focus on street food and markets
AgentThread branchB = agent.DeserializeThread(checkpoint);
Console.WriteLine("\n=== Branch B: Street Food and Markets ===\n");
Console.WriteLine(await agent.RunAsync("Refine the plan focusing on street food and markets.", branchB));

// Optional: continue Branch A one more turn (streaming)
Console.WriteLine("\n--- Rama A (streaming): Agregá una joyita oculta por día ---\n");
await foreach (var update in agent.RunStreamingAsync("Agregá una joyita oculta por día.", branchA))
{
	Console.Write(update);
}
Console.WriteLine();
