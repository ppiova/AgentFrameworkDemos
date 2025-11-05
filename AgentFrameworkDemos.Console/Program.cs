// Launcher for all demos: choose a sample by number or pass --sample <n|key>
using System.Diagnostics;
using DotNetEnv;

internal class Program
{
	private sealed record Sample(int Id, string Key, string Title, string CsprojRelativePath, bool RequiresAzure = true, bool RequiresOllama = false);
	private sealed record Options(string? SampleToken, bool ListOnly, bool ShowHelp, bool DryRun, string[] PassThroughArgs);
	private sealed record EnvInfo(string? Endpoint, string? Deployment);
	private sealed record OllamaInfo(string Endpoint, bool FromEnv);

	private static readonly Sample[] Samples =
	{
		new Sample(1, "basic",       "(1) Basic – Agente simple + streaming",                           @"samples\Agents\Basic\AgentDemos.Agents.Basic.csproj"),
		new Sample(2, "tools",       "(2) Tools – Function tools",                                        @"samples\Agents\Tools\AgentDemos.Agents.Tools.csproj"),
		new Sample(3, "multiturn",   "(3) MultiTurn – AgentThread en memoria",                           @"samples\Agents\MultiTurn\AgentDemos.Agents.MultiTurn.csproj"),
		new Sample(4, "telemetry",   "(4) Telemetry – OpenTelemetry",                                     @"samples\Telemetry\Basic\AgentDemos.Telemetry.Basic.csproj"),
		new Sample(5, "persistence", "(5) Persistence – Serializa/Deserializa AgentThread",               @"samples\Agents\Persistence\AgentDemos.Agents.Persistence.csproj"),
		new Sample(6, "branching",   "(6) Threads/Branching – Checkpoint + ramas divergentes",           @"samples\Agents\Threads\Branching\AgentDemos.Agents.Threads.Branching.csproj"),
		new Sample(7, "workflow",    "(7) Workflow – Researcher → Writer (secuencial)",                   @"samples\Workflows\Sequential\AgentDemos.Workflows.Sequential.csproj"),
		new Sample(8, "a2a",         "(8) A2A – Agent-to-Agent as Tools",                                @"samples\Agents\A2A\AgentDemos.Agents.A2A.csproj"),
		new Sample(9, "mcp",         "(9) MCP – Integración básica (mock)",                              @"samples\Integrations\MCP\Basic\AgentDemos.Integrations.MCP.Basic.csproj"),
		new Sample(10, "ollama",     "(10) Ollama – GPT-OSS 20B local",                                  @"samples\Integrations\Ollama\AgentDemos.Integrations.Ollama.csproj", false, true),
	};

	private static async Task<int> Main(string[] args)
	{
		try
		{
			var root = FindSolutionRoot() ?? Directory.GetCurrentDirectory();
			LoadEnvIfPresent(root);

			var options = ParseArgs(args);
			if (options.ShowHelp)
			{
				PrintUsage();
				return 0;
			}
			if (options.ListOnly)
			{
				PrintSamples();
				return 0;
			}

			var sample = ResolveSample(options.SampleToken) ?? PromptForSample();
			if (sample is null) return 2;

			var csprojPath = Path.Combine(root, sample.CsprojRelativePath);
			if (!File.Exists(csprojPath))
			{
				WriteError($"No se encontró el proyecto: {csprojPath}");
				return 3;
			}

			EnvInfo? azureEnv = null;
			OllamaInfo? ollamaInfo = null;

			if (sample.RequiresAzure)
			{
				azureEnv = GetAzureOpenAIEnv();
				WarnMissingAzure(azureEnv);
			}

			if (sample.RequiresOllama)
			{
				ollamaInfo = GetOllamaInfo();
				WarnMissingOllama(ollamaInfo);
			}

			PrintLaunchInfo(sample, csprojPath, azureEnv, ollamaInfo);

			if (options.DryRun)
			{
				Console.WriteLine("(dry-run) No se ejecuta el proceso.");
				return 0;
			}

			var exitCode = await RunDotnetRunAsync(root, csprojPath, options.PassThroughArgs);
			Console.WriteLine($"\nProceso finalizado con código {exitCode}. Fin de la ejecución");
			return exitCode;
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"Error: {ex.Message}");
			Console.Error.WriteLine(ex);
			return 1;
		}
	}

	private static Options ParseArgs(string[] args)
	{
		if (args.Length == 0) return new Options(null, false, false, false, Array.Empty<string>());

		var passThrough = Array.Empty<string>();
		var tokens = args;
		var separator = Array.IndexOf(args, "--");
		if (separator >= 0)
		{
			passThrough = args[(separator + 1)..];
			tokens = args[..separator];
		}

		string? sample = null;
		var list = false;
		var help = false;
		var dry = false;

		for (var i = 0; i < tokens.Length; i++)
		{
			var token = tokens[i];
			switch (token.ToLowerInvariant())
			{
				case "--sample":
				case "-s":
					if (i + 1 < tokens.Length)
					{
						sample = tokens[++i];
					}
					break;
				case "--list":
				case "-l":
					list = true;
					break;
				case "--help":
				case "-h":
				case "/?":
					help = true;
					break;
				case "--dry-run":
					dry = true;
					break;
				default:
					if (string.IsNullOrEmpty(sample))
					{
						sample = token;
					}
					break;
			}
		}

		return new Options(sample, list, help, dry, passThrough);
	}

	private static Sample? ResolveSample(string? token)
	{
		if (string.IsNullOrWhiteSpace(token)) return null;
		if (int.TryParse(token, out var id))
		{
			return Samples.FirstOrDefault(s => s.Id == id);
		}
		var key = token.Trim().ToLowerInvariant();
		return Samples.FirstOrDefault(s => s.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
	}

	private static void PrintLaunchInfo(Sample sample, string csprojPath, EnvInfo? azureEnv, OllamaInfo? ollama)
	{
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine($"\nEjecutando: {sample.Title}");
		Console.WriteLine(csprojPath);
		if (azureEnv is { Endpoint: { } endpoint })
		{
			Console.WriteLine($"Endpoint: {endpoint}");
		}
		if (azureEnv is { Deployment: { } deployment })
		{
			Console.WriteLine($"Deployment: {deployment}");
		}
		if (ollama is { } local)
		{
			Console.WriteLine($"Ollama endpoint: {local.Endpoint}");
			Console.WriteLine("Modelo: gpt-oss:20b");
		}
		Console.ResetColor();
	}

	private static void PrintUsage()
	{
		Console.WriteLine("Microsoft Agent Framework – Launcher\n");
		Console.WriteLine("Uso:");
		Console.WriteLine("  dotnet run -- [opciones] [id|clave] [-- args_para_sample]\n");
		Console.WriteLine("Opciones:");
		Console.WriteLine("  -l, --list            Lista los samples disponibles y sale.");
		Console.WriteLine("  -s, --sample VAL      Selecciona el sample por número o clave.");
		Console.WriteLine("  -h, --help            Muestra esta ayuda.");
		Console.WriteLine("      --dry-run         Muestra lo que se ejecutaría y sale.\n");
		Console.WriteLine("Ejemplos:");
		Console.WriteLine("  dotnet run -- -l");
		Console.WriteLine("  dotnet run -- -s 3");
		Console.WriteLine("  dotnet run -- multiturn");
		Console.WriteLine("  dotnet run -- -s tools -- --verbose");
	}

	private static void PrintSamples()
	{
		Console.WriteLine("Samples disponibles:\n");
		foreach (var sample in Samples)
		{
			Console.WriteLine($"  {sample.Id}. {sample.Title}  [{sample.Key}]");
		}
	}

	private static Sample? PromptForSample()
	{
		Console.WriteLine("Microsoft Agent Framework – Launcher\n");
		Console.WriteLine("Selecciona un ejemplo para ejecutar:\n");
		PrintSamples();
		Console.WriteLine("\nIngresa el número o clave (p.ej. 3 o 'multiturn'). Deja vacío para cancelar.");
		Console.Write("> ");
		var input = Console.ReadLine();
		return ResolveSample(input);
	}

	private static void LoadEnvIfPresent(string root)
	{
		var envPath = Path.Combine(root, ".env");
		if (File.Exists(envPath))
		{
			Env.Load(envPath);
		}
	}

	private static EnvInfo GetAzureOpenAIEnv()
	{
		var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
		var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME");
		return new EnvInfo(endpoint, deployment);
	}

	private static void WarnMissingAzure(EnvInfo env)
	{
		if (string.IsNullOrWhiteSpace(env.Endpoint))
		{
			WriteWarning("Advertencia: AZURE_OPENAI_ENDPOINT no está definido. Cárgalo en .env o como variable de entorno.");
		}
	}

	private static OllamaInfo GetOllamaInfo()
	{
		var raw = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT");
		return string.IsNullOrWhiteSpace(raw)
			? new OllamaInfo("http://localhost:11434", false)
			: new OllamaInfo(raw.Trim(), true);
	}

	private static void WarnMissingOllama(OllamaInfo info)
	{
		if (!info.FromEnv)
		{
			WriteWarning($"Usando OLLAMA_ENDPOINT por defecto: {info.Endpoint}. Asegúrate de que Ollama esté corriendo con el modelo gpt-oss:20b (set OLLAMA_ENDPOINT para cambiarlo).");
		}
	}

	private static string? FindSolutionRoot()
	{
		var current = new DirectoryInfo(AppContext.BaseDirectory);
		while (current != null)
		{
			if (current.GetFiles("*.sln").Any())
			{
				return current.FullName;
			}
			current = current.Parent;
		}
		return null;
	}

	private static async Task<int> RunDotnetRunAsync(string workingDir, string csprojFullPath, string[] passThrough)
	{
		var extraArgs = passThrough.Length > 0 ? " -- " + string.Join(' ', passThrough.Select(QuoteArg)) : string.Empty;

		var startInfo = new ProcessStartInfo
		{
			FileName = "dotnet",
			Arguments = $"run --project \"{csprojFullPath}\"{extraArgs}",
			WorkingDirectory = workingDir,
			UseShellExecute = false,
			RedirectStandardOutput = false,
			RedirectStandardError = false,
		};

		using var process = Process.Start(startInfo);
		if (process is null) return 4;

		Console.CancelKeyPress += (_, e) =>
		{
			try
			{
				if (!process.HasExited)
				{
					process.Kill(entireProcessTree: true);
				}
			}
			catch
			{
				// swallow cleanup errors
			}
			e.Cancel = true;
		};

		await process.WaitForExitAsync();
		return process.ExitCode;
	}

	private static string QuoteArg(string value)
	{
		return value.Contains(' ') || value.Contains('"')
			? "\"" + value.Replace("\"", "\\\"") + "\""
			: value;
	}

	private static void WriteError(string message)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.Error.WriteLine(message);
		Console.ResetColor();
	}

	private static void WriteWarning(string message)
	{
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine(message);
		Console.ResetColor();
	}
}
