$c = Get-Content 'C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs'
1184..1192 | ForEach-Object { "$($_): $($c[$_ - 1])" }
