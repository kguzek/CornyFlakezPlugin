& $PSScriptRoot\..\Increment-BuildNumber.ps1 -PluginRoot $PSScriptRoot
dotnet build
if (-not $?) {
  throw 'Build failure -- aborting script'
}
Copy-Item $PSScriptRoot\bin\Debug\net48\CornyFlakezPlugin.*  'C:\Program Files\Rockstar Games\Grand Theft Auto V\Plugins\LSPDFR\'
Copy-Item $PSScriptRoot\bin\Debug\net48\System.* 'C:\Program Files\Rockstar Games\Grand Theft Auto V\'
