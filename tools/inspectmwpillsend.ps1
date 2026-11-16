$c = Get-Content 'C:\Users\User\Documents\win-tweaker\MainWindow.xaml'
930..945 | ForEach-Object { "$($_): $($c[$_ - 1])" }
