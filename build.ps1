dotnet build
Copy-Item .\CornyFlakezPlugin\bin\Debug\net472\CornyFlakezPlugin.dll  'C:\Program Files\Rockstar Games\Grand Theft Auto V\Plugins\LSPDFR\'
Copy-Item .\CornyFlakezPlugin\bin\Debug\net472\CornyFlakezPlugin.pdb  'C:\Program Files\Rockstar Games\Grand Theft Auto V\Plugins\LSPDFR\'
Copy-Item .\CornyFlakezPlugin2\bin\Debug\CornyFlakezPlugin2.dll 'C:\Program Files\Rockstar Games\Grand Theft Auto V\Plugins\'
Copy-Item .\CornyFlakezPlugin2\bin\Debug\CornyFlakezPlugin2.pdb 'C:\Program Files\Rockstar Games\Grand Theft Auto V\Plugins\'
Write-Host "Compiled plugins and moved to GTA V LSPDFR Plugins directory!"
