# Check upstream nohuto/win-config & noverse.dev for new commits or changed files
$ErrorActionPreference = "Stop"

$repoRoot = $PSScriptRoot
$sourceDir = Join-Path $repoRoot "data\source\win-config"

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "     CHECKING UPSTREAM SOURCES (nohuto/win-config & noverse.dev)  " -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan

# 1. Check latest commit on GitHub
try {
    $latestCommit = Invoke-RestMethod -Uri "https://api.github.com/repos/nohuto/win-config/commits/main" -Headers @{ "User-Agent" = "WinTweaker-Sync" }
    Write-Host "Latest upstream commit: $($latestCommit.sha.Substring(0,7)) ($($latestCommit.commit.author.date)) - $($latestCommit.commit.message)" -ForegroundColor Green
} catch {
    Write-Host "[WARN] Could not query GitHub API: $($_.Exception.Message)" -ForegroundColor Yellow
}

# 2. Check noverse.dev docs availability
try {
    $siteResp = Invoke-WebRequest -Uri "https://noverse.dev/docs/win-config/" -UseBasicParsing -TimeoutSec 10
    Write-Host "Site https://noverse.dev/docs/win-config/ -> HTTP $($siteResp.StatusCode) ($($siteResp.Content.Length) bytes)" -ForegroundColor Green
} catch {
    Write-Host "[WARN] Could not reach noverse.dev: $($_.Exception.Message)" -ForegroundColor Yellow
}

# 3. Compare local desc.md files against upstream main
Write-Host "`n--- Comparing category desc.md files ---" -ForegroundColor Yellow
$dirs = Get-ChildItem -Directory $sourceDir | Select-Object -ExpandProperty Name
$diffCount = 0

foreach ($cat in $dirs) {
    if ($cat -eq "steam") { continue } # Custom Noverse-style Steam module
    $localFile = Join-Path $sourceDir "$cat\desc.md"
    if (-not (Test-Path $localFile)) { continue }

    $url = "https://raw.githubusercontent.com/nohuto/win-config/main/$cat/desc.md"
    try {
        $remote = (Invoke-WebRequest -UseBasicParsing $url).Content.Replace("`r`n","`n").Trim()
        $local = [System.IO.File]::ReadAllText($localFile, [System.Text.Encoding]::UTF8).Replace("`r`n","`n").Trim()

        # Strip known local appendix blocks for nvidia and visibility before comparison
        if ($cat -eq "nvidia" -and $local.Contains("// NVIDIA > Performance : Hardware Accelerated GPU Scheduling")) {
            $local = $local.Substring(0, $local.IndexOf("// NVIDIA > Performance : Hardware Accelerated GPU Scheduling")).Trim()
        }
        if ($cat -eq "visibility" -and $local.Contains("// Visibility > Animations : Do not allow window animations (DWM)")) {
            $local = $local.Substring(0, $local.IndexOf("// Visibility > Animations : Do not allow window animations (DWM)")).Trim()
        }

        if ($remote -eq $local) {
            Write-Host "  [UP-TO-DATE] $cat/desc.md" -ForegroundColor Green
        } else {
            Write-Host "  [CHANGED]    $cat/desc.md (Local: $($local.Length), Remote: $($remote.Length))" -ForegroundColor Magenta
            $diffCount++
        }
    } catch {
        Write-Host "  [INFO]       $cat/desc.md -> $($_.Exception.Message)" -ForegroundColor DarkGray
    }
}

# 4. Compare upstream .ps1 scripts
Write-Host "`n--- Comparing upstream PowerShell scripts ---" -ForegroundColor Yellow
$ps1Files = Get-ChildItem -Path $sourceDir -Recurse -Filter "*.ps1"
foreach ($f in $ps1Files) {
    $rel = $f.FullName.Substring($sourceDir.Length + 1).Replace("\", "/")
    $url = "https://raw.githubusercontent.com/nohuto/win-config/main/$rel"
    try {
        $remote = (Invoke-WebRequest -UseBasicParsing $url).Content.Replace("`r`n","`n").Trim()
        $local = [System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8).Replace("`r`n","`n").Trim()
        if ($remote -eq $local) {
            Write-Host "  [UP-TO-DATE] $rel" -ForegroundColor Green
        } else {
            Write-Host "  [CHANGED]    $rel" -ForegroundColor Magenta
            $diffCount++
        }
    } catch {
        Write-Host "  [INFO]       $rel -> $($_.Exception.Message)" -ForegroundColor DarkGray
    }
}

Write-Host "`n==================================================================" -ForegroundColor Cyan
if ($diffCount -eq 0) {
    Write-Host "[OK] Local database is 100% synchronized with latest upstream!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "[UPDATE NEEDED] Found $diffCount file(s) with upstream changes." -ForegroundColor Yellow
    exit 2
}
