# Lethal Menu

A menu mod for Lethal Company (BepInEx 5 plugin), with a website remote panel.

All support to IcyRelic and Dustin, the creators of Lethal Menu. This is a fixed-up version by TyGamer4.

## Building

- **On your PC:** `dotnet build -c Release`. It compiles against your installed game
  (`C:\Program Files (x86)\Steam\steamapps\common\Lethal Company`, change with `-p:GameDir=...`)
  and copies `LethalMenu.dll` into `BepInEx\plugins\LethalMenu`.
- **On GitHub:** every push runs the **Build** action. The game isn't installed there, so it uses the
  community's reference packages (`LethalCompany.GameLibs.Steam`, `BepInEx.Core`) instead. Download the DLL
  from the run's **Artifacts**. Pushing a tag like `v1.4.8` also creates a Release with the DLL attached.
  After a game update, bump `GameLibsVersion` in `LethalMenu.csproj` to the matching package version.

## Layout

- `Cheats/`, `Handler/`, `Menu/`, ... the mod itself
- `Remote/` the link to the website remote panel (Firebase)
- `RemotePanel/` Firebase setup: `database.rules.json`, `SETUP.md`. The website itself (`RemotePanel/public`)
  lives in its own repo, [NewTabGames/LM-Remote](https://github.com/NewTabGames/LM-Remote).
