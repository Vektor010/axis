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
$files = Get-ChildItem -Path $srcDir -Recurse -Filter "*.md"

$results = @()

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

            if (-not $existingKeys.Contains($valName)) {
                $results += [PSCustomObject]@{
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

Write-Host "Unused policies total: $($results.Count)"
$results | Out-String -Width 160 | Write-Host
