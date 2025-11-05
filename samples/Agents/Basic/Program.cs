using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
	?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5-mini";

AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
	.GetChatClient(deployment)
	.AsIChatClient()
	.CreateAIAgent(
		instructions: @"Sos 'TácticoBot', un asistente argentino con el estilo de un director técnico de la Selección.
Hablás con pasión, estrategia y metáforas futboleras.
Usá expresiones como “esto es como armar el mediocampo”, “no te me quedés en offside”, “vamos con línea de tres si hace falta”.
Explicá conceptos tecnológicos como si fueran jugadas, entrenamientos o partidos.
Sé claro, motivador y con un toque de humor criollo.
Siempre respetuoso e inclusivo, como buen líder de vestuario.",
		name: "AsadoBot");

Console.WriteLine("> Prompt: Explicame qué es una API como si fueras el DT de la Selección\n");
var reply = await agent.RunAsync("Explicame qué es una API como si fueras el DT de la Selección.");
Console.WriteLine(reply);

Console.WriteLine("\n> Streaming prompt: Dame una arenga para antes de hacer un deploy importante\n");
await foreach (var update in agent.RunStreamingAsync("Dame una arenga para antes de hacer un deploy importante"))
{
	Console.Write(update);
}
Console.WriteLine();

Console.WriteLine("\n> Prompt:Dame 3 consejos para armar un buen equipo de desarrollo, como si fueras el DT de la Selección. Usá viñetas.\n");
var tips = await agent.RunAsync("Dame 3 consejos para armar un buen equipo de desarrollo, como si fueras el DT de la Selección. Usá viñetas.");
Console.WriteLine(tips);
