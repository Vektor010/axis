Set-Location C:\Users\User\Documents\win-tweaker

$files = Get-ChildItem -Path "data\source\win-config\*\desc.md"

$entries = @()

foreach ($file in $files) {
    $cat = $file.Directory.Name
    $lines = Get-Content $file.FullName
    $currentH1 = "None"
    
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -match '^#\s+(.+)$') {
            $currentH1 = $matches[1].Trim()
        }
        
        # Match Tab/space separated or quoted Type: REG_
        # Example: HKLM\...\ValName <TAB> Type: REG_DWORD, Length: 4, Data: 1
        # Example: "HKLM\...\ValName","Type: REG_DWORD, Length: 4, Data: 1"
        if ($line -match '^"?((?:HKLM|HKCU|HKEY_LOCAL_MACHINE|HKEY_CURRENT_USER)\\[^",\t\r\n]+)"?[,\t ]+Type:\s*(REG_[^,\r\n]+)(?:,\s*Length:\s*[^,\r\n]+)?(?:,\s*Data:\s*(.*?))?"?\s*$') {
            $fullPath = $matches[1].Trim()
            $regType = $matches[2].Trim()
            $data = if ($matches[3]) { $matches[3].Trim() } else { "<empty>" }
            
            $lastSlash = $fullPath.LastIndexOf('\')
            if ($lastSlash -gt 0) {
                $key = $fullPath.Substring(0, $lastSlash)
                $val = $fullPath.Substring($lastSlash + 1)
            } else {
                $key = $fullPath
                $val = "@"
            }
            
            $entries += [PSCustomObject]@{
                Category = $cat
                H1 = $currentH1
                Key = $key
                Value = $val
                Type = $regType
                Data = $data
                LineNum = $i + 1
                Raw = $line
            }
        }
    }
}

Write-Output "Parsed registry entries: $($entries.Count)"

$grouped = $entries | Group-Object { "$($_.Key)\$($_.Value)".ToLower() }
$dups = $grouped | Where-Object { $_.Count -gt 1 }

Write-Output "Unique key+value targets with multiple references: $($dups.Count)"

# Separate:
# 1. References within the SAME tweak (e.g. showing multiple states 0 vs 1, or option choices)
# 2. References ACROSS DIFFERENT tweaks (cross-tweak collisions)
$crossTweakCollisions = @()
$withinTweakVariants = @()

foreach ($g in $dups) {
    $distinctH1 = $g.Group | Select-Object -ExpandProperty H1 -Unique
    if ($distinctH1.Count -gt 1) {
        $crossTweakCollisions += $g
    } else {
        $withinTweakVariants += $g
    }
}

Write-Output "Within-tweak variants (options/states in same H1): $($withinTweakVariants.Count)"
Write-Output "Cross-tweak collisions (referenced across different H1s): $($crossTweakCollisions.Count)"

Write-Output "`n=== CROSS-TWEAK COLLISIONS ==="
foreach ($c in $crossTweakCollisions) {
    Write-Output "------------------------------------------------------------"
    Write-Output "Target: $($c.Name) (Count: $($c.Count))"
    foreach ($item in $c.Group) {
        Write-Output "  [$($item.Category)] H1: '$($item.H1)' (line $($item.LineNum)): Type=$($item.Type), Data='$($item.Data)'"
    }
}
