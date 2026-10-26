# Connecting EchoClone to Nomad from an iPhone + Quest 3S

Target reported by the user: **Blade & Sorcery: Nomad 1.3.2.261012003**.

## What “connect to the game” means

There is no remote connection API that lets this GitHub repository attach to the running Quest game. EchoClone must be built as a **Nomad-compatible scripted mod** and then installed by the game. The repo is not connected to the headset, and no mod has been installed on the device.

The iPhone can edit this repository and start GitHub Actions. The Quest can install a published compatible mod using the game's in-game Mods manager. Those are separate steps; a successful source-code test does not connect the mod to the running game.

## Important build correction

Do **not** build a new Android APK for this project. EchoClone is a mod loaded inside the existing Nomad app, not a replacement game or standalone Quest app. The deliverable should be a mod folder/ZIP containing a compiled C# plugin DLL and the required mod metadata (including a correct manifest.json), plus any assets the mod actually needs.

A normal .NET build of the current game-independent core is not the game plugin. A Unity Android build would produce the wrong kind of deliverable.

## Verified repository state

- The recorder core is game-independent and has automated source checks.
- The repository does not yet contain a Nomad plugin entry point or compiled plugin.
- No matching game reference assemblies are stored in this repository.
- No real implementation currently reads Quest poses, spawns a creature, drives a humanoid rig, adds working ModOptions buttons, or handles combat/dismemberment.
- The project has not been installed or tested on the user's Quest 3S.

## The next hard gate: matching references

A real plugin must compile against the game/API references matching the user's installed Nomad version. BasSDK's public documentation explains ModOptions and ThunderScript, but docs alone are not enough to guarantee exact method signatures or behavior for creature spawning, humanoid motion control, damage, or native dismemberment.

The game update notes for 1.3.2.2 specifically mention adding Unity.XR.OpenXR to the game assemblies list to avoid mod-loading issues:
- https://steamdb.info/patchnotes/25901209/

Do not commit proprietary game DLLs to this public repository. The next implementation step needs a legitimate matching reference set or a trustworthy public Nomad mod source/project that demonstrates compatible APIs for the exact target version.

## What the user can do with only the iPhone and Quest

1. Keep the repository public and use GitHub Actions to run source-level tests.
2. Once a correct plugin is built, prepare a Nomad mod folder with its DLL and manifest; do not package an APK.
3. Publish the mod to the Blade & Sorcery: Nomad section of mod.io, if the publisher account and upload requirements are met.
4. On Quest 3S, open Blade & Sorcery: Nomad → Mods and install the published Nomad mod from the in-game manager.
5. Test the actual features in-game. A green CI result is not a substitute for this step.

The official community install guide recommends the in-game Mod Manager and notes that manual Android file transfer is harder on recent Android versions and generally involves connecting the Quest to a computer:
- https://kospy.github.io/BasSDK/Components/Guides/HowToInstallMods.html

## Feature implementation order

1. Build/load a minimal Nomad plugin and verify it logs a startup message.
2. Add real ModOptions controls (Spawn, Record, Stop, Playback, Hostile Mode) that invoke implemented methods.
3. Capture head/hand transforms using the actual tracking API available in Nomad.
4. Spawn a vanilla human using a verified catalog ID and supported spawn API.
5. Prototype playback safely; replaying tracked head/hand transforms onto a normal NPC is not automatically the same as controlling its animated, physics-driven rig.
6. Add hostile AI and test attacks against the player.
7. Verify native damage and dismemberment with in-game tests.

Do not mark a milestone complete until its build/test evidence exists. In particular, do not claim hostile combat or native dismemberment works just because the clone spawns.
