& $PSScriptRoot\..\Increment-BuildNumber.ps1 -PluginRoot $PSScriptRoot
dotnet build
if (-not $?) {
  throw 'Build failure -- aborting script'
}
Copy-Item $PSScriptRoot\bin\Debug\net48\CornyFlakezPlugin2.* 'C:\Program Files\Rockstar Games\Grand Theft Auto V\Plugins\'
Copy-Item $PSScriptRoot\bin\Debug\net48\System.* 'C:\Program Files\Rockstar Games\Grand Theft Auto V\'
Copy-Item $PSScriptRoot\bin\Debug\net48\Microsoft.* 'C:\Program Files\Rockstar Games\Grand Theft Auto V\'
