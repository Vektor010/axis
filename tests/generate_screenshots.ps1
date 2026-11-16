$rootDir = Split-Path -Parent $PSScriptRoot
Set-Location $rootDir
$exe = Join-Path $rootDir "dist\Tweaker.exe"
$docsProto = Join-Path $rootDir "docs\prototype"
if (-not (Test-Path $docsProto)) { New-Item -ItemType Directory -Path $docsProto | Out-Null }
$artDir = "C:\Users\User\.gemini\antigravity\brain\6094a221-bf4d-4b38-be73-a0487ddbba7b"

$shots = @(
    @{ Name = "screenshot-dark.png"; Args = @("--screenshot", (Join-Path $docsProto "screenshot-dark.png")) },
    @{ Name = "screenshot-light.png"; Args = @("--light", "--screenshot", (Join-Path $docsProto "screenshot-light.png")) },
    @{ Name = "screenshot-journal.png"; Args = @("--apply-tweak", "security-dynamic-lock", "--tab", "journal", "--screenshot", (Join-Path $docsProto "screenshot-journal.png")) },
    @{ Name = "screenshot-settings.png"; Args = @("--tab", "settings", "--screenshot", (Join-Path $docsProto "screenshot-settings.png")) },
    @{ Name = "screenshot-about.png"; Args = @("--tab", "about", "--screenshot", (Join-Path $docsProto "screenshot-about.png")) }
)

Add-Type -AssemblyName System.Drawing

$table = @()

foreach ($s in $shots) {
    $outPath = Join-Path $docsProto $s.Name
    Remove-Item $outPath -ErrorAction SilentlyContinue
    $p = Start-Process -FilePath $exe -ArgumentList $s.Args -PassThru -Wait
    Start-Sleep -Milliseconds 400

    if (Test-Path $outPath) {
        # Copy to artifact directory
        $artPath = Join-Path $artDir $s.Name
        Copy-Item $outPath $artPath -Force

        $fi = Get-Item $outPath
        $hash = (Get-FileHash $outPath -Algorithm SHA256).Hash

        $img = [System.Drawing.Image]::FromFile($outPath)
        $dim = "{0}x{1}" -f $img.Width, $img.Height
        $img.Dispose()

        $table += [PSCustomObject]@{
            "Имя файла" = $s.Name
            "Путь" = $outPath
            "Размер (байт)" = $fi.Length
            "Разрешение" = $dim
            "SHA-256" = $hash
        }
    } else {
        Write-Error "Failed to generate screenshot $($s.Name)"
    }
}

$table | Format-Table -AutoSize
