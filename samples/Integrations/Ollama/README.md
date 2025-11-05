# Integración Ollama con Microsoft Agent Framework

Este ejemplo demuestra cómo integrar **Microsoft Agent Framework** con **Ollama** para ejecutar modelos de IA localmente.

## 📋 Prerrequisitos

### 1. Instalar Ollama

Descargá e instalá Ollama desde: https://ollama.com/download

### 2. Descargar el modelo

```powershell
ollama pull gpt-oss:20b
```

También podés usar otros modelos como:
- `llama3.2` - Modelo Llama 3.2
- `phi3` - Microsoft Phi-3
- `mistral` - Mistral AI

### 3. Configurar variables de entorno

Copiá `.env.example` a `.env` y ajustá los valores:

```bash
cp .env.example .env
```

O configurá las variables directamente:

```powershell
$env:OLLAMA_ENDPOINT="http://localhost:11434"
$env:OLLAMA_MODEL_NAME="gpt-oss:20b"
```

## 🚀 Ejecutar el ejemplo

### Desde la raíz del repositorio:

```powershell
dotnet run --project samples/Integrations/Ollama/AgentDemos.Integrations.Ollama.csproj
```

### Desde el directorio del proyecto:

```powershell
cd samples/Integrations/Ollama
dotnet run
```

### Usando el script helper:

```powershell
.\scripts\run.ps1 samples/Integrations/Ollama/AgentDemos.Integrations.Ollama.csproj
```

## 📦 Paquetes utilizados

- **Microsoft.Agents.AI** (1.0.0-preview.251028.1) - Core del Agent Framework
- **Microsoft.Extensions.AI** (9.10.2) - Abstracciones de AI
- **OllamaSharp** (3.0.8) - Cliente para Ollama API
- **Microsoft.Extensions.AI.Ollama** (9.7.0-preview.1.25356.2) - Extensiones Ollama

## 🔍 Qué hace el ejemplo

1. **Verso milonga**: Genera un verso estilo milonga sobre agentes en Ollama
2. **Streaming**: Genera una frase ingeniosa sobre debugging en tiempo real
3. **Tips offline**: Lista 3 consejos para demos sin conexión

## 🛠️ Troubleshooting

### Error: "Connection refused"

Asegurate de que Ollama esté corriendo:

```powershell
ollama serve
```

### Error: "Model not found"

Descargá el modelo primero:

```powershell
ollama pull gpt-oss:20b
```

### Error: "OLLAMA_ENDPOINT is not set"

Configurá las variables de entorno o creá un archivo `.env`.

## 📚 Referencias

- [Microsoft Agent Framework](https://github.com/microsoft/agent-framework)
- [Ollama](https://ollama.com)
- [OllamaSharp](https://github.com/awaescher/OllamaSharp)
- [Ejemplo oficial](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/GettingStarted/AgentProviders/Agent_With_Ollama)
