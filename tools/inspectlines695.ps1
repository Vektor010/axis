$c = Get-Content 'C:\Users\User\Documents\win-tweaker\src\ViewModels\MainViewModel.cs'
695..720 | ForEach-Object { "$($_): $($c[$_ - 1])" }
