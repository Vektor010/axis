$rootDir = Split-Path -Parent $PSScriptRoot
Set-Location $rootDir

Write-Host "--- 1. Содержимое %ProgramData%\Tweaker ДО запуска тестов и приложения ---"
$before = Get-ChildItem "$env:ProgramData\Tweaker" -Recurse -ErrorAction SilentlyContinue
if ($before) {
    $before | Select-Object FullName, Length, LastWriteTime | Format-Table -AutoSize
} else {
    Write-Host "Каталог $env:ProgramData\Tweaker отсутствует или пуст."
}

Write-Host "`n--- 2. Запуск dist\Tests.exe ---"
$pTests = Start-Process -FilePath "dist\Tests.exe" -NoNewWindow -Wait -PassThru
Write-Host "Tests.exe ExitCode: $($pTests.ExitCode)"

Write-Host "`n--- 3. Запуск dist\Tweaker.exe в тестовом режиме (применение 1 твика) ---"
$pTweaker = Start-Process -FilePath "dist\Tweaker.exe" -ArgumentList @("--apply-tweak", "security-dynamic-lock", "--tab", "journal", "--screenshot", "docs\prototype\screenshot-journal.png") -NoNewWindow -Wait -PassThru
Write-Host "Tweaker.exe ExitCode: $($pTweaker.ExitCode)"

Write-Host "`n--- 4. Содержимое %ProgramData%\Tweaker ПОСЛЕ запуска ---"
$after = Get-ChildItem "$env:ProgramData\Tweaker" -Recurse -ErrorAction SilentlyContinue
if ($after) {
    $after | Select-Object FullName, Length, LastWriteTime | Format-Table -AutoSize
} else {
    Write-Host "Каталог $env:ProgramData\Tweaker по-прежнему отсутствует или пуст (изоляция 100%)."
}

Write-Host "`n--- 5. Пути расположения файлов тестового режима ---"
$tempTweaker = Join-Path $env:TEMP "Tweaker"
Write-Host "state.json (тестовый):   $(Join-Path $tempTweaker 'state-test.json')"
Write-Host "journal.log (тестовый):  $(Join-Path $tempTweaker 'journal-test.log')"
Write-Host "backups (тестовый):      $(Join-Path $tempTweaker 'backups')"

if (Test-Path $tempTweaker) {
    Write-Host "`nФактические файлы в $($tempTweaker):"
    Get-ChildItem $tempTweaker -Recurse | Select-Object FullName, Length | Format-Table -AutoSize
}
