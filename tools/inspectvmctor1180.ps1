$c = Get-Content 'C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs'
1180..1200 | ForEach-Object { "$($_): $($c[$_ - 1])" }
