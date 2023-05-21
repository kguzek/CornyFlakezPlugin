param([string]$PluginRoot)

$AssemblyInfoFile = Get-Content -Path "$PluginRoot\Properties\AssemblyInfo.cs"

function Increment {
  param (
    [string[]]$FileString,
    [string]$Attribute
  )
  
  "$AssemblyInfoFile" -match "$Attribute\(`"(?<version>(?:[0-9]+\.){3}[0-9]+)`"\)" | Out-Null
  $Version = [version]($Matches.version)

  $NewVersion = New-Object -TypeName System.Version -ArgumentList $Version.Major, $Version.Minor, $Version.Build, ($Version.Revision + 1)
  $FileString | ForEach-Object { $_.Replace("$Attribute(`"$Version`")", "$Attribute(`"$NewVersion`")") }
}

if ($PluginRoot -match 'CornyFlakezPlugin$')
{
  $AssemblyInfoFile = Increment -FileString $AssemblyInfoFile -Attribute "AssemblyVersion"
}
$AssemblyInfoFile = Increment -FileString $AssemblyInfoFile -Attribute "AssemblyFileVersion"
$AssemblyInfoFile | Set-Content -Path "$PluginRoot\Properties\AssemblyInfo.cs"

