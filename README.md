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

## Added/Fixed

### Added

**Website remote panel** (Firebase + GitHub Pages: [LM-Remote](https://github.com/NewTabGames/LM-Remote))
- Control the whole menu from a browser: every toggle, action, slider and setting, laid out in the same tabs and
  sections as the in-game menu (Visuals, Troll, Self, Players, Enemies, Map, Server, Settings), A-Z.
- Accounts for anyone (email, username, password). Pair your own PC with an 8-character code from the menu;
  codes refresh every 9 minutes and leftover codes are cleaned up after a crash.
- Live info bar: moon, phase, a card per player (health, inside/outside, dead/alive, held item), credits,
  quota, days left, host.
- Player cards (heal, kill, demi god, spectate, mini cam...) and enemy cards (control, kill, stun, hunt, anger,
  teleport) with one player picker for the whole Enemies tab.
- "Running now" bar to stop monster control, hunts, spectate, mini cam and free cam in one tap, plus the
  controlled monster's target and an "AI driving" switch.
- Live map: the moon's outside and the facility's inside drawn from the game's own floor, with players,
  monsters, the ship and entrances. Follow me, Outside, Inside or Both side by side; zoom and drag.
- Extra website actions: spawn enemies and items, change moon and weather, unlock ship items and suits, kill
  everyone / everyone else, steal items (Hoarding Bug), spawn web (Bunker Spider).
- Sliders under features that have one in game (FOV, speed, jump, climb, noclip, item slots, kill distance...).
- Stealth Mode switch, open/close the in-game menu, eject the menu.
- Works on old tablets (plain flat design, big tap targets), reloads itself when the site is updated, and
  clears the enemies when the PC goes offline. `?demo` shows it with sample data.

**Monster control**
- Controllers for the newer monsters: Kidnapper Fox, Cadaver Bloom, Giant Kiwi (Sapsucker), Puma, Stingray, Old
  Bird, plus a default one so any monster can be driven.
- New controls: left/right click skills, F or middle mouse to lunge at a nearby player, Shift sprint, Ctrl
  sneak, Space jump, scroll to zoom, T to pick the attack target (yourself included), F9 to let the AI drive.
- Each monster walks and runs at its own in-game speed; Manticoils can fly (Space up, Ctrl down, right click
  takes off / lands).
- Third-person orbit camera that fits each monster's size and doesn't go through walls; help box with the keys.
- Only one monster at a time; switching releases the old one cleanly.

**Hunts**
- Hunt Player: the monster walks to a player (even across the map) without being angered, then its normal AI
  takes over once it sees them or starts its own chase.
- Persistent Hunt: keeps going after the player until stopped.
- Aggro Hunt (Eyeless Dogs): walks until about 12 m away, then charges and bites; walks into the ship properly.
- Hunt With All: every monster on the map goes to the chosen player. Stop All Hunts.

**Menu**
- BepInEx plugin (loads on startup, Pause reloads it) and Eject, which removes the menu cleanly and restores
  everything it changed.
- Stealth Mode: no menu and no notifications on screen; only the website controls it (exit: L-Ctrl + L-Alt +
  Insert).
- Debug tab: lobby, remote link, controlled monster, hunts, every enemy, and the last errors and warnings.
- Credit line in the General tab; "Crawler" shown as "Thumper".

**Building**
- Builds against the current game, and on GitHub Actions without the game installed.

### Fixed

- Updated for the current Lethal Company: renamed and moved game methods, network calls and fields.
- Super Speed / Super Jump / Fast Climb were weaker than normal; they now start at double the game's default.
- Ship ESP, Turret ESP and other ESP that stopped drawing; one broken ESP no longer stops the rest.
- Mini Cam couldn't be turned off from the player list.
- Spider Web did nothing; Break All Webs threw errors; Spawn Masked Enemy didn't spawn one.
- Lightning Strike and Explode Closest Mine erroring when there was no storm or no mine.
- Map object spawns (landmines, turrets, spike traps) crashing on a missing container.
- Heal did nothing on other players as host; Kill could leave someone at 5 HP; an error on every client after
  some hurt/kill calls; Demi God sending a heal every frame.
- Monster control: freezing after F9, frozen first monster after switching, mid-air release teleporting it back,
  camera far too high on bees and small monsters, Manticoil turning the wrong way, Forest Giant error spam,
  lunge ignoring the monster's speed, cleanup not running if the monster despawned.
- Lures that never worked (Nutcracker, Snare Flea, Hoarding Bug, Kidnapper Fox).
- F12 taking a Steam screenshot (kill key moved to Delete); keybind capture errors; the keybind search box you
  couldn't leave; a settings-tab layout error; slider layout.
- Full Render Resolution not working.
- Website: dropdowns flickering, Enemies tab layout, wrapped status text, gaps between boxes.

## Credits

- **Original Lethal Menu:** [IcyRelic/LethalMenu](https://github.com/IcyRelic/LethalMenu), created by
  [IcyRelic](https://github.com/IcyRelic) and [Dustin21335](https://github.com/Dustin21335), with
  [everyone who contributed](https://github.com/IcyRelic/LethalMenu/graphs/contributors) to it.
- Built on [BepInEx](https://github.com/BepInEx/BepInEx) and [Harmony](https://github.com/pardeike/Harmony).

## Layout

- `Cheats/`, `Handler/`, `Menu/`, ... the mod itself
- `Remote/` the link to the website remote panel (Firebase)
- `RemotePanel/` Firebase setup: `database.rules.json`, `SETUP.md`. The website itself (`RemotePanel/public`)
  lives in its own repo, [NewTabGames/LM-Remote](https://github.com/NewTabGames/LM-Remote).
