$content = Get-Content "C:\Users\User\Documents\win-tweaker\data\source\win-config\system\desc.md" -Raw
$matches = [regex]::Matches($content, "(?ms)```[^\r\n]*\r?\n(.*?)\r?\n```")
Write-Host ("Total code blocks in system/desc.md: " + $matches.Count)
for ($i = 0; $i -lt [math]::Min(10, $matches.Count); $i++) {
    Write-Host ("`n--- Block " + $i + " ---")
    $block = $matches[$i].Groups[1].Value
    $blockLines = ($block -split "`r?`n") | Select-Object -First 6
    foreach ($l in $blockLines) {
        Write-Host $l
    }
}
