$proc = Start-Process -FilePath "C:\Users\User\Documents\win-tweaker\tests\baseline\bin\BaselineWindow.exe" -PassThru -Wait
Write-Host "ExitCode: $($proc.ExitCode)"
if (Test-Path "$env:TEMP\baseline_first_render.txt") {
    Get-Content "$env:TEMP\baseline_first_render.txt"
} else {
    Write-Host "File not created"
}
