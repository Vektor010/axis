Add-Type -AssemblyName System.Drawing
$targets = @(
    'docs\prototype\screenshot-dark.png',
    'docs\prototype\screenshot-light.png',
    'docs\prototype\screenshot-journal.png'
)
foreach ($t in $targets) {
    $item = Get-Item $t
    $img = [System.Drawing.Image]::FromFile($item.FullName)
    $w = $img.Width
    $h = $img.Height
    $img.Dispose()
    $hash = (Get-FileHash $item.FullName -Algorithm SHA256).Hash
    Write-Host "Файл: $($item.FullName)"
    Write-Host "Размер: $($item.Length) байт"
    Write-Host "Разрешение: $($w)x$($h)"
    Write-Host "SHA-256: $hash"
    Write-Host "--------------------------------------------------------"
}
