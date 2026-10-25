<#
.SYNOPSIS
    Repository verification runner for unity-ui-agent-tools.
.DESCRIPTION
    Runs fast lints and full multi-runtime unit, schema, and meta-file test suites.
#>

param(
    [ValidateSet("fast", "full")]
    [string]$Mode = "full"
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "=== Starting unity-ui-agent-tools Verification ($Mode mode) ===" -ForegroundColor Cyan

# 1. Clean-room & secrets check
Write-Host "`n[Step 1/5] Checking clean-room and secrets baseline..." -ForegroundColor Yellow
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot "../tools/guard.ps1")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# 2. Meta-file integrity check (Unity XR requirement)
Write-Host "`n[Step 2/5] Checking Unity .meta file integrity..." -ForegroundColor Yellow
$MissingMeta = 0
$AssetRoots = @(
    (Join-Path $RepoRoot "Packages/io.github.thesanketkale.ui-context"),
    (Join-Path $RepoRoot "Projects/Sample/Assets")
)

foreach ($Root in $AssetRoots) {
    if (Test-Path $Root) {
        $Items = Get-ChildItem -Path $Root -Recurse | Where-Object {
            $_.FullName -notmatch "\.meta$" -and
            $_.Name -notmatch "~$" -and
            $_.FullName -notmatch "[\/\\](bin|obj|Documentation~)[\/\\]"
        }
        foreach ($Item in $Items) {
            $ExpectedMeta = "$($Item.FullName).meta"
            if (-not (Test-Path $ExpectedMeta)) {
                Write-Host "Missing .meta file for: $($Item.FullName)" -ForegroundColor Red
                $MissingMeta++
            }
        }
    }
}

if ($MissingMeta -gt 0) {
    Write-Host "FAILED: $MissingMeta missing .meta file(s) found!" -ForegroundColor Red
    exit 1
}
Write-Host "Unity .meta check PASSED." -ForegroundColor Green

# 3. Workflow syntax check
Write-Host "`n[Step 3/5] Validating GitHub Actions workflow schemas..." -ForegroundColor Yellow
$WorkflowsDir = Join-Path $RepoRoot ".github/workflows"
if (Test-Path $WorkflowsDir) {
    $WfFiles = Get-ChildItem -Path $WorkflowsDir -Filter "*.yml"
    foreach ($Wf in $WfFiles) {
        # Basic validation: ensure non-empty and well-formed headers
        $Content = Get-Content $Wf.FullName -Raw
        if ($Content -notmatch "name:" -or $Content -notmatch "on:") {
            Write-Host "Invalid workflow file: $($Wf.Name)" -ForegroundColor Red
            exit 1
        }
    }
    Write-Host "Workflow syntax check PASSED ($($WfFiles.Count) workflows verified)." -ForegroundColor Green
} else {
    Write-Host "No workflows directory found yet (skipping in early milestones)." -ForegroundColor Gray
}

if ($Mode -eq "fast") {
    Write-Host "`n=== Verification PASSED (fast mode complete) ===" -ForegroundColor Green
    exit 0
}

# 4. .NET Core engine unit tests
Write-Host "`n[Step 4/5] Running .NET Core test suite..." -ForegroundColor Yellow
$TestCsproj = Join-Path $RepoRoot "tests/UiContext.Core.Tests/UiContext.Core.Tests.csproj"
if (Test-Path $TestCsproj) {
    dotnet test $TestCsproj -c Release --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "FAILED: .NET Core unit tests failed!" -ForegroundColor Red
        exit $LASTEXITCODE
    }
    Write-Host ".NET Core test suite PASSED." -ForegroundColor Green
} else {
    Write-Host "Test project not yet created (skipping in early milestones)." -ForegroundColor Gray
}

# 5. TypeScript CLI and MCP test suite
Write-Host "`n[Step 5/5] Running TypeScript CLI & MCP test suite..." -ForegroundColor Yellow
$CliDir = Join-Path $RepoRoot "cli"
$CliPackageJson = Join-Path $CliDir "package.json"
if (Test-Path $CliPackageJson) {
    Push-Location $CliDir
    try {
        if (-not (Test-Path "node_modules")) {
            Write-Host "Installing CLI dependencies..." -ForegroundColor Gray
            npm install --silent
        }
        npm test
        if ($LASTEXITCODE -ne 0) {
            Write-Host "FAILED: CLI and MCP tests failed!" -ForegroundColor Red
            exit $LASTEXITCODE
        }
    }
    finally {
        Pop-Location
    }
    Write-Host "CLI & MCP test suite PASSED." -ForegroundColor Green
} else {
    Write-Host "CLI package not yet created (skipping in early milestones)." -ForegroundColor Gray
}

Write-Host "`n=== Verification PASSED (full mode complete) ===" -ForegroundColor Green
exit 0
