$tweaks = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json
$existingKeys = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)

foreach ($t in $tweaks) {
    if ($t.registry) {
        foreach ($r in $t.registry) {
            $existingKeys.Add($r.name) | Out-Null
        }
    }
}

$srcDir = "C:\Users\User\Documents\win-tweaker\data\source\win-config"
$files = @("security/desc.md", "privacy/desc.md", "system/desc.md", "visibility/desc.md")

$results = @()

foreach ($rel in $files) {
    $full = Join-Path $srcDir $rel
    if (Test-Path $full) {
        $lines = Get-Content $full
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i]
            if ($line -match '^\|\s*\[([^\]]+)\]\([^\)]+\)\s*\|\s*`([^`]+)`\s*\|\s*`([^`]+)`') {
                $policyTitle = $matches[1]
                $keyPath = $matches[2]
                $valName = $matches[3]
                $lineNum = $i + 1

                if (-not $existingKeys.Contains($valName)) {
                    $results += [PSCustomObject]@{
                        File = $rel
                        Line = $lineNum
                        Policy = $policyTitle
                        Path = $keyPath
                        Name = $valName
                    }
                }
            }
        }
    }
}

Write-Host "Remaining unused policies total: $($results.Count)"
$results | Format-Table File, Line, Name, Path -AutoSize
