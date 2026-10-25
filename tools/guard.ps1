<#
.SYNOPSIS
    Repository integrity, clean-room, and secret guard.
.DESCRIPTION
    Scans tracked and working tree files for unauthorized client references,
    private corporate entities, and credentials.
#>

param(
    [string]$TargetDir = "."
)

$ErrorActionPreference = "Stop"
$ForbiddenPatterns = @(
    "VueXR",
    "Trium Designs",
    "Fudge3d",
    "BEGIN (RSA|OPENSSH|EC|DSA)? PRIVATE KEY",
    "ghp_[A-Za-z0-9_]{36}",
    "AKIA[0-9A-Z]{16}"
)

Write-Host "Running repository clean-room and secret guard..." -ForegroundColor Cyan

$Files = Get-ChildItem -Path $TargetDir -Recurse -File | Where-Object {
    $_.FullName -notmatch "[\/\\]\.git[\/\\]" -and
    $_.FullName -notmatch "[\/\\]\.pipeline[\/\\]" -and
    $_.FullName -notmatch "[\/\\]node_modules[\/\\]" -and
    $_.FullName -notmatch "[\/\\]bin[\/\\]" -and
    $_.FullName -notmatch "[\/\\]obj[\/\\]" -and
    $_.FullName -notmatch "[\/\\]Library[\/\\]" -and
    $_.FullName -notmatch "tools[\/\\]guard\.(ps1|sh)"
}

$Violations = 0

foreach ($Pattern in $ForbiddenPatterns) {
    foreach ($File in $Files) {
        $Matches = Select-String -Path $File.FullName -Pattern $Pattern -CaseSensitive:$false
        if ($Matches) {
            Write-Host "GUARD VIOLATION: Pattern '$Pattern' found in $($File.FullName):" -ForegroundColor Red
            foreach ($Match in $Matches) {
                Write-Host "  Line $($Match.LineNumber): $($Match.Line.Trim())" -ForegroundColor Yellow
            }
            $Violations++
        }
    }
}

if ($Violations -gt 0) {
    Write-Host "Guard check FAILED with $Violations violation(s)." -ForegroundColor Red
    exit 1
}

Write-Host "Guard check PASSED: Clean-room and secret requirements satisfied." -ForegroundColor Green
exit 0
