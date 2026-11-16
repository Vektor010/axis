$lines = Get-Content 'C:\Users\User\Documents\оптимизация\scripts\System-Tweaks.ps1' -Encoding UTF8
$lines | Where-Object { $_ -match '^#.*?:' } | Select-Object -First 30 | ForEach-Object { Write-Host $_ }
