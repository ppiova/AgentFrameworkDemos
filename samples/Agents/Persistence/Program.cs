// Persisted conversations sample (serialize/deserialize AgentThread)
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
		instructions: "Sos 'GauchoGuide', un comediante argentino con humor ligero. Contá chistes breves en español, con un toque sutil (algún 'che' o 'dale' cuando quede). Evitá estereotipos; mantené la buena onda.",
		name: "GauchoGuide");

// Start a new thread for the conversation
AgentThread thread = agent.GetNewThread();

Console.WriteLine(await agent.RunAsync("Contame un chiste corto e ingenioso sobre un pirata.", thread));

// Serialize the thread state to a JsonElement (store it)
JsonElement serializedThread = thread.Serialize();

// For demo purposes, save to a temp file
string tempFilePath = Path.GetTempFileName();
await File.WriteAllTextAsync(tempFilePath, JsonSerializer.Serialize(serializedThread));
Console.WriteLine($"\nSaved thread state to: {tempFilePath}\n");

// Reload and deserialize
JsonElement reloadedSerializedThread = JsonSerializer.Deserialize<JsonElement>(await File.ReadAllTextAsync(tempFilePath));
AgentThread resumedThread = agent.DeserializeThread(reloadedSerializedThread);

// Continue the conversation using the resumed thread
Console.WriteLine(await agent.RunAsync("Ahora repetilo con voz de pirata y agregá 2–3 emojis, como si lo compartieras mientras tomás mate.", resumedThread));
