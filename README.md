# Lethal Menu

A menu mod for Lethal Company (BepInEx 5 plugin), with a website remote panel.

All support to IcyRelic and Dustin, the creators of Lethal Menu. This is a fixed-up version by TyGamer4.

## Requirements

- **BepInEx 5** (Windows x64), **5.4.21 or newer**. Tested with **BepInEx 5.4.23.6**.
  BepInEx 6 is not supported.
- Lethal Company (Steam), the current version.

## Install

1. Install [BepInEx 5](https://github.com/BepInEx/BepInEx/releases) into the Lethal Company folder and run the
   game once so it creates its folders.
2. Put `LethalMenu.dll` in `BepInEx\plugins\LethalMenu\`.
3. Start the game.

## Building

- **On your PC:** `dotnet build -c Release`. It compiles against your installed game
  (`C:\Program Files (x86)\Steam\steamapps\common\Lethal Company`, change with `-p:GameDir=...`)
  and copies `LethalMenu.dll` into `BepInEx\plugins\LethalMenu`.
- **On GitHub:** every push runs the **Build** action. The game isn't installed there, so it uses the
  community's reference packages (`LethalCompany.GameLibs.Steam`, `BepInEx.Core`) instead. Download the DLL
  from the run's **Artifacts**.

## Credits

- **Original Lethal Menu:** [IcyRelic/LethalMenu](https://github.com/IcyRelic/LethalMenu), created by
  [IcyRelic](https://github.com/IcyRelic) and [Dustin21335](https://github.com/Dustin21335), with
  [everyone who contributed](https://github.com/IcyRelic/LethalMenu/graphs/contributors) to it.
- **This version** (fixes, website remote panel, monster control and hunts): TyGamer4.
- Built on [BepInEx](https://github.com/BepInEx/BepInEx) and [Harmony](https://github.com/pardeike/Harmony).

## Layout

- `Cheats/`, `Handler/`, `Menu/`, ... the mod itself
- `Remote/` the link to the website remote panel (Firebase)
- `RemotePanel/` Firebase setup: `database.rules.json`, `SETUP.md`. The website itself (`RemotePanel/public`)
  lives in its own repo, [NewTabGames/LM-Remote](https://github.com/NewTabGames/LM-Remote).
