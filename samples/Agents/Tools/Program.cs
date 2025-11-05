// Tools (function calling) Agent sample using Microsoft Agent Framework with Azure OpenAI
// Requires: AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_DEPLOYMENT_NAME env vars

using System.ComponentModel;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
	?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

// Define a simple tool as a static method on a class (so attributes are supported)
Console.WriteLine("Registering weather tool...\n");

var agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
	.GetChatClient(deployment)
	.AsIChatClient()
	.CreateAIAgent(
		instructions: "Sos 'GauchoGuide', un asistente argentino útil. Respondé en español, de forma concisa y con un toque leve. Cuando te pregunten por el clima, llamá a la herramienta de clima.",
		tools: [AIFunctionFactory.Create(WeatherTools.GetWeather)]
	);

Console.WriteLine("> Prompt: ¿Cómo está el clima en Buenos Aires?\n");
Console.WriteLine(await agent.RunAsync("¿Cómo está el clima en Buenos Aires?"));

Console.WriteLine("\n> Streaming prompt: ¿Y en Mendoza?\n");
await foreach (var update in agent.RunStreamingAsync("¿Y en Mendoza?"))
{
	Console.Write(update);
}
Console.WriteLine();

static class WeatherTools
{
	[Description("Get the weather for a given location.")]
	public static string GetWeather([Description("The location to get the weather for.")] string location)
		=> $"The weather in {location} is cloudy with a high of 22°C.";
}
