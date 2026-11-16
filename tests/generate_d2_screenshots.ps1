$rootDir = Split-Path -Parent $PSScriptRoot
Set-Location $rootDir
$exe = Join-Path $rootDir "dist\Tweaker.exe"
$docsProto = Join-Path $rootDir "docs\prototype"
if (-not (Test-Path $docsProto)) { New-Item -ItemType Directory -Path $docsProto | Out-Null }
$artDir = "C:\Users\User\.gemini\antigravity\brain\6094a221-bf4d-4b38-be73-a0487ddbba7b"

$shots = @(
    @{ Name = "d2-overview-dark.png"; Args = @("--screenshot", (Join-Path $docsProto "d2-overview-dark.png"), "--tab", "overview") },
    @{ Name = "d2-overview-light.png"; Args = @("--light", "--screenshot", (Join-Path $docsProto "d2-overview-light.png"), "--tab", "overview") },
    @{ Name = "d2-tweaks-dark.png"; Args = @("--screenshot", (Join-Path $docsProto "d2-tweaks-dark.png"), "--tab", "tweaks", "--expand-card") },
    @{ Name = "d2-tweaks-light.png"; Args = @("--light", "--screenshot", (Join-Path $docsProto "d2-tweaks-light.png"), "--tab", "tweaks", "--expand-card") },
    @{ Name = "d2-journal-dark.png"; Args = @("--screenshot", (Join-Path $docsProto "d2-journal-dark.png"), "--tab", "journal") },
    @{ Name = "d2-journal-light.png"; Args = @("--light", "--screenshot", (Join-Path $docsProto "d2-journal-light.png"), "--tab", "journal") },
    @{ Name = "d2-settings-dark.png"; Args = @("--screenshot", (Join-Path $docsProto "d2-settings-dark.png"), "--tab", "settings") },
    @{ Name = "d2-settings-light.png"; Args = @("--light", "--screenshot", (Join-Path $docsProto "d2-settings-light.png"), "--tab", "settings") },
    @{ Name = "d2-about-dark.png"; Args = @("--screenshot", (Join-Path $docsProto "d2-about-dark.png"), "--tab", "about") }
)

Add-Type -AssemblyName System.Drawing

$table = @()

foreach ($s in $shots) {
    $outPath = Join-Path $docsProto $s.Name
    Remove-Item $outPath -ErrorAction SilentlyContinue
    $p = Start-Process -FilePath $exe -ArgumentList $s.Args -PassThru -Wait
    Start-Sleep -Milliseconds 400

    if (Test-Path $outPath) {
        if (Test-Path $artDir) {
            $artPath = Join-Path $artDir $s.Name
            Copy-Item $outPath $artPath -Force
        }

        $fi = Get-Item $outPath
        $hash = (Get-FileHash $outPath -Algorithm SHA256).Hash

        $img = [System.Drawing.Image]::FromFile($outPath)
        $dim = "{0}x{1}" -f $img.Width, $img.Height
        $img.Dispose()

        $table += [PSCustomObject]@{
            "Name" = $s.Name
            "Length" = $fi.Length
            "Resolution" = $dim
            "LastWriteTime" = $fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
            "SHA256" = $hash
        }
    } else {
        Write-Error "Failed to generate screenshot $($s.Name)"
    }
}

$table | Format-Table -AutoSize
