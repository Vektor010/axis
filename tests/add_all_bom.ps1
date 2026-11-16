$files = Get-ChildItem "C:\Users\User\Documents\win-tweaker\tests\*.ps1"
foreach ($f in $files) {
    & "C:\dev\AddBom.exe" $f.FullName
}
