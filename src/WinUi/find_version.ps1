$pattern = '[^\w]Version="((\d.?)+)"'
$input_string = Get-Content Package.appxmanifest 
$pattern_match = [regex]::Matches($input_string, $pattern)
Write-Output $pattern_match.Groups[1].Value