# Research and completion status (2026-10-26)

This document records the current technical state of EchoClone Motion Recorder and the verified steps needed to finish it. It deliberately separates source checks from a real game build and headset validation.

## Current repository state

Repository: https://github.com/Unknown55676/EchoCloneMotionRecorder

The Nomad integration is a first gameplay prototype, not a finished mod. It declares ModOptions actions and attempts to spawn the vanilla `HumanMale` creature, record player head/hand transforms, and move the spawned creature root along the recorded head path.

Not implemented yet:
- retargeting recorded head, left hand, and right hand transforms onto the NPC skeleton;
- finding and recording the actual held weapon transform and replaying weapon motion;
- making the clone hostile using supported creature/faction/brain APIs;
- reliably applying damage to the player during replay;
- confirming native dismemberment interactions;
- compiling the game-side project against the exact Nomad 1.3.2.261012003 managed assemblies;
- launching and testing the DLL in Quest.

The source-validation workflow passing is not proof of plugin compatibility. It currently builds/tests only the game-independent core and checks that integration source contains expected tokens.

## Verified documentation

- [ThunderScript guide](https://kospy.github.io/BasSDK/Components/Guides/ThunderScript/): documents `ScriptLoaded(ModManager.ModData)` and the ThunderScript lifecycle.
- [ModOptions guide](https://kospy.github.io/BasSDK/Components/Guides/ModOptions/): documents static method/field options, button attributes, option value sources, and categories.
- [JSON modding guide](https://kospy.github.io/BasSDK/Components/Guides/SDK-HowTo/JSONModding.html): confirms that a mod folder needs a `manifest.json`, and explains the game's JSON/catalog approach.
- [Nomad installation guide](https://kospy.github.io/BasSDK/Components/Guides/HowToInstallMods.html): describes the in-game Mod.io manager and manual Android installation. Manual access to Android/data is restricted on newer Quest Android versions.
- [Nomad-specific FAQ](https://kospy.github.io/BasSDK/Components/Guides/FAQ/AndroidFAQ.html): emphasizes Nomad-specific optimization and that Nomad is a separate Android build; do not substitute PCVR assemblies.

These sources support the general mod structure and API patterns, but do not verify that every API signature in this repository matches the exact target game build.

## Required completion sequence

### 1. Obtain exact target references and compile

The plugin project expects `ThunderRoad.dll` and `UnityEngine.CoreModule.dll` from the target Nomad game's managed assemblies. Keep those proprietary DLLs out of this public repository.

Build command:

```sh
dotnet build src/EchoCloneMotionRecorder.Nomad/EchoCloneMotionRecorder.Nomad.csproj -c Release
```

Set `NOMAD_GAME_MANAGED_DIR` to the directory containing those assemblies. Resolve all compile errors against the actual target references; do not silence errors by guessing method signatures or substituting PCVR DLLs.

### 2. Make recording timing deterministic

Review timestamp generation in `EchoCloneThunderScript.RecordUpdate`. It should use one monotonic elapsed recording clock and add exactly one frame per sample; avoid deriving the next timestamp from a duration that may be computed from the existing frame list. Add tests for irregular frame rates and recording start/stop/restart.

### 3. Implement clone body retargeting

At spawn completion, cache the creature's supported animator/rig transforms using APIs confirmed from the target assemblies. Convert the recorded player poses to a stable clone-local coordinate system. During playback, apply pose deltas to supported rig targets with smoothing and safe null checks. Do not directly overwrite arbitrary bones until the target rig hierarchy and game update timing have been verified.

### 4. Track the held weapon

Use the target game's documented player/hand/item APIs to detect the item held in the right or left hand and record its actual transform. If there is no held item, record an explicit “no weapon” state rather than treating the hand as a weapon. During playback, the clone needs an actual held item or a clearly defined replay strategy; moving an invisible transform will not recreate weapon swings.

### 5. Hostile behavior, damage, and dismemberment

Use supported Nomad creature AI/faction and damage APIs found in the exact target assemblies or official SDK examples. Hostile mode must have a real, reversible effect and should not claim success if the NPC remains passive. Prefer letting the game's native weapon/damager system handle hits and dismemberment rather than implementing a parallel damage simulation. Test with safe, short recordings and one clone first.

### 6. Package and test

A distributable mod folder should contain at least:
- `manifest.json`;
- the compiled `EchoCloneMotionRecorder.Nomad.dll`;
- any additional files genuinely required by the mod.

Do not include game DLLs in the package. Zip the mod folder with the manifest and DLL at the expected folder level (avoid an extra nested directory). Test on the exact Nomad build in this order: mod loads; options appear; spawn works; recording starts/stops; playback moves clone; body retargeting; held weapon; hostile AI; damage; native dismemberment. Check the game's Player.log after any failure.

The game's in-headset Mod.io manager is the easiest install route once the mod is published for the correct Nomad game version. For private testing, manual installation may require Quest file access that is restricted on newer Android versions.

## Phone-only constraint

An iPhone can edit repository files and inspect GitHub Actions, but it cannot provide the game's private managed assemblies by itself. GitHub Actions can run the build only after those exact references are made available securely to the runner. Do not publish proprietary game assemblies in the public repository or expose them in public build artifacts. If the references cannot be obtained from a lawful, safe source, the plugin build and runtime verification remain blocked.

## Acceptance checklist

- [ ] Plugin compiles against exact target Nomad references.
- [ ] CI has a genuine plugin build job (not just source-token checks).
- [ ] Mod loads on Quest and exposes all actions.
- [ ] Spawn, record, stop, playback tested in-game.
- [ ] Full body and actual weapon replay tested.
- [ ] Hostile mode changes real AI behavior.
- [ ] Hits damage the player using native game systems.
- [ ] Dismemberment behavior tested.
- [ ] Final ZIP inspected and installed successfully.
- [ ] Release notes state exact supported game version and remaining limitations.
