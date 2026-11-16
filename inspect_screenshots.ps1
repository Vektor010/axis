Add-Type -AssemblyName System.Drawing
$files = @(
    'docs\prototype\screenshot-dark.png',
    'docs\prototype\screenshot-light.png',
    'docs\prototype\screenshot-journal.png'
)
foreach ($f in $files) {
    $full = Resolve-Path $f
    $item = Get-Item $full
    $img = [System.Drawing.Image]::FromFile($full)
    $w = $img.Width
    $h = $img.Height
    $img.Dispose()
    $sha = (Get-FileHash $full -Algorithm SHA256).Hash
    Write-Host "Файл: $($item.FullName)"
    Write-Host "  Размер: $($item.Length) байт"
    Write-Host "  Пиксели: ${w}x${h}"
    Write-Host "  SHA-256: $sha"
    Write-Host ""
}
