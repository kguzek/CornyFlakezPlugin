dotnet build
if (-not $?) {
  throw 'Build failure -- aborting script'
}
Copy-Item .\bin\Debug\net48\CornyFlakezPlugin.*  'C:\Program Files\Rockstar Games\Grand Theft Auto V\Plugins\LSPDFR\'
Copy-Item .\bin\Debug\net48\System.* 'C:\Program Files\Rockstar Games\Grand Theft Auto V\'
