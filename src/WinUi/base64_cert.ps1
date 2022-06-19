
$dir = Get-Location
[System.IO.Directory]::SetCurrentDirectory($dir)

$pfx_cert = [System.IO.File]::ReadAllBytes("WinUi_TemporaryKey.pfx")

[System.Convert]::ToBase64String($pfx_cert) | Out-File 'WinUi_TemporaryKey_Base64.txt'