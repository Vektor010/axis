$files = @(
    'App.xaml.cs',
    'MainWindow.xaml',
    'MainWindow.xaml.cs',
    'Tests\Program.cs',
    'Tests\Tests.csproj',
    'Tweaker.csproj',
    'cmd\analyzer\main.go',
    'data\tweaks.json',
    'docs\evidence\example-minimal-effects.txt',
    'docs\evidence\nohuto-repos.csv',
    'docs\generated\stats.md',
    'docs\known-issues.md',
    'docs\prototype\screenshot-dark.png',
    'docs\prototype\screenshot-journal.png',
    'docs\prototype\screenshot-light.png',
    'src\Common\NativeMethods.cs',
    'src\Engine\JournalManager.cs',
    'src\Engine\RestorePointManager.cs',
    'src\Engine\TweakEngine.cs',
    'src\Models\TweakDefinition.cs',
    'src\ViewModels\MainViewModel.cs',
    'tests\measure.ps1',
    'tests\negative.ps1'
)
foreach ($f in $files) {
    if (Test-Path $f) {
        $item = Get-Item $f
        $hash = (Get-FileHash $item.FullName -Algorithm SHA256).Hash
        Write-Host "$($item.FullName) | $($item.Length) | $hash"
    }
}
