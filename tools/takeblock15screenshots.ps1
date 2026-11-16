$app = "C:\Users\User\Documents\win-tweaker\dist\Tweaker.exe"
$artDir = "C:\Users\User\.gemini\antigravity\brain\6094a221-bf4d-4b38-be73-a0487ddbba7b"

$shots = @(
    @{ cat = "all"; out = "$artDir\ui-all-350.png" },
    @{ cat = "system"; out = "$artDir\ui-system-350.png" },
    @{ cat = "security"; out = "$artDir\ui-security-350.png" },
    @{ cat = "privacy"; out = "$artDir\ui-privacy-350.png" },
    @{ cat = "peripheral"; out = "$artDir\ui-peripheral-350.png" },
    @{ cat = "network"; out = "$artDir\ui-network-350.png" }
)

foreach ($s in $shots) {
    Write-Host "Capturing $($s.cat)..."
    Start-Process -FilePath $app -ArgumentList "--category $($s.cat) --screenshot `"$($s.out)`"" -Wait
    if (Test-Path $s.out) {
        $size = (Get-Item $s.out).Length
        Write-Host "Saved $($s.out) ($size bytes)"
    } else {
        Write-Host "FAILED to capture $($s.out)"
    }
}
