# CornyFlakezPlugin

This repository contains two projects, `CornyFlakezPlugin` and `CornyFlakezPlugin2`, which are plugins for LSPDFR and RPH.

## Installation

Download the latest release and copy all files into your root GTA V directory.

## Compilation

You may also compile the plugin from source locally.
First, clone the repository:
```sh
git clone git@github.com:kguzek/CornyFlakezPlugin.git
```
Then, compile the project.
To do this there is an included build script for Windows which assumes your game is installed in the default location
(`C:\Program Files\Rockstar Games\Grand Theft Auto V`).
```pwsh
cd CornyFlakezPlugin
.\Build-Plugins.ps1
```
Otherwise, you may do this step manually. You can open the `Build-Plugins.ps1` file with a text editor and see what it does.
The main steps are running `dotnet build` and copying the build output into the appropriate plugin folders, as well as copying the referenced system DLLs to the root game folder.

## CornyFlakezPlugin

This is an LSPDFR plugin which adds new callouts and loadout functionality.

### Configuration

The the plugin's file directory is in `Plugins/LSPDFR/CornyFlakezPlugin`. Here, there is a configuration file called `settings.ini`.
It contains basic settings which slightly affect the plugin's functionality and may be changed freely.
The supported callsign format is Division-Unit-Beat, where Unit is a code word from the [LAPD phonetic alphabet](https://en.wikipedia.org/wiki/APCO_radiotelephony_spelling_alphabet).

### Custom loadouts

Loadouts can be defined in `WeaponLoadouts.xml` in the plugin directory: `Plugins/LSPDFR/CornyFlakezPlugin`. These will be applied when the user goes on duty.

### Callouts

Currently, there are two added callouts in CornyFlakezPlugin.

#### Vehicle Pursuit

This is a basic vehicle pursuit which practically doesn't differ from the built-in pursuit. It was made while learning the basics of RPH plugin development.

#### Police escort

This is a linear callout which involves escorting a VIP from a start location (currently only City Hall) to the LSIA.
I am planning on adding more events to it (random attacks on VIP etc.) but currently each playthrough will be the same.

## CornyFlakezPlugin2

This is a debug plugin for RagePluginHook (it does not require LSPDFR) that I created to ease the develpoment of CornyFlakezPlugin.
It has two menus activated by two keybinds.

### Main menu

This menu can be opened by pressing `]`. It contains options pertaining to spawning peds and vehicles, as well as emulating LSPDFR callouts.
The callout emulator is particularly useful as it means you can update the callout code and test it without reloading LSPDFR.
This menu also contains a `Reload plugin` option, which further helps with development when testing new versions.

### Debug menu

This menu can be opened by pressing `[` and it contains information regarding peds and vehicles belonging to the active callouts.

#### Thank you for reading!
