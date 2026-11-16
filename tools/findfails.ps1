$lines = Get-Content -Raw "C:\Users\User\.gemini\antigravity\brain\6094a221-bf4d-4b38-be73-a0487ddbba7b\.system_generated\steps\16080\output.txt" -Encoding UTF8
$matches = [System.Text.RegularExpressions.Regex]::Matches($lines, "(?m)^\[FAIL\].*")
foreach ($m in $matches) {
    [Console]::WriteLine($m.Value)
}
