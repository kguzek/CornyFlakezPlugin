Set-Location .\CornyFlakezPlugin
.\Build-CornyFlakezPlugin.ps1
Set-Location ..\CornyFlakezPlugin2
.\Build-CornyFlakezPlugin2.ps1
Write-Host 'Built both plugins and installed them in the appropriate directories.'
