# tools\add-groups.ps1
# Добавляет поле "group" сразу после "category" текстовой вставкой, сохраняя форматирование и CRLF.

$jsonPath = "C:\Users\User\Documents\win-tweaker\data\tweaks.json"
$lines = [IO.File]::ReadAllLines($jsonPath, [Text.Encoding]::UTF8)

# Сначала парсим объект, чтобы точно знать id -> group по правилу line_from
$raw = [IO.File]::ReadAllText($jsonPath, [Text.Encoding]::UTF8)
$obj = ConvertFrom-Json $raw

$groupMap = @{}
foreach ($t in $obj) {
    $grp = "Основное"
    if ($t.category -eq "security") {
        $grp = "Основное"
    } elseif ($t.category -eq "visibility") {
        $from = [int]$t.source.line_from
        if ($from -le 277) {
            $grp = "Основное"
        } elseif ($from -le 302) {
            $grp = "Панель навигации"
        } else {
            $grp = "Поиск"
        }
    }
    $groupMap[$t.id] = $grp
}

$newLines = New-Object System.Collections.Generic.List[string]
$currentId = $null

for ($i = 0; $i -lt $lines.Length; $i++) {
    $line = $lines[$i]
    $newLines.Add($line)
    
    if ($line -match '"id":\s*"([^"]+)"') {
        $currentId = $matches[1]
    }
    
    if ($line -match '^(\s*)"category":\s*"([^"]+)",') {
        $indent = $matches[1]
        $grp = $groupMap[$currentId]
        if (-not $grp) { $grp = "Основное" }
        $groupLine = "$indent`"group`":  `"$grp`","
        $newLines.Add($groupLine)
    }
}

# Записываем с точным CRLF и UTF-8 без BOM, сохраняя отсутствие перевода строки в конце файла
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$newContent = [string]::Join("`r`n", $newLines)
[IO.File]::WriteAllText($jsonPath, $newContent, $utf8NoBom)
Write-Output "Successfully updated $jsonPath with $($groupMap.Count) groups."
