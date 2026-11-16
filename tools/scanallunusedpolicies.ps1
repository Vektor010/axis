$tw = Get-Content 'C:\Users\User\Documents\win-tweaker\data\tweaks.json' -Raw | ConvertFrom-Json
$existingRegKeys = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
$existingNames = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)

foreach ($t in $tw) {
    if ($t.registry) {
        foreach ($r in $t.registry) {
            $existingRegKeys.Add("$($r.hive)\$($r.path)\$($r.name)") | Out-Null
            $existingNames.Add($r.name) | Out-Null
        }
    }
}

$srcDir = "C:\Users\User\Documents\win-tweaker\data\source\win-config"
$files = Get-ChildItem -Path $srcDir -Recurse -Filter "*.md"

$policyResults = @()

foreach ($f in $files) {
    $lines = Get-Content $f.FullName
    $relPath = $f.FullName.Substring($srcDir.Length + 1).Replace("\", "/")
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        # Match markdown tables
        if ($line -match '^\|\s*\[([^\]]+)\]\([^\)]+\)\s*\|\s*`([^`]+)`\s*\|\s*`([^`]+)`') {
            $policyTitle = $matches[1]
            $keyPath = $matches[2]
            $valName = $matches[3]
            $lineNum = $i + 1

            if (-not $existingNames.Contains($valName)) {
                $policyResults += [PSCustomObject]@{
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

Write-Host "Unused policies found across all markdown tables: $($policyResults.Count)"
$policyResults | Format-Table File, Line, Name, Path -AutoSize
