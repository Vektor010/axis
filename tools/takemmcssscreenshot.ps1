$app = "C:\Users\User\Documents\win-tweaker\dist\Tweaker.exe"
$artDir = "C:\Users\User\.gemini\antigravity\brain\6094a221-bf4d-4b38-be73-a0487ddbba7b"
$out = Join-Path $artDir "ui-overview-mmcss.png"

if (Test-Path $out) {
    Remove-Item -Path $out -Recurse -Force
}

Write-Host "Capturing overview MMCSS..."
$p = Start-Process -FilePath $app -ArgumentList @("--tab", "overview", "--screenshot", $out) -PassThru -Wait

if (Test-Path $out) {
    $size = (Get-Item $out).Length
    Write-Host "Saved $out ($size bytes)"
} else {
    Write-Host "FAILED to capture $out"
}
