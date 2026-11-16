$tw = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json
$samples = @(
    "system-game-mode-allow-auto",
    "system-game-mode-gpu-yield",
    "security-defender-disable-behavior-monitoring",
    "privacy-copilot-hardware-key",
    "power-remove-sleep-option"
)

$srcBase = "C:\Users\User\Documents\win-tweaker\data\source\win-config"

foreach ($id in $samples) {
    $item = $tw | Where-Object { $_.id -eq $id }
    Write-Host "=========================================================="
    Write-Host "ТВИК: $($item.id)" -ForegroundColor Cyan
    Write-Host "Имя (RU): $($item.name_ru)"
    Write-Host "Файл Noverse: $($item.source.file) (строки $($item.source.line_from)..$($item.source.line_to))"
    
    $fullPath = Join-Path $srcBase $item.source.file
    $lines = Get-Content $fullPath
    Write-Host "--- Цитата из репозитория nohuto/win-config ---" -ForegroundColor Yellow
    for ($l = $item.source.line_from; $l -le $item.source.line_to; $l++) {
        Write-Host "[$l]: $($lines[$l - 1])"
    }
    Write-Host ""
}
