# tools\scan-candidates.ps1
param(
    [string]$Mode = "Records" # "Records" (57 записей по строкам 140-317) или "Candidates" (анализ блоков-кандидатов)
)

$repoRoot = Split-Path -Parent $PSScriptRoot
$vFile = Join-Path $repoRoot "data\source\win-config\visibility\desc.md"
$sFile = Join-Path $repoRoot "data\source\win-config\security\desc.md"
$tweaksJson = Join-Path $repoRoot "data\tweaks.json"

$vlines = Get-Content $vFile
$slines = Get-Content $sFile
$tweaks = Get-Content $tweaksJson -Raw -Encoding UTF8 | ConvertFrom-Json

if ($Mode -eq "Records") {
    # Режим вывода каждой строки с Type: в строках 140-317 visibility
    for ($i = 140; $i -le 317; $i++) {
        $raw = $vlines[$i - 1]
        if ($raw -match 'Type:') {
            $t = $raw.Trim()
            $typeIdx = $t.IndexOf("Type:")
            $pathPart = ($t.Substring(0, $typeIdx)).Trim()
            $dataVal = ""
            if ($t -match 'Data:\s*([0-9a-fA-Fx]+)') { $dataVal = $matches[1] }
            
            # Сопоставление с tweaks.json
            $matchedTweak = $tweaks | Where-Object {
                $_.source.file -eq "visibility/desc.md" -and
                $i -ge [int]$_.source.line_from -and
                $i -le [int]$_.source.line_to
            }
            
            $status = ""
            if ($matchedTweak) {
                $status = $matchedTweak.id
            } else {
                # Исключенные строки
                if ($i -eq 140) {
                    $status = "исключено: блок ShowFrequent 136-140 имеет Enabled (default)"
                } elseif ($i -in @(184, 186)) {
                    $status = "исключено: параметр с вариантами Hidden (Data 2 / Data 1)"
                } elseif ($i -in @(226, 228)) {
                    $status = "исключено: параметр с вариантами ShowDriveLettersFirst (Data 0 / Data 2)"
                } elseif ($i -in @(274, 276)) {
                    $status = "исключено: TypeAhead (под-параметр поиска без бинарного тумблера)"
                } else {
                    $status = "исключено: вне диапазонов включенных твиков"
                }
            }
            
            Write-Output ("{0,4} | {1} | {2} | {3}" -f $i, $pathPart, $dataVal, $status)
        }
    }
} else {
    # Режим анализа блоков-кандидатов (для Compare-Object list-tweaks)
    function Analyze-Candidate($lines, $headerLine, $maxLines, $isMarkdownHeader, $srcFile) {
        $headerText = $lines[$headerLine - 1].Trim()
        $enLine = $null
        $enMarker = "none"
        $enData = $null
        $enType = $null
        $enPath = $null
        $enComments = 0
        
        $disLine = $null
        $disMarker = "none"
        $disData = $null
        $disType = $null
        $disPath = $null
        $disComments = 0
        
        $curBlock = "none"
        $otherComments = 0
        $pairsCount = 0
        $hasSecondComment = $false
        
        $limit = [Math]::Min($lines.Count, $headerLine + $maxLines)
        for ($j = $headerLine + 1; $j -le $limit; $j++) {
            $raw = $lines[$j - 1]
            $t = $raw.Trim()
            
            if (-not $isMarkdownHeader -and $t -match '^\/\/' -and $t -notmatch '^\/\/\s*(Enabled|Disabled)') {
                break
            }
            if ($isMarkdownHeader -and ($t -match '^#\s+' -or $t -match '^##\s+')) {
                break
            }
            if ($t -eq '```' -and $curBlock -eq "disabled" -and $disData -ne $null) {
                break
            }
            
            if ($t -match '^\/\/\s*Enabled(\s*\((.+?)\))?') {
                $curBlock = "enabled"
                $enLine = $j
                if ($matches[2]) { $enMarker = "(" + $matches[2] + ")" }
            }
            elseif ($t -match '^\/\/\s*Disabled(\s*\((.+?)\))?') {
                $curBlock = "disabled"
                $disLine = $j
                if ($matches[2]) { $disMarker = "(" + $matches[2] + ")" }
            }
            elseif ($t -match '^\/\/\s*(.+)') {
                if ($curBlock -eq "enabled") {
                    $enComments++
                    $hasSecondComment = $true
                }
                elseif ($curBlock -eq "disabled") {
                    $disComments++
                    $hasSecondComment = $true
                }
                else {
                    $otherComments++
                }
            }
            elseif ($t -match '^(HKCU|HKLM)\\[^\t ]+') {
                $type = ""
                $data = ""
                if ($t -match 'Type:\s*([A-Za-z0-9_]+)') { $type = $matches[1] }
                if ($t -match 'Data:\s*([0-9]+)') { $data = $matches[1] }
                $typeIdx = $t.IndexOf("Type:")
                $path = if ($typeIdx -gt 0) { $t.Substring(0, $typeIdx).Trim() } else { $t.Trim() }
                
                if ($curBlock -eq "enabled") {
                    if ($enPath) { $pairsCount++ }
                    $enPath = $path
                    $enType = $type
                    $enData = $data
                }
                elseif ($curBlock -eq "disabled") {
                    if ($disPath) { $pairsCount++ }
                    $disPath = $path
                    $disType = $type
                    $disData = $data
                }
            }
        }
        
        $structure = "pair"
        $decision = "include"
        $reason = ""
        
        if (-not $enLine -or -not $disLine -or -not $enPath -or -not $disPath) {
            $structure = "other"
            $decision = "exclude"
            $reason = "отсутствует блок Enabled или Disabled"
        }
        elseif ($hasSecondComment) {
            $structure = "other"
            $decision = "exclude"
            $reason = "вторая строка комментария внутри блока"
        }
        elseif ($pairsCount -gt 0) {
            $structure = "other"
            $decision = "exclude"
            $reason = "больше одной пары путь+имя"
        }
        elseif ($enType -ne "REG_DWORD" -or $disType -ne "REG_DWORD") {
            $structure = "other"
            $decision = "exclude"
            $reason = "не DWORD"
        }
        elseif ($enData -ne "0" -and $enData -ne "1") {
            $structure = "other"
            $decision = "exclude"
            $reason = "параметр с вариантами (Data Enabled не 0 и не 1)"
        }
        elseif ($disData -ne "0" -and $disData -ne "1") {
            $structure = "other"
            $decision = "exclude"
            $reason = "параметр с вариантами (Data Disabled не 0 и не 1)"
        }
        elseif ($enMarker -eq "(default)") {
            $decision = "exclude"
            $reason = "(default) на Enabled"
        }
        else {
            $decision = "include"
            if ($disMarker -eq "(default)") {
                $reason = "revert.mode=set (value=$disData)"
            } else {
                $reason = "revert.mode=snapshot"
            }
        }
        
        [PSCustomObject]@{
            HeaderLine = $headerLine
            HeaderText = $headerText
            EnabledLine = $(if ($enLine) { $enLine } else { "-" })
            EnabledMarker = $enMarker
            DataEnabled = $(if ($enData) { $enData } else { "-" })
            DisabledLine = $(if ($disLine) { $disLine } else { "-" })
            DisabledMarker = $disMarker
            DataDisabled = $(if ($disData) { $disData } else { "-" })
            Structure = $structure
            Decision = $decision
            Reason = $reason
            SourceFile = $srcFile
        }
    }

    $results = @()
    # Security: 1250
    $results += Analyze-Candidate $slines 1250 25 $true "security/desc.md"
    
    # Visibility: lines 140 to 314
    $headersV = @(142, 152, 158, 164, 170, 176, 182, 188, 194, 200, 206, 212, 218, 224, 230, 236, 242, 248, 254, 260, 266, 272, 278, 285, 291, 297, 307, 313)
    foreach ($h in $headersV) {
        $results += Analyze-Candidate $vlines $h 20 $false "visibility/desc.md"
    }

    foreach ($r in $results) {
        Write-Output ("{0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8} | {9} | {10}" -f $r.HeaderLine, $r.HeaderText, $r.EnabledLine, $r.EnabledMarker, $r.DataEnabled, $r.DisabledLine, $r.DisabledMarker, $r.DataDisabled, $r.Structure, $r.Decision, $r.Reason)
    }
}
