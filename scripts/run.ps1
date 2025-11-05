param(
  [Parameter(Mandatory=$true, HelpMessage="Relative path to the .csproj from repo root, e.g. samples/Agents/Basic/AgentDemos.Agents.Basic.csproj")] 
  [string]$Project,
  [Parameter(Mandatory=$false)]
  [string[]]$Args
)

# Determine repo root (parent of scripts folder)
$RepoRoot = Split-Path $PSScriptRoot -Parent
$EnvFile = Join-Path $RepoRoot ".env"

if (-not (Test-Path -LiteralPath $EnvFile)) {
  Write-Error ".env not found at $EnvFile. Create it or copy from .env.example."
  exit 1
}

# Load .env (KEY=VALUE pairs, ignoring comments and empty lines)
Get-Content -LiteralPath $EnvFile | ForEach-Object {
  $line = $_.Trim()
  if ($line -and -not $line.StartsWith('#')) {
    $pair = $line -split '=', 2
    if ($pair.Count -eq 2) {
      $name = $pair[0].Trim()
      $value = $pair[1].Trim()
      if ($name) { Set-Item -Path "Env:$name" -Value $value }
    }
  }
}

# Validate required variables
if (-not $env:AZURE_OPENAI_ENDPOINT -or -not $env:AZURE_OPENAI_DEPLOYMENT_NAME) {
  Write-Error "AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_DEPLOYMENT_NAME must be set in .env"
  exit 1
}

# Resolve project path
$ProjectPath = Join-Path $RepoRoot $Project
if (-not (Test-Path -LiteralPath $ProjectPath)) {
  Write-Error "Project not found: $ProjectPath"
  exit 1
}

# Optional: remind about Azure CLI login if using AzureCliCredential
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
  Write-Warning "Azure CLI (az) not found on PATH. If your samples use AzureCliCredential, install/login first."
}

# Run
Write-Host "Running: dotnet run --project `"$ProjectPath`" $Args" -ForegroundColor Cyan
& dotnet run --project $ProjectPath @Args
$exit = $LASTEXITCODE
exit $exit
