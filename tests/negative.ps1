param(
    [string]$TestsExe = "dist\Tests.exe",
    [string]$SourceTweaks = "data\tweaks.json"
)

$ErrorActionPreference = "Stop"

Write-Host "=================================================================="
Write-Host "    ОДИННАДЦАТЬ НЕГАТИВНЫХ СЦЕНАРИЕВ С DIFF И ТОЧНЫМ РАВЕНСТВОМ   "
Write-Host "=================================================================="

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
Set-Location $rootDir

if (-not [System.IO.Path]::IsPathRooted($TestsExe)) {
    $TestsExe = Join-Path $rootDir $TestsExe
}
if (-not [System.IO.Path]::IsPathRooted($SourceTweaks)) {
    $SourceTweaks = Join-Path $rootDir $SourceTweaks
}

if (-not (Test-Path $TestsExe)) {
    Write-Error "Исполняемый файл тестов не найден: $TestsExe"
    exit 1
}

# 1. Фиксация SHA-256 ДО тестов
$shaBefore = (Get-FileHash -Path $SourceTweaks -Algorithm SHA256).Hash
Write-Host ("SHA-256 tweaks.json ДО начала: {0}" -f $shaBefore)

$rawJson = [System.IO.File]::ReadAllText($SourceTweaks, [System.Text.Encoding]::UTF8)
$tempDir = [System.IO.Path]::GetTempPath()
$allPassed = $true

function Run-Scenario {
    param(
        [int]$Number,
        [string]$Title,
        [string[]]$ExpectedFails,
        [string]$BeforeSnippet,
        [string]$AfterSnippet,
        [string]$MutatedJson
    )

    Write-Host ""
    Write-Host "------------------------------------------------------------------"
    Write-Host (">>> [СЦЕНАРИЙ {0}] {1}" -f $Number, $Title)
    Write-Host "------------------------------------------------------------------"
    Write-Host "ОЖИДАЕМЫЕ ПАДЕНИЯ ТЕСТОВ (ТОЧНЫЙ СПИСОК):" -ForegroundColor Magenta
    foreach ($ef in $ExpectedFails) {
        Write-Host ("   * {0}" -f $ef) -ForegroundColor Magenta
    }
    Write-Host "--- DIFF МУТАЦИИ ДО: ---" -ForegroundColor Yellow
    Write-Host $BeforeSnippet
    Write-Host "+++ DIFF МУТАЦИИ ПОСЛЕ: +++" -ForegroundColor Cyan
    Write-Host $AfterSnippet

    $tempFile = Join-Path $tempDir ("tweaks_neg_{0}.json" -f $Number)
    $outFile = Join-Path $tempDir ("tweaks_neg_{0}_out.txt" -f $Number)
    [System.IO.File]::WriteAllText($tempFile, $MutatedJson, [System.Text.Encoding]::UTF8)

    $proc = Start-Process -FilePath $TestsExe -ArgumentList @("--tweaks", $tempFile) -RedirectStandardOutput $outFile -NoNewWindow -Wait -PassThru
    $code = $proc.ExitCode
    $output = [System.IO.File]::ReadAllText($outFile, [System.Text.Encoding]::UTF8)

    Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
    Remove-Item $outFile -Force -ErrorAction SilentlyContinue

    Write-Host ("Код выхода процесса: {0}" -f $code)

    $failLines = @()
    foreach ($line in ($output -split "`r?`n")) {
        if ($line -match "^\[FAIL\]\s*(.*)$") {
            $f = $matches[1].Trim()
            if ($f -notmatch "^Обнаружены проваленные тесты") {
                $arrowIdx = $f.IndexOf(" -> ")
                if ($arrowIdx -ge 0) {
                    $f = $f.Substring(0, $arrowIdx).Trim()
                }
                $failLines += $f
            }
        }
    }

    Write-Host "ФАКТИЧЕСКИЕ ПАДЕНИЯ ТЕСТОВ ([FAIL]):"
    foreach ($fl in $failLines) {
        Write-Host ("   [FAIL] {0}" -f $fl) -ForegroundColor Red
    }

    # Сверка по ТОЧНОМУ равенству множеств (Compare-Object)
    $expectedSorted = @($ExpectedFails | Sort-Object -Unique)
    $actualSorted = @($failLines | Sort-Object -Unique)

    $diff = Compare-Object -ReferenceObject $expectedSorted -DifferenceObject $actualSorted
    $isEqual = ($null -eq $diff -or $diff.Count -eq 0)

    if ($code -ne 0 -and $isEqual) {
        Write-Host ("   [ВЕРДИКТ СЦЕНАРИЯ {0}]: СОВПАЛО (точное равенство множеств, код {1})" -f $Number, $code) -ForegroundColor Green
    } else {
        $script:allPassed = $false
        if ($code -eq 0) {
            Write-Host ("   [ОШИБКА]: Процесс вернул код 0 (ожидался сбой)!" -f $Number) -ForegroundColor Red
        }
        if (-not $isEqual) {
            Write-Host "   [ОШИБКА СВЕРКИ]: Множества сбоев не совпали!" -ForegroundColor Red
            foreach ($d in $diff) {
                if ($d.SideIndicator -eq "<=") {
                    Write-Host ("      Отсутствует ожидаемый сбой: {0}" -f $d.InputObject) -ForegroundColor Red
                } elseif ($d.SideIndicator -eq "=>") {
                    Write-Host ("      Лишний непредвиденный сбой: {0}" -f $d.InputObject) -ForegroundColor Red
                }
            }
        }
    }
}

# -------------------------------------------------------------------------
# СЦЕНАРИЙ 1: Полярность перепутана (apply = 0, revert.value = 1) у security-dynamic-lock
# -------------------------------------------------------------------------
$before1 = @'
          "apply":  1,
          "apply_block":  "Enabled",
          "revert":  {
                         "mode":  "set",
                         "value":  0
                     }
'@
$after1 = @'
          "apply":  0,
          "apply_block":  "Enabled",
          "revert":  {
                         "mode":  "set",
                         "value":  1
                     }
'@
$mutated1 = $rawJson -replace '("id":\s*"security-dynamic-lock"[\s\S]*?"apply":\s*)1', '${1}0' -replace '("id":\s*"security-dynamic-lock"[\s\S]*?"value":\s*)0', '${1}1'
Run-Scenario -Number 1 `
    -Title "Полярность перепутана: apply=0, revert.value=1 у security-dynamic-lock" `
    -ExpectedFails @(
        "security-dynamic-lock[EnableGoodbye]: Значение apply совпадает с Data под-блока",
        "security-dynamic-lock[EnableGoodbye]: Под-блок revert ОБЯЗАТЕЛЬНО содержит маркер (default)"
    ) `
    -BeforeSnippet $before1 `
    -AfterSnippet $after1 `
    -MutatedJson $mutated1

# -------------------------------------------------------------------------
# СЦЕНАРИЙ 2: Твик без (default) (HideFileExt): mode=set со значением 0
# -------------------------------------------------------------------------
$before2 = @'
          "apply":  1,
          "apply_block":  "Enabled",
          "revert":  {
                         "mode":  "snapshot"
                     }
'@
$after2 = @'
          "apply":  1,
          "apply_block":  "Enabled",
          "revert":  {
                         "mode":  "set",
                         "value":  0
                     }
'@
$mutated2 = $rawJson -replace '("id":\s*"visibility-hide-file-ext"[\s\S]*?"mode":\s*)"snapshot"', '${1}"set", "value": 0'
Run-Scenario -Number 2 `
    -Title "Твик без (default) (HideFileExt): mode=set, value=0 -> падает только тест маркера revert" `
    -ExpectedFails @(
        "visibility-hide-file-ext[HideFileExt]: Под-блок revert ОБЯЗАТЕЛЬНО содержит маркер (default)"
    ) `
    -BeforeSnippet $before2 `
    -AfterSnippet $after2 `
    -MutatedJson $mutated2

# -------------------------------------------------------------------------
# СЦЕНАРИЙ 3: mode=set, revert.value = apply (1 == 1) у security-dynamic-lock
# -------------------------------------------------------------------------
$before3 = @'
          "mode":  "set",
          "value":  0
'@
$after3 = @'
          "mode":  "set",
          "value":  1
'@
$mutated3 = $rawJson -replace '("id":\s*"security-dynamic-lock"[\s\S]*?"value":\s*)0', '${1}1'
Run-Scenario -Number 3 `
    -Title "mode=set, revert.value = apply (1 == 1) -> падает apply != revert.value и маркер revert" `
    -ExpectedFails @(
        "security-dynamic-lock[EnableGoodbye]: apply != revert.value",
        "security-dynamic-lock[EnableGoodbye]: Под-блок revert ОБЯЗАТЕЛЬНО содержит маркер (default)"
    ) `
    -BeforeSnippet $before3 `
    -AfterSnippet $after3 `
    -MutatedJson $mutated3

# -------------------------------------------------------------------------
# СЦЕНАРИЙ 4: Диапазон строк сдвинут на +300 (visibility-hide-file-ext)
# -------------------------------------------------------------------------
$before4 = @'
      "line_from":  194,
      "line_to":  198
'@
$after4 = @'
      "line_from":  494,
      "line_to":  498
'@
$mutated4 = $rawJson -replace '("id":\s*"visibility-hide-file-ext"[\s\S]*?"line_from":\s*)194', '${1}494' -replace '("id":\s*"visibility-hide-file-ext"[\s\S]*?"line_to":\s*)198', '${1}498'
Run-Scenario -Number 4 `
    -Title "Диапазон строк сдвинут на +300 -> падение тестов поиска и парсинга в диапазоне" `
    -ExpectedFails @(
        "visibility-hide-file-ext: В диапазоне ровно одна пара (путь, имя)",
        "visibility-hide-file-ext[HideFileExt]: Наличие пути и имени в диапазоне",
        "visibility-hide-file-ext[HideFileExt]: Наличие пути ключа в источнике",
        "visibility-hide-file-ext[HideFileExt]: Наличие имени параметра в источнике",
        "visibility-hide-file-ext[HideFileExt]: Найден под-блок apply 'Enabled'",
        "visibility-hide-file-ext[HideFileExt]: В под-блоке 'Enabled' найдена запись по пути и имени",
        "visibility-hide-file-ext[HideFileExt]: Значение apply совпадает с Data под-блока",
        "visibility-hide-file-ext[HideFileExt]: Блок Disabled существует в источнике",
        "visibility-hide-file-ext[HideFileExt]: В блоке Disabled найдена запись по пути и имени"
    ) `
    -BeforeSnippet $before4 `
    -AfterSnippet $after4 `
    -MutatedJson $mutated4

# -------------------------------------------------------------------------
# СЦЕНАРИЙ 5: Путь ключа искажен (Advanced -> AdvancedBAD) у visibility-hide-file-ext
# -------------------------------------------------------------------------
$before5 = @'
          "path":  "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced",
'@
$after5 = @'
          "path":  "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\AdvancedBAD",
'@
$mutated5 = $rawJson -replace '("id":\s*"visibility-hide-file-ext"[\s\S]*?"path":\s*"Software\\\\Microsoft\\\\Windows\\\\CurrentVersion\\\\Explorer\\\\)Advanced"', '${1}AdvancedBAD"'
Run-Scenario -Number 5 `
    -Title "Путь ключа искажен (AdvancedBAD) -> путь не найден в источнике" `
    -ExpectedFails @(
        "visibility-hide-file-ext[HideFileExt]: Наличие пути и имени в диапазоне",
        "visibility-hide-file-ext[HideFileExt]: Наличие пути ключа в источнике",
        "visibility-hide-file-ext[HideFileExt]: В под-блоке 'Enabled' найдена запись по пути и имени",
        "visibility-hide-file-ext[HideFileExt]: Значение apply совпадает с Data под-блока",
        "visibility-hide-file-ext[HideFileExt]: В блоке Disabled найдена запись по пути и имени"
    ) `
    -BeforeSnippet $before5 `
    -AfterSnippet $after5 `
    -MutatedJson $mutated5

# -------------------------------------------------------------------------
# СЦЕНАРИЙ 6: revert.mode=snapshot, но revert.value задан (value: 0) у visibility-hide-file-ext
# -------------------------------------------------------------------------
$before6 = @'
          "revert":  {
                         "mode":  "snapshot"
                     }
'@
$after6 = @'
          "revert":  {
                         "mode":  "snapshot",
                         "value":  0
                     }
'@
$mutated6 = $rawJson -replace '("id":\s*"visibility-hide-file-ext"[\s\S]*?"mode":\s*"snapshot"\s*)\}', '${1}, "value": 0 }'
Run-Scenario -Number 6 `
    -Title "revert.mode=snapshot, но revert.value задан -> падает проверка отсутствия value" `
    -ExpectedFails @(
        "visibility-hide-file-ext[HideFileExt]: При mode=snapshot revert.value отсутствует (null)"
    ) `
    -BeforeSnippet $before6 `
    -AfterSnippet $after6 `
    -MutatedJson $mutated6

# -------------------------------------------------------------------------
# СЦЕНАРИЙ 7: Имя значения изменено (HideFileExtX) у visibility-hide-file-ext
# -------------------------------------------------------------------------
$before7 = @'
          "name":  "HideFileExt",
'@
$after7 = @'
          "name":  "HideFileExtX",
'@
$mutated7 = $rawJson -replace '("id":\s*"visibility-hide-file-ext"[\s\S]*?"name":\s*)"HideFileExt"', '${1}"HideFileExtX"'
Run-Scenario -Number 7 `
    -Title "Имя значения изменено (HideFileExtX) -> падение тестов поиска имени и записей" `
    -ExpectedFails @(
        "visibility-hide-file-ext[HideFileExtX]: Наличие пути и имени в диапазоне",
        "visibility-hide-file-ext[HideFileExtX]: Наличие имени параметра в источнике",
        "visibility-hide-file-ext[HideFileExtX]: В под-блоке 'Enabled' найдена запись по пути и имени",
        "visibility-hide-file-ext[HideFileExtX]: Значение apply совпадает с Data под-блока",
        "visibility-hide-file-ext[HideFileExtX]: В блоке Disabled найдена запись по пути и имени"
    ) `
    -BeforeSnippet $before7 `
    -AfterSnippet $after7 `
    -MutatedJson $mutated7

# -------------------------------------------------------------------------
# СЦЕНАРИЙ 8: apply_block = 'Disabled' при apply = 1 у HideFileExt
# -------------------------------------------------------------------------
$before8 = @'
          "apply":  1,
          "apply_block":  "Enabled",
'@
$after8 = @'
          "apply":  1,
          "apply_block":  "Disabled",
'@
$mutated8 = $rawJson -replace '("id":\s*"visibility-hide-file-ext"[\s\S]*?"apply_block":\s*)"Enabled"', '${1}"Disabled"'
Run-Scenario -Number 8 `
    -Title "apply_block = 'Disabled' при apply = 1 у HideFileExt -> падает тест совпадения apply с Data и полярности" `
    -ExpectedFails @(
        "visibility-hide-file-ext[HideFileExt]: Значение apply совпадает с Data под-блока",
        "visibility-hide-file-ext[HideFileExt]: Data блока Disabled (0) отличается от Data блока Enabled (0)"
    ) `
    -BeforeSnippet $before8 `
    -AfterSnippet $after8 `
    -MutatedJson $mutated8

# -------------------------------------------------------------------------
# СЦЕНАРИЙ 9: Диапазон сдвинут на +6 строк (line_from=200, line_to=204) у HideFileExt
# -------------------------------------------------------------------------
$before9 = @'
      "line_from":  194,
      "line_to":  198
'@
$after9 = @'
      "line_from":  200,
      "line_to":  204
'@
$mutated9 = $rawJson -replace '("id":\s*"visibility-hide-file-ext"[\s\S]*?"line_from":\s*)194', '${1}200' -replace '("id":\s*"visibility-hide-file-ext"[\s\S]*?"line_to":\s*)198', '${1}204'
Run-Scenario -Number 9 `
    -Title "Диапазон сдвинут на +6 строк (200..204) -> чужой блок, падает поиск пути/имени" `
    -ExpectedFails @(
        "visibility-hide-file-ext[HideFileExt]: Наличие пути и имени в диапазоне",
        "visibility-hide-file-ext[HideFileExt]: Наличие имени параметра в источнике",
        "visibility-hide-file-ext[HideFileExt]: В под-блоке 'Enabled' найдена запись по пути и имени",
        "visibility-hide-file-ext[HideFileExt]: Значение apply совпадает с Data под-блока",
        "visibility-hide-file-ext[HideFileExt]: В блоке Disabled найдена запись по пути и имени"
    ) `
    -BeforeSnippet $before9 `
    -AfterSnippet $after9 `
    -MutatedJson $mutated9

# -------------------------------------------------------------------------
# СЦЕНАРИЙ 10: Добавлена копия твика с name=SnapAssist, line 784–788 (случай 3.2)
# -------------------------------------------------------------------------
$before10 = "Отсутствует твик visibility-snap-assist (строки 784..788)"
$after10 = @'
    {
        "id":  "visibility-snap-assist",
        "name_ru":  "Помощник привязки окон",
        "description_ru":  "",
        "category":  "visibility",
        "type":  "toggle",
        "risk":  "low",
        "presets":  [],
        "registry":  [
                         {
                             "hive":  "HKCU",
                             "path":  "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced",
                             "name":  "SnapAssist",
                             "type":  "REG_DWORD",
                             "apply":  1,
                             "apply_block":  "Enabled",
                             "revert":  { "mode": "snapshot" }
                         }
                     ],
        "source":  {
                       "file":  "visibility/desc.md",
                       "line_from":  784,
                       "line_to":  788
                   },
        "min_build":  null,
        "needs_review":  true
    },
'@
$snapItemJson = @'
    {
        "id":  "visibility-snap-assist",
        "name_ru":  "Помощник привязки окон",
        "description_ru":  "",
        "category":  "visibility",
        "type":  "toggle",
        "risk":  "low",
        "presets":  [],
        "registry":  [
                         {
                             "hive":  "HKCU",
                             "path":  "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced",
                             "name":  "SnapAssist",
                             "type":  "REG_DWORD",
                             "apply":  1,
                             "apply_block":  "Enabled",
                             "revert":  { "mode": "snapshot" }
                         }
                     ],
        "source":  {
                       "file":  "visibility/desc.md",
                       "line_from":  784,
                       "line_to":  788
                   },
        "min_build":  null,
        "needs_review":  true
    },
'@
$mutated10 = $rawJson.Insert($rawJson.IndexOf("[") + 1, "`r`n" + $snapItemJson)

Run-Scenario -Number 10 `
    -Title "Добавлена копия твика с name=SnapAssist, line 784..788 -> в диапазоне нет ключа" `
    -ExpectedFails @(
        "visibility-snap-assist: В диапазоне ровно одна пара (путь, имя)",
        "visibility-snap-assist[SnapAssist]: Наличие пути и имени в диапазоне",
        "visibility-snap-assist[SnapAssist]: Наличие пути ключа в источнике",
        "visibility-snap-assist[SnapAssist]: Наличие имени параметра в источнике",
        "visibility-snap-assist[SnapAssist]: Найден под-блок apply 'Enabled'",
        "visibility-snap-assist[SnapAssist]: В под-блоке 'Enabled' найдена запись по пути и имени",
        "visibility-snap-assist[SnapAssist]: Значение apply совпадает с Data под-блока",
        "visibility-snap-assist[SnapAssist]: Блок Disabled существует в источнике",
        "visibility-snap-assist[SnapAssist]: В блоке Disabled найдена запись по пути и имени"
    ) `
    -BeforeSnippet $before10 `
    -AfterSnippet $after10 `
    -MutatedJson $mutated10

# -------------------------------------------------------------------------
# СЦЕНАРИЙ 11: Диапазон line_to увеличен на +6 строк (line_to=204) у HideFileExt
# -------------------------------------------------------------------------
$before11 = @'
      "line_from":  194,
      "line_to":  198
'@
$after11 = @'
      "line_from":  194,
      "line_to":  204
'@
$mutated11 = $rawJson -replace '("id":\s*"visibility-hide-file-ext"[\s\S]*?"line_to":\s*)198', '${1}204'

Run-Scenario -Number 11 `
    -Title "Диапазон line_to увеличен на +6 строк (194..204) -> захват второго твика (пар=2)" `
    -ExpectedFails @(
        "visibility-hide-file-ext: В диапазоне ровно одна пара (путь, имя)"
    ) `
    -BeforeSnippet $before11 `
    -AfterSnippet $after11 `
    -MutatedJson $mutated11

Write-Host ""
Write-Host "=================================================================="

# 6. Проверка целостности data\tweaks.json и git status
$shaAfter = (Get-FileHash -Path $SourceTweaks -Algorithm SHA256).Hash
Write-Host ("SHA-256 tweaks.json ДО:    {0}" -f $shaBefore)
Write-Host ("SHA-256 tweaks.json ПОСЛЕ: {0}" -f $shaAfter)

$shaMatches = ($shaBefore -eq $shaAfter)
$shaMatchText = $(if ($shaMatches) { "ДА (совпадает)" } else { "НЕТ (РАСХОЖДЕНИЕ!)" })
Write-Host ("SHA-256 совпадает: {0}" -f $shaMatchText)

$gitStatus = git status --short $SourceTweaks
$gitStatusText = $(if ([string]::IsNullOrWhiteSpace($gitStatus)) { "ЧИСТО (файл не изменен относительно индекса/коммита)" } else { "ИЗМЕНЕН: $gitStatus" })
Write-Host ("git status data\tweaks.json: {0}" -f $gitStatusText)

if ($allPassed -and $shaMatches) {
    Write-Host "ВСЕ 11 НЕГАТИВНЫХ СЦЕНАРИЕВ УСПЕШНО ПРОЙДЕНЫ (ТОЧНОЕ РАВЕНСТВО МНОЖЕСТВ СБОЕВ = СОВПАЛО)!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "ОШИБКА: ПРОВАЛ ОДНОГО ИЛИ НЕСКОЛЬКИХ СЦЕНАРИЕВ ЛИБО РАСХОЖДЕНИЕ МНОЖЕСТВ!" -ForegroundColor Red
    exit 1
}
