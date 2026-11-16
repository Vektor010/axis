$files = Get-ChildItem -Recurse "C:\Users\User\Documents\win-tweaker\data\source\win-config" -Filter "*.md"

foreach ($f in $files) {
    $lines = Get-Content $f.FullName
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^\|\s*Policy\s*\|' -or $lines[$i] -match '^\|\s*Setting\s*\|' -or $lines[$i] -match '^\|\s*Option\s*\|') {
            $rel = $f.FullName.Substring("C:\Users\User\Documents\win-tweaker\data\source\win-config\".Length)
            Write-Host "$rel : line $($i + 1): $($lines[$i])"
        }
    }
}
