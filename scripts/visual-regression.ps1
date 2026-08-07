[CmdletBinding()]
param(
    [string]$BaseUrl = "http://localhost:5037",
    [switch]$UpdateBaseline,
    [switch]$UseExistingServer,
    [switch]$SkipBrowserInstall,
    [switch]$RunMutableFlows
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$visualProject = Join-Path $repoRoot "IkkonAdmin.VisualTests\IkkonAdmin.VisualTests.csproj"
$solution = Join-Path $repoRoot "IkkonAdmin.slnx"
$serverArtifactDirectory = Join-Path $repoRoot "artifacts\visual-regression-server"
$webProcess = $null

function Invoke-Dotnet {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet encerrou com o código $LASTEXITCODE."
    }
}

try {
    New-Item -ItemType Directory -Path $serverArtifactDirectory -Force | Out-Null

    if ($UseExistingServer) {
        Invoke-Dotnet build $visualProject
    }
    else {
        Invoke-Dotnet build $solution

        $webExecutable = Join-Path $repoRoot "IkkonAdmin.Web\bin\Debug\net10.0\IkkonAdmin.Web.exe"
        if (-not (Test-Path -LiteralPath $webExecutable)) {
            throw "Executável web não encontrado em $webExecutable."
        }

        $stdoutPath = Join-Path $serverArtifactDirectory "web.stdout.log"
        $stderrPath = Join-Path $serverArtifactDirectory "web.stderr.log"
        $webProcess = Start-Process `
            -FilePath $webExecutable `
            -ArgumentList @("--urls", $BaseUrl, "--environment", "Development") `
            -WorkingDirectory (Join-Path $repoRoot "IkkonAdmin.Web") `
            -RedirectStandardOutput $stdoutPath `
            -RedirectStandardError $stderrPath `
            -WindowStyle Hidden `
            -PassThru

        $deadline = [DateTime]::UtcNow.AddSeconds(60)
        $serverReady = $false
        while ([DateTime]::UtcNow -lt $deadline) {
            if ($webProcess.HasExited) {
                throw "A aplicação encerrou antes de ficar disponível. Consulte $stderrPath."
            }

            try {
                $response = Invoke-WebRequest `
                    -Uri ($BaseUrl.TrimEnd("/") + "/blog") `
                    -UseBasicParsing `
                    -TimeoutSec 2
                if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) {
                    $serverReady = $true
                    break
                }
            }
            catch {
                Start-Sleep -Milliseconds 500
            }
        }

        if (-not $serverReady) {
            throw "A aplicação não ficou disponível em $BaseUrl dentro de 60 segundos."
        }
    }

    $playwrightScript = Join-Path `
        $repoRoot `
        "IkkonAdmin.VisualTests\bin\Debug\net10.0\playwright.ps1"
    if (-not $SkipBrowserInstall) {
        & pwsh -NoProfile -File $playwrightScript install chromium
        if ($LASTEXITCODE -ne 0) {
            throw "A instalação do Chromium para os testes visuais falhou."
        }
    }

    $mode = if ($UpdateBaseline) { "update" } else { "compare" }
    $runnerArguments = @(
        "run",
        "--project", $visualProject,
        "--no-build",
        "--",
        $mode,
        "--base-url", $BaseUrl
    )
    if ($RunMutableFlows) {
        if ([string]::IsNullOrWhiteSpace($env:ConnectionStrings__DefaultConnection)) {
            throw "RunMutableFlows exige ConnectionStrings__DefaultConnection para criar e limpar o cenário E2E."
        }

        $runnerArguments += "--mutable-flows"
    }

    Invoke-Dotnet @runnerArguments
}
finally {
    if ($null -ne $webProcess -and -not $webProcess.HasExited) {
        Stop-Process -Id $webProcess.Id -Force
    }
}
