$shots = Get-ChildItem "C:\Users\User\Documents\win-tweaker\docs\prototype\*.png"
Add-Type -AssemblyName System.Drawing
foreach ($f in $shots) {
    $img = [System.Drawing.Image]::FromFile($f.FullName)
    $dim = "$($img.Width)x$($img.Height)"
    $img.Dispose()
    $h = (Get-FileHash $f.FullName -Algorithm SHA256).Hash
    Write-Host "$($f.Name) | $($f.FullName) | $($f.Length) bytes | $dim | $h"
}
