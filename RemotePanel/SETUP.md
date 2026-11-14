# Lethal Menu Remote: how it works and how to set it up

Anyone can make an account on the website and control **their own** PCs running Lethal Menu.
Each account only ever sees and controls the PCs paired to it.

## For players (anyone using the panel)
1. Install BepInEx and put `LethalMenu.dll` in `BepInEx\plugins`.
2. Start Lethal Company. In the menu go to **Settings > Remote Panel > Enable Remote**.
   An 8-character pairing code appears (it changes every ~9 minutes until used).
3. Open the website, press **Create an account** (email, username, password) or sign in.
4. Press **Add PC** and type the code. The PC connects within a few seconds.

To unpair: **Remove this PC** on the website, or **Pair Again** in the menu. Either way the menu shows a new code.

## For the panel owner (one-time Firebase setup, already done for LM-Remote)
- Firebase project **lm-remote**: Realtime Database, Authentication with **Email/Password** and **Anonymous** enabled.
- Database rules: paste `database.rules.json` into **Realtime Database > Rules** and **Publish**.
- The website is the `public` folder (hosted on GitHub Pages). Add the site's domain under
  **Authentication > Settings > Authorized domains**.
- The mod has the project's apiKey/databaseURL built in (`Remote/RemoteConfig.cs`), so players do not edit any files.
  `lethalmenu.remote.json` in the game folder can still override them.

## Database layout
| Path | Written by | Read by |
|---|---|---|
| `usernames/{name}` = uid | the account claiming the name | any signed-in user (to check if taken) |
| `users/{uid}/profile`, `users/{uid}/devices/{pc}` | that account | that account |
| `pairCodes/{CODE}` = {device, name, at} | the PC (expires after 10 min) | signed-in accounts that know the code |
| `devices/{pc}/owner` = {uid, username, code} | the account, only with a valid code | the PC |
| `devices/{pc}/info, catalog, state, lastResult` | the PC, once paired | its owner |
| `devices/{pc}/commands/{id}` | its owner (create), the PC (delete) | the PC |

## Notes
- `index.html?demo` shows the panel with sample data.
- The site auto-reloads within about a minute after new files are uploaded.
- Free Firebase (Spark) limits: 100 simultaneous connections and 10 GB downloads/month. Each connected PC plus each open
  panel counts as a connection. Fine for a group of friends; upgrade the plan if lots of people use it.
