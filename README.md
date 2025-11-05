# AgentFrameworkDemos (.NET + Microsoft Agent Framework)

Ejemplos mínimos en C# para demostrar Microsoft Agent Framework con Azure OpenAI como proveedor.

## Requisitos

- .NET 8 SDK
- Una cuenta de Azure y un recurso de Azure OpenAI con un modelo desplegado (por ejemplo `gpt-4o-mini`).
- Azure CLI instalada y con sesión iniciada (`az login`).

## Configuración rápida (PowerShell)

```powershell
# Inicia sesión en Azure (se abrirá el navegador)
az login

# Establece variables de entorno para el proyecto actual
$env:AZURE_OPENAI_ENDPOINT = "https://<tu-recurso>.openai.azure.com"
$env:AZURE_OPENAI_DEPLOYMENT_NAME = "<nombre-del-despliegue>"  # p.ej. gpt-4o-mini
```

> Nota: las variables de entorno se leen en tiempo de ejecución. Puedes definirlas de forma persistente en tu sistema si lo prefieres.

## Uso recomendado: launcher + .env

1) Crea un archivo `.env` en la raíz (puedes copiar de `/.env.example`). Ejemplo:

```
AZURE_OPENAI_ENDPOINT=https://<tu-recurso>.openai.azure.com
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o-mini
```

2) Ejecuta el launcher y elige el sample por número o clave:

```powershell
dotnet run --project .\AgentFrameworkDemos.Console\AgentFrameworkDemos.Console.csproj
```

Atajos útiles:

```powershell
# Listar samples
dotnet run --project .\AgentFrameworkDemos.Console\AgentFrameworkDemos.Console.csproj -- -l

# Elegir por número
dotnet run --project .\AgentFrameworkDemos.Console\AgentFrameworkDemos.Console.csproj -- -s 3

# Elegir por clave
dotnet run --project .\AgentFrameworkDemos.Console\AgentFrameworkDemos.Console.csproj -- multiturn

# Pasar argumentos al sample (después de "--")
dotnet run --project .\AgentFrameworkDemos.Console\AgentFrameworkDemos.Console.csproj -- -s tools -- --verbose
```

El launcher carga automáticamente `/.env` (si existe) y propaga las variables al proceso del sample.

## Cómo ejecutar (directo por proyecto)

Desde la carpeta raíz del workspace:

```powershell
# Compilar
 dotnet build

# (1) Ejemplo básico (agente simple + streaming)
 dotnet run --project .\samples\Agents\Basic\AgentDemos.Agents.Basic.csproj

# (2) Herramientas (function tools)
 dotnet run --project .\samples\Agents\Tools\AgentDemos.Agents.Tools.csproj

# (3) Conversación multi-turn (AgentThread en memoria)
 dotnet run --project .\samples\Agents\MultiTurn\AgentDemos.Agents.MultiTurn.csproj

# (5) Persistencia de conversación
 dotnet run --project .\samples\Agents\Persistence\AgentDemos.Agents.Persistence.csproj

# (4) Telemetría con OpenTelemetry (spans en consola)
 dotnet run --project .\samples\Telemetry\Basic\AgentDemos.Telemetry.Basic.csproj

# (7) “Workflow” secuencial (Researcher -> Writer)
 dotnet run --project .\samples\Workflows\Sequential\AgentDemos.Workflows.Sequential.csproj

# (8) Agent-to-Agent as Tools (A2A)
 dotnet run --project .\samples\Agents\A2A\AgentDemos.Agents.A2A.csproj

# (9) MCP (Model Context Protocol) – integración básica (mock MCP)
 dotnet run --project .\samples\Integrations\MCP\Basic\AgentDemos.Integrations.MCP.Basic.csproj

# (6) AgentThread: Branching desde un checkpoint
 dotnet run --project .\samples\Agents\Threads\Branching\AgentDemos.Agents.Threads.Branching.csproj

# (10) Ollama local (gpt-oss:20b)
 dotnet run --project .\samples\Integrations\Ollama\AgentDemos.Integrations.Ollama.csproj

> Alternativa: `scripts/run.ps1` también carga `/.env` y ejecuta cualquier sample directo.
> Ejemplo:
> 
> ```powershell
> .\scripts\run.ps1 -Project "samples/Agents/Basic/AgentDemos.Agents.Basic.csproj"
> ```
```

## Estructura y ejemplos

- samples/Agents/Basic: Crea un agente con instrucciones y envía una petición normal y otra en streaming.
- samples/Agents/Tools: Registra una función .NET como herramienta (`GetWeather`) y deja que el agente la llame cuando corresponde.
- samples/Agents/MultiTurn: Mantiene el contexto de la conversación con varias rondas usando AgentThread explícito.
- samples/Agents/Persistence: Demuestra persistencia de la conversación (serializa/deserializa `AgentThread`).
- samples/Agents/Threads/Branching: Demuestra checkpoint y branching de un `AgentThread` (dos planes divergentes desde el mismo estado).
- samples/Telemetry/Basic: Integra OpenTelemetry y emite spans a consola para el flujo del agente.
- samples/Workflows/Sequential: Orquestación secuencial simple entre dos agentes (Researcher → Writer).
- samples/Agents/A2A: Patrón Agent-to-Agent como Tools (un agente expone capacidades como herramienta de otro agente).
- samples/Integrations/MCP/Basic: Ejemplo de integración con MCP (usa un cliente MCP simulado para compilar sin dependencias externas).
- samples/Integrations/Ollama: Conecta con un modelo local (gpt-oss:20b) servido por Ollama.

## Recorrido recomendado (1 → 10)

1. Basic — `samples/Agents/Basic`
	- Crea un agente simple y muestra respuesta normal y en streaming.
	- Run: `dotnet run --project .\samples\Agents\Basic\AgentDemos.Agents.Basic.csproj`
2. Tools — `samples/Agents/Tools`
	- Registra una función .NET como herramienta y deja que el agente la invoque.
	- Run: `dotnet run --project .\samples\Agents\Tools\AgentDemos.Agents.Tools.csproj`
3. MultiTurn — `samples/Agents/MultiTurn`
	- Usa `AgentThread` explícito para mantener el contexto entre turnos.
	- Run: `dotnet run --project .\samples\Agents\MultiTurn\AgentDemos.Agents.MultiTurn.csproj`
4. Telemetry — `samples/Telemetry/Basic`
	- Instrumentación con OpenTelemetry para trazas de agente.
	- Run: `dotnet run --project .\samples\Telemetry\Basic\AgentDemos.Telemetry.Basic.csproj`
5. Persistence — `samples/Agents/Persistence`
	- Serializa/deserializa `AgentThread` para reanudar conversaciones.
	- Run: `dotnet run --project .\samples\Agents\Persistence\AgentDemos.Agents.Persistence.csproj`
6. Threads/Branching — `samples/Agents/Threads/Branching`
	- Checkpoint del hilo y dos ramas divergentes desde el mismo estado.
	- Run: `dotnet run --project .\samples\Agents\Threads\Branching\AgentDemos.Agents.Threads.Branching.csproj`
7. Workflows/Sequential — `samples/Workflows/Sequential`
	- Orquestación entre dos agentes (Researcher → Writer) de forma secuencial.
	- Run: `dotnet run --project .\samples\Workflows\Sequential\AgentDemos.Workflows.Sequential.csproj`
8. Agents/A2A — `samples/Agents/A2A`
	- Un agente expone capacidades como herramienta de otro (Agent-to-Agent as Tools).
	- Run: `dotnet run --project .\samples\Agents\A2A\AgentDemos.Agents.A2A.csproj`
9. Integrations/MCP/Basic — `samples/Integrations/MCP/Basic`
	- Wrapper de herramientas MCP (mock) para integrar capacidades externas.
	- Run: `dotnet run --project .\samples\Integrations\MCP\Basic\AgentDemos.Integrations.MCP.Basic.csproj`
10. Integrations/Ollama — `samples/Integrations/Ollama`
	- Ejecuta un agente contra un modelo local expuesto por Ollama (`gpt-oss:20b`).
	- Run: `dotnet run --project .\samples\Integrations\Ollama\AgentDemos.Integrations.Ollama.csproj`

> Nota: el proyecto `AgentFrameworkDemos.Console` es el launcher recomendado cuando trabajas con varios samples.

## Paquetes clave

- `Microsoft.Agents.AI` — Extensiones del Agent Framework para .NET.
- `Azure.AI.OpenAI` — Cliente Azure OpenAI (SDK v2).
- `Azure.Identity` — Credenciales (usa `AzureCliCredential` para autenticación con `az login`).
- `Microsoft.Extensions.AI` y `Microsoft.Extensions.AI.OpenAI` — Adaptadores y utilidades para `IChatClient` y herramientas.
- `Microsoft.Extensions.AI.Ollama` — Adaptador `IChatClient` para servidores Ollama locales.

## Recursos

- Docs oficiales: https://learn.microsoft.com/agent-framework/overview/agent-framework-overview
- Repo: https://github.com/microsoft/agent-framework
- NuGet (Azure AI provider): https://www.nuget.org/packages/Microsoft.Agents.AI.AzureAI

## Problemas comunes

- Error "AZURE_OPENAI_ENDPOINT is not set": define la variable de entorno como se indica arriba.
- No tienes permisos para el recurso Azure OpenAI: asegúrate de que tu identidad tenga acceso y de estar logueado con `az login` en la suscripción correcta.
- Despliegue inexistente o nombre incorrecto: confirma el nombre del deployment en Azure OpenAI Studio/Portal.
- Ollama no responde: asegurate de haber ejecutado `ollama run gpt-oss:20b` (descarga inicial) y que `ollama serve` esté activo en `http://localhost:11434` o define `OLLAMA_ENDPOINT`.

### Notas sobre AgentThread

- MultiTurn crea un `AgentThread` explícito y lo pasa en cada `RunAsync(...)` para mantener el contexto entre turnos.
- Persistence muestra cómo serializar (`thread.Serialize()`) y deserializar (`agent.DeserializeThread(...)`) el hilo para reanudar conversaciones más tarde o en otro proceso.

### Notas sobre MCP

- Este repo incluye un ejemplo MCP “mock” para mostrar el patrón de integración sin añadir paquetes extra. Para usar un servidor MCP real:
	- Instala un servidor MCP (por ejemplo, alguno oficial o de la comunidad) y obtén su comando de arranque.
	- Reemplaza `MockMcpClient` por un cliente MCP real (o añade una implementación que hable JSON-RPC/stdio o WebSocket según el servidor) dentro del proyecto `samples/Integrations/MCP/Basic`.
	- Opcionalmente, expón herramientas específicas (por nombre y parámetros) como funciones del agente, en lugar del wrapper genérico `call_mcp_tool`.
