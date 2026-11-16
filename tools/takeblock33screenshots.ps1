$app = "C:\Users\User\Documents\win-tweaker\dist\Tweaker.exe"
$artDir = "C:\Users\User\.gemini\antigravity\brain\6094a221-bf4d-4b38-be73-a0487ddbba7b"

$shots = @(
    @{ cat = "overview"; out = "$artDir\ui-overview-800.png" },
    @{ cat = "all"; out = "$artDir\ui-all-800.png" },
    @{ cat = "system"; out = "$artDir\ui-system-800.png" },
    @{ cat = "network"; out = "$artDir\ui-network-800.png" },
    @{ cat = "peripheral"; out = "$artDir\ui-peripheral-800.png" }
)

foreach ($s in $shots) {
    if (Test-Path $s.out) {
        Remove-Item -Path $s.out -Recurse -Force
    }
    Write-Host "Capturing $($s.cat)..."
    Start-Process -FilePath $app -ArgumentList @("--category", $s.cat, "--screenshot", $s.out) -Wait
    if (Test-Path $s.out) {
        $size = (Get-Item $s.out).Length
        Write-Host "Saved $($s.out) ($size bytes)"
    } else {
        Write-Host "FAILED to capture $($s.out)"
    }
}
