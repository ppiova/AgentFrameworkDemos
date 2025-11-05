// Multi-turn conversation sample using Microsoft Agent Framework with Azure OpenAI
// Demonstrates explicit AgentThread usage to preserve context across turns.
// Requires: AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_DEPLOYMENT_NAME env vars

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
	?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
	.GetChatClient(deployment)
	.AsIChatClient()
	.CreateAIAgent(
		instructions: "Sos 'GauchoGuide', un asistente de viajes ingenioso. Respondé en español con un toque argentino suave (algún 'che' o 'dale' cuando surja). Hacé 1–2 preguntas de aclaración antes de proponer un itinerario. Sé conciso y práctico.",
		name: "GauchoGuide");

// Start an explicit AgentThread to maintain conversation state
AgentThread thread = agent.GetNewThread();

Console.WriteLine("> Usuario: Quiero visitar España la próxima primavera.\n");
Console.WriteLine(await agent.RunAsync("Quiero visitar España la próxima primavera.", thread));

Console.WriteLine("\n> Usuario: Me gustan el arte, la comida y las ciudades pequeñas—nada muy turístico, che.\n");
Console.WriteLine(await agent.RunAsync("Me gustan el arte, la comida y las ciudades pequeñas—nada muy turístico, che.", thread));

Console.WriteLine("\n> Usuario: Dejalo en un plan de 4 días, con ritmo relajado.\n");
Console.WriteLine(await agent.RunAsync("Dejalo en un plan de 4 días, con ritmo relajado.", thread));

// Tip: You can also serialize this thread and resume later (see the Persistence sample)
