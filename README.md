# pantheon-plugin

A personal BepInEx 6 IL2CPP plugin for the MMORPG **Pantheon: Rise of the Fallen**. It uses HarmonyLib to patch game methods at runtime and adds a small in-game IMGUI panel with toggles. Most of the plugin is exploratory — a workbench for poking at how the game's code is wired, not a polished cheat suite.

## Status

Pantheon is a server-authoritative MMO, so the realistic scope of a client-side mod is narrow:

- **Works** — anything wholly client-side: movement speed, fly mode, gravity, sneak, breath-related stats. The `CharacterMover` patches reliably take effect.
- **Visual only** — anything the server rolls (damage, crit, hit/dodge, XP/level, currency, equipment stats). The plugin can deceive the local client's display but does not alter the canonical state on the server.
- **Closed** — there is no client→server RPC that carries a damage value, a crit roll, or a stat override. `Pools.__RpcMethods` has no outgoing pool RPC; `Abilities.RequestAbilityCast` carries only an ability id; `LegacyHandlersDoNotAddAnythingHere.RequestAutoAttackRpc` carries only a boolean. Server runs cast timing, swing timer, and damage rolls.

A handful of in-development patches are currently disabled because they crashed the game on load — see commented blocks in `Plugin.cs` and the `3f0f60f` commit.

## Features (current panel)

- `+1 Level` — calls `Experience.AddLevel`. Visual on the client; the server does not honour this.
- `+/- 0.5 speed mult` — drives `CharacterMover.SetMoveSpeedMultiplier`. Real effect.
- `Fly Mode` — toggles `CharacterMover.SetIsFlying`. Real effect.
- `Haste +/- 100%` — overrides `StatFormulas.ModifyValueByHastePercent`. Affects local timings only.
- `Sneak` — forces `CombatEffects.SetStealth(true)`.
- `Show pos` — logs the local player's world coordinates.
- `Visual Lv 99` — overrides incoming `Experience.__RpcMethods.SetLevelFromServer` to display level 99 client-side.
- `Almost level up` — sets the local XP value to one short of the next level (visual only).
- `RPC log` — toggles instrumentation that logs key combat/network RPCs to `BepInEx/LogOutput.log`. Useful for confirming what the server actually sends.
- Several toggles (`GodMode (vis)`, `Dmg Infl`) are wired into the UI but their backing patches are currently disabled.

The auxiliary 5-second timer writes the local player's `(x, y)` to a file named `loc` next to the plugin DLL, consumed by `loc.py`.

## Requirements

- Pantheon: Rise of the Fallen, installed via `PantheonLauncher` (the `App\` subfolder is what BepInEx targets).
- BepInEx 6 IL2CPP — currently pinned to BE build **755**. Install once, launch the game once so it generates `BepInEx\interop\`, then build this plugin.
- Visual Studio 2022 Community **or** the .NET 6 SDK for `dotnet build`.
- Windows. The repository contains a few `.sh` scripts from a legacy Steam Deck workflow; they are not part of the current build.

## Build & install

```
dotnet build
```

The csproj's `DeployToGame` target runs after every build and copies `bin\Debug\net6.0\PantheonPlugin.dll` into `<game>\BepInEx\plugins\`. The default game path matches a standard PantheonLauncher install:

```
%USERPROFILE%\OneDrive\Documents\My Games\Pantheon\App
```

To override on a different machine, drop a sibling `PantheonPlugin.csproj.user` (gitignored) with a `<PantheonGamePath>` property. Detail in [CLAUDE.md](./CLAUDE.md).

The game log is at `<game>\BepInEx\LogOutput.log`.

## Project layout

- `Plugin.cs` — single-file plugin (entry point, IMGUI panel, all Harmony patches).
- `PantheonPlugin.csproj`, `.sln` — build.
- `nuget.config` — BepInEx NuGet feed.
- `CLAUDE.md` — deeper architecture notes, conventions, BepInEx version pinning, the Python tooling, the legacy Steam Deck scripts.
- `loc.py`, `plot.py`, `shark.py`, `scale.py`, `world.png` — reverse-engineering helpers (packet parsing, position plotting). Independent of the plugin build.
- `*.sh`, `tcpdump.sh`, etc. — legacy Steam Deck scripts, kept for reference.

## Notes

This is a personal experimentation and reverse-engineering project against a paid early-access game. Running it on a live server may violate the game's EULA and could result in account action. Use at your own risk; no warranty, no support.

Pull requests are not particularly expected — the code is messy on purpose, with commented-out blocks left in as a backlog rather than dead code to clean up.
