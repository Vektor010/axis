$p1 = "C:\Users\User\Documents\win-tweaker\Tweaker.csproj"
$c1 = [System.IO.File]::ReadAllText($p1)
if (-not $c1.Contains("GpuHwschService.cs")) {
    $c1 = $c1.Replace('<Compile Include="src\Engine\GameModeWnfService.cs" />', "<Compile Include=""src\Engine\GameModeWnfService.cs"" />`r`n    <Compile Include=""src\Engine\GpuHwschService.cs"" />")
    [System.IO.File]::WriteAllText($p1, $c1, [System.Text.Encoding]::UTF8)
    Write-Host "Registered in Tweaker.csproj"
}

$p2 = "C:\Users\User\Documents\win-tweaker\Tests\Tests.csproj"
$c2 = [System.IO.File]::ReadAllText($p2)
if (-not $c2.Contains("GpuHwschService.cs")) {
    $c2 = $c2.Replace('<Compile Include="..\src\Engine\GameModeWnfService.cs" />', "<Compile Include=""..\src\Engine\GameModeWnfService.cs"" />`r`n    <Compile Include=""..\src\Engine\GpuHwschService.cs"" />")
    [System.IO.File]::WriteAllText($p2, $c2, [System.Text.Encoding]::UTF8)
    Write-Host "Registered in Tests.csproj"
}
