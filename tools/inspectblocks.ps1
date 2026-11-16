$files = Get-ChildItem "C:\Users\User\Documents\win-tweaker\tools\AddBlock*.cs"
foreach ($f in $files) {
    $c = (Select-String -Path $f.FullName -Pattern "new TweakItem|new TweakDef").Count
    Write-Host "$($f.Name): $c tweaks"
}
