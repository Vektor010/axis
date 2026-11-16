$app = "C:\Users\User\Documents\win-tweaker\dist\Tweaker.exe"
$artDir = "C:\Users\User\.gemini\antigravity\brain\6094a221-bf4d-4b38-be73-a0487ddbba7b"

$shots = @(
    @{ cat = "overview"; out = "$artDir\ui-overview-audit.png" },
    @{ cat = "all"; out = "$artDir\ui-all-audit.png" },
    @{ cat = "system"; out = "$artDir\ui-system-audit.png" },
    @{ cat = "security"; out = "$artDir\ui-security-audit.png" }
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
