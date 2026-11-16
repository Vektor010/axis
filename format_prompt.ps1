$raw = [System.IO.File]::ReadAllText('C:\Users\User\Documents\win-tweaker\last_prompt.txt', [System.Text.Encoding]::UTF8)
$formatted = $raw.Replace("\r\n", "`n").Replace("\n", "`n")
$formatted = [System.Text.RegularExpressions.Regex]::Replace($formatted, '(?m)^(?=[0-9А-ЯA-Z]\. )', "`n")
[System.IO.File]::WriteAllText('C:\Users\User\Documents\win-tweaker\last_prompt_clean.txt', $formatted, [System.Text.Encoding]::UTF8)
