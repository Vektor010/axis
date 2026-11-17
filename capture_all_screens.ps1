$exe = 'C:\Users\User\Documents\win-tweaker\dist\Axis.exe'
$art = 'C:\Users\User\.gemini\antigravity\brain\6094a221-bf4d-4b38-be73-a0487ddbba7b'

Write-Host "Capturing Settings..."
Start-Process -FilePath $exe -ArgumentList '--mock', '--screenshot', "$art\axis_2026_settings_v3.png", '--tab', 'settings' -Wait

Write-Host "Capturing Tweaks (Expanded)..."
Start-Process -FilePath $exe -ArgumentList '--mock', '--screenshot', "$art\axis_2026_tweaks_expanded_v3.png", '--tab', 'tweaks', '--expand-card' -Wait

Write-Host "Capturing Overview..."
Start-Process -FilePath $exe -ArgumentList '--mock', '--screenshot', "$art\axis_2026_overview_v3.png", '--tab', 'overview', '--history-60s' -Wait

Write-Host "Capturing About..."
Start-Process -FilePath $exe -ArgumentList '--mock', '--screenshot', "$art\axis_2026_about_v3.png", '--tab', 'about' -Wait

Write-Host "Capturing History..."
Start-Process -FilePath $exe -ArgumentList '--mock', '--screenshot', "$art\axis_2026_history_v3.png", '--tab', 'history' -Wait

Write-Host "Capturing Journal..."
Start-Process -FilePath $exe -ArgumentList '--mock', '--screenshot', "$art\axis_2026_journal_v3.png", '--tab', 'journal' -Wait

Write-Host "Done. Checking files:"
Get-ChildItem -Path $art -Filter 'axis_2026_*_v3.png' | Select-Object Name, Length
