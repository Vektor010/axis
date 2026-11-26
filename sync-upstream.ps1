<#
.SYNOPSIS
    Синхронизация базы документации Noverse (nohuto/win-config) из актуальной структуры <category>/docs/*.md.
#>
$ErrorActionPreference = "Stop"

$repoRoot = $PSScriptRoot
$sourceBase = Join-Path $repoRoot "data\source\win-config"
New-Item -ItemType Directory -Path $sourceBase -Force | Out-Null

$categories = @(
    "system", "visibility", "network", "peripheral", "privacy",
    "security", "power", "cleanup", "misc", "nvidia", "affinities"
)

Write-Host "Fetching file tree from nohuto/win-config (main branch)..." -ForegroundColor Cyan
try {
    $treeUrl = "https://api.github.com/repos/nohuto/win-config/git/trees/main?recursive=1"
    $headers = @{ "User-Agent" = "Axis-Upstream-Sync" }
    $resp = Invoke-RestMethod -Uri $treeUrl -Headers $headers -TimeoutSec 20
}
catch {
    Write-Host "[ERROR] Failed to query GitHub tree API: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$downloaded = 0
foreach ($cat in $categories) {
    $prefix = "$cat/docs/"
    $docs = $resp.tree | Where-Object { $_.type -eq "blob" -and $_.path.StartsWith($prefix) -and $_.path.EndsWith(".md") }
    foreach ($item in $docs) {
        $relLocal = $item.path.Replace('/', '\')
        $localPath = Join-Path $sourceBase $relLocal
        $parentDir = Split-Path $localPath -Parent
        if (-not (Test-Path $parentDir)) {
            New-Item -ItemType Directory -Path $parentDir -Force | Out-Null
        }
        $rawUrl = "https://raw.githubusercontent.com/nohuto/win-config/main/$($item.path)"
        Invoke-WebRequest -Uri $rawUrl -OutFile $localPath -UseBasicParsing -TimeoutSec 20
        $downloaded++
    }
    Write-Host "  Synced $($docs.Count) docs for $cat" -ForegroundColor Green
}

Write-Host "Total upstream docs synced: $downloaded" -ForegroundColor Green
