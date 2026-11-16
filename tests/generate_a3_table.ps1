$rootDir = Split-Path -Parent $PSScriptRoot
Set-Location $rootDir
$jsonPath = Join-Path $rootDir "data\tweaks.json"
$json = [System.IO.File]::ReadAllText($jsonPath, [System.Text.Encoding]::UTF8)
$tweaks = ConvertFrom-Json $json

$outRows = @()

foreach ($t in $tweaks) {
    $id = $t.id
    $file = $t.source.file
    $from = $t.source.line_from
    $to = $t.source.line_to

    $sourcePath = Join-Path "data\source\win-config" ($file -replace '/', '\')
    $rawLines = Get-Content $sourcePath | Select-Object -Skip ($from - 1) -First ($to - $from + 1) | ForEach-Object -Begin {$n=$from} -Process {"{0}: {1}" -f $n++, $_}
    $rawBlock = $rawLines -join "`n"

    # Default location
    $defaultLoc = "нет"
    for ($i = 0; $i -lt $rawLines.Count; $i++) {
        if ($rawLines[$i] -match '\(default\)') {
            $lineNum = $rawLines[$i].Split(':')[0]
            $defaultLoc = "$file`:$lineNum"
        }
    }

    # Query registry (read only)
    $reg = $t.registry[0]
    $regPath = "$($reg.hive)\$($reg.path)"
    $valName = $reg.name
    $queryOut = ""
    try {
        $p = Start-Process "reg.exe" -ArgumentList @("query", $regPath, "/v", $valName) -RedirectStandardOutput "$env:TEMP\reg_out.txt" -RedirectStandardError "$env:TEMP\reg_err.txt" -NoNewWindow -Wait -PassThru
        if ($p.ExitCode -eq 0) {
            $qLines = Get-Content "$env:TEMP\reg_out.txt" | Where-Object { $_ -match [regex]::Escape($valName) }
            $queryOut = ($qLines -join " ").Trim()
        } else {
            $queryOut = "параметр отсутствует"
        }
    } catch {
        $queryOut = "ключа нет"
    }

    $decision = if ($t.registry[0].revert.mode -eq "set") {
        "Включен: apply=$($reg.apply), revert=set ($($reg.revert.value))"
    } else {
        "Включен: apply=$($reg.apply), revert=snapshot"
    }

    $outRows += [PSCustomObject]@{
        "id" = $id
        "rawBlock" = $rawBlock
        "defaultLoc" = $defaultLoc
        "queryOut" = $queryOut
        "decision" = $decision
    }
}

$outRows | ConvertTo-Json -Depth 4 | Out-File -FilePath "$env:TEMP\a3_table.json" -Encoding UTF8
Write-Host "Table generated for $($outRows.Count) tweaks."
