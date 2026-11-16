$f = "C:\Users\User\Documents\win-tweaker\Tests\Tests.csproj"
$txt = [System.IO.File]::ReadAllText($f)
$target = '<Compile Include="..\src\Engine\SteamConfigHelper.cs" />'
$replacement = "<Compile Include=""..\src\Engine\SteamConfigHelper.cs"" />`r`n    <Compile Include=""..\src\Engine\GameModeWnfService.cs"" />"
$txt = $txt.Replace($target, $replacement)
[System.IO.File]::WriteAllText($f, $txt, [System.Text.Encoding]::UTF8)
Write-Host "Updated Tests.csproj successfully!"
