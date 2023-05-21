dotnet build
if (-not $?) {
  throw 'Build failure -- aborting script'
}
Copy-Item .\bin\Debug\net48\CornyFlakezPlugin2.* 'C:\Program Files\Rockstar Games\Grand Theft Auto V\Plugins\'
Copy-Item .\bin\Debug\net48\System.* 'C:\Program Files\Rockstar Games\Grand Theft Auto V\'
Copy-Item .\bin\Debug\net48\Microsoft.* 'C:\Program Files\Rockstar Games\Grand Theft Auto V\'
