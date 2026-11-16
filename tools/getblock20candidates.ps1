$tweaks = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json
$existingKeys = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)

foreach ($t in $tweaks) {
    if ($t.registry) {
        foreach ($r in $t.registry) {
            $key = "$($r.hive):$($r.path):$($r.name)".ToLowerInvariant()
            $existingKeys.Add($key) | Out-Null
            $existingKeys.Add($r.name) | Out-Null
        }
    }
}

$srcDir = "C:\Users\User\Documents\win-tweaker\data\source\win-config"
$files = Get-ChildItem -Path $srcDir -Recurse -Filter "*.md"

$candidates = @()

foreach ($f in $files) {
    $lines = Get-Content $f.FullName
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -match '^\|\s*\[([^\]]+)\]\([^\)]+\)\s*\|\s*`([^`]+)`\s*\|\s*`([^`]+)`') {
            $policyTitle = $matches[1]
            $keyPath = $matches[2]
            $valName = $matches[3]
            $lineNum = $i + 1
            $relPath = $f.FullName.Substring($srcDir.Length + 1).Replace("\", "/")

            $fullKey = "hklm:$($keyPath):$($valName)".ToLowerInvariant()
            if (-not $existingKeys.Contains($valName) -and -not $existingKeys.Contains($fullKey)) {
                $candidates += [PSCustomObject]@{
                    File = $relPath
                    Line = $lineNum
                    Policy = $policyTitle
                    Path = $keyPath
                    Name = $valName
                }
            }
        }
    }
}

Write-Host "Remaining filtered candidates count: $($candidates.Count)"
$candidates | Format-Table File, Line, Name, Path, Policy -AutoSize
