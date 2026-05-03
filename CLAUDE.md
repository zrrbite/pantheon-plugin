# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A BepInEx IL2CPP plugin for the MMORPG **Pantheon: Rise of the Fallen** (Unity, IL2CPP backend). The plugin uses **HarmonyLib** to patch game methods at runtime — overriding move speed, gravity, fly state, attack speed, weapon damage, haste, etc. — and adds an in-game IMGUI panel (`Hmm : MonoBehaviour`) with buttons for cheats like "+1 Level", speed mult, and fly toggle.

The dev environment is **Windows + Visual Studio 2022 Community** with the game installed locally via Steam. (The `*.sh` scripts and `192.168.86.42` references are leftovers from a previous Steam Deck workflow — see "Legacy Steam Deck scripts" below.)

## Build & deploy

Open `PantheonPlugin.sln` in VS 2022 and build, or from PowerShell run `dotnet build`. The `BepInEx.Unity.IL2CPP` package comes from the BepInEx NuGet feed declared in `nuget.config`; first restore will pull it.

The csproj's `DeployToGame` MSBuild target runs after every build and copies `bin\Debug\net6.0\PantheonPlugin.dll` straight into `<game>\BepInEx\plugins\`. The target is gated on `Exists($(PantheonPluginsPath))`, so it silently no-ops on machines without the game installed.

The default game path matches the standalone `PantheonLauncher` install — Pantheon.exe lives in the `App\` subfolder:

```
%USERPROFILE%\OneDrive\Documents\My Games\Pantheon\App
```

To override (different machine / different install), drop a sibling **`PantheonPlugin.csproj.user`** file (gitignored):

```xml
<Project>
  <PropertyGroup>
    <PantheonGamePath>D:\Path\To\Pantheon\App</PantheonGamePath>
  </PropertyGroup>
</Project>
```

The game log is at `<game>\BepInEx\LogOutput.log`.

## Interop assemblies

The plugin compiles against Il2Cpp interop DLLs that BepInEx generates **the first time the game is launched after BepInEx is installed**. They live in `<game>\BepInEx\interop\`. The csproj globs every `*.dll` from that folder via `<Reference Include="$(PantheonInteropPath)\*.dll">`, so as long as `PantheonGamePath` resolves correctly the references just work — there is nothing to copy or check in.

When you see types like `EntityPlayerGameObject`, `Equipment.Logic`, `StatFormulas`, `StatType`, `EquipSlotType`, `CharacterMover`, `Pools`, `ItemTemplate`, `LogicalGraphNodes.AttackSpeedCalculator` — all of these come from the interop assemblies, not from any source file in this repo. Method signatures (including IL2CPP-only oddities like `[DefaultParameterValue(null)]` decorations) are documented inline as comments in `Plugin.cs`; trust those comments when looking up patch targets.

If the game updates and method signatures change, relaunch the game once with BepInEx active to regenerate `BepInEx\interop\`, then rebuild.

## Legacy Steam Deck scripts

`PatchBepinx.sh`, `upload.sh`, `catlog.sh`, `dl_interop.sh`, and `tcpdump.sh` were all targeted at a Steam Deck at `deck@192.168.86.42`. They are kept for reference but are not part of the current Windows workflow.

The plugin is built against **BepInEx-Unity.IL2CPP BE build 755** (pinned in `PantheonPlugin.csproj` as `6.0.0-be.755`). When bumping BepInEx, change both the runtime install in `<game>\App\` and the `PackageReference` version in the csproj together so compile-time and runtime stay in lockstep.

## Architecture

Single-file plugin (`Plugin.cs`, ~750 lines). Layout:

- `Plugin : BasePlugin` — entry point; `Load()` calls `Harmony.PatchAll()` and registers the `Hmm` MonoBehaviour for the cheat-button GUI.
- `Hmm : MonoBehaviour` — in-game `OnGUI()` panel; buttons mutate static fields (`SpeedMult`, `Fly`) that the Harmony patches read.
- `PlayerNetwork` — container class holding all `[HarmonyPatch]` inner classes. Its `NetworkStart` postfix captures the local `EntityPlayerGameObject` into the static `LocalPlayer` field (filtered to the player matching `EntityPlayerGameObject.LocalPlayerId`, ignoring the placeholder NetworkId == 1). Most other patch classes (`CharacterGravityPatch`, `CharacterMoverPatch`, `FlyPatch`, `AtkSpeedPatch`, `MaxDmg`, `HasteValuePatch`, `TeleportPatch`) are Harmony Prefix patches that mutate `ref` parameters.
- A `System.Timers.Timer` started in `NetworkStart` ticks every 5s and writes the player's `(x,y)` position to a file named `loc` next to the plugin DLL — consumed by `loc.py` (see below).

Lots of commented-out patch blocks remain as scratch notes for what has and hasn't worked (e.g. `BonusSpellDamagePatch`, `ImmortalPatch`, `LevelUpPatch`). Treat them as a backlog, not dead code to delete.

## Python tooling (reverse-engineering helpers)

These scripts are independent of the plugin build — they help analyze the game's network traffic and visualize position data.

- `tcpdump.sh` — pipes live `tcpdump` of UDP 7000–8000 from the Steam Deck into a Python parser. Requires `setcap cap_net_raw,cap_net_admin=eip` on tcpdump on the Deck (see comments in `shark.py`).
- `shark.py` — parses position packets: header byte `0x1a`, 35-byte payload, three little-endian floats at offsets 15/20/25 = (x, y, z).
- `plot.py` — same parser, but draws each fix on `world.png` using the calibration in the file (do not change `calibration_offset` / `world_xmin`/`xmax` casually — they're hand-tuned to the map image).
- `scale.py` — standalone math sandbox for the world→map projection; not run as part of any pipeline.
- `loc.py` — polls the `loc` file written by the plugin and opens `shalazam.info/maps` in Playwright at the current coordinates. Run with `python3.11`.

## Conventions worth knowing

- `Log` is `Plugin.Log` (a `BepInEx.Logging.ManualLogSource`) and is `internal static new`. Use `Plugin.Log.LogInfo(...)` from anywhere in the plugin; in patch classes the surrounding `PlayerNetwork` already references it as `Log`.
- Harmony patches that need to read shared state (e.g. `SpeedMult`, `Fly`, `LocalPlayer`) read it from `Plugin`'s static fields. Don't introduce DI here — the static-field pattern is intentional given how patches are invoked.
- Every cheat button wraps its body in `try/catch` and logs the exception message rather than letting it bubble — IL2CPP exceptions in IMGUI handlers tend to wedge the game otherwise. Keep that pattern when adding buttons.
- The plugin's `Harmony` instance ID is `"pantheon"` and `MyPluginInfo.PLUGIN_GUID/NAME/VERSION` come from a generated `MyPluginInfo` class (BepInEx source-generates this from the csproj's assembly metadata — another reason the csproj matters).
