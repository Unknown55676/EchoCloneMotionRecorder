# Research and completion status (2026-10-26)

This document records the technical state of EchoClone Motion Recorder and the verified steps needed to finish it. It separates source checks from a real game build and headset validation.

## Current repository state

Repository: https://github.com/Unknown55676/EchoCloneMotionRecorder

The Nomad integration attempts to spawn the vanilla `HumanMale` creature, record player head/hand transforms, and move the spawned creature root along the recorded head path. A recent prototype update also caches the spawned creature's Unity humanoid Head/LeftHand/RightHand bones and applies recorded poses to them during playback, if those bones are available. Sample timestamps now use an accumulated recording clock.

The bone retargeting is experimental, not confirmed gameplay functionality: Nomad's animation/ragdoll update order may overwrite the transform changes, and the plugin has not been compiled against the exact target game assemblies.

Not implemented yet:
- detecting and recording the actual held weapon transform and replaying weapon motion;
- making the clone hostile using supported creature/faction/brain APIs;
- reliably applying damage to the player during replay;
- confirming native dismemberment interactions;
- compiling the game-side project against the exact Nomad 1.3.2.261012003 managed assemblies;
- launching and testing the DLL in Quest.

The source-validation workflow passing is not proof of plugin compatibility. It builds/tests only the game-independent core and checks that integration source contains expected tokens.

## Verified documentation

- [ThunderScript guide](https://kospy.github.io/BasSDK/Components/Guides/ThunderScript/): documents `ScriptLoaded(ModManager.ModData)` and the ThunderScript lifecycle.
- [ModOptions guide](https://kospy.github.io/BasSDK/Components/Guides/ModOptions/): documents static method/field options, button attributes, option value sources, and categories.
- [JSON modding guide](https://kospy.github.io/BasSDK/Components/Guides/SDK-HowTo/JSONModding.html): explains the game's JSON/catalog approach and mod structure.
- [Nomad installation guide](https://kospy.github.io/BasSDK/Components/Guides/HowToInstallMods.html): describes the in-game Mod.io manager and manual Android installation.
- [Nomad-specific FAQ](https://kospy.github.io/BasSDK/Components/Guides/FAQ/AndroidFAQ.html): covers Nomad-specific constraints; do not substitute PCVR assemblies.
- [Creature reference](https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Creature.html): describes creature/animator data.
- [Ragdoll part reference](https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/RagdollPart.html): documents limb setup and the game's native slice/dismemberment-related configuration.

These sources support general architecture but do not verify every API signature in this repository against the exact game build.

## Required completion sequence

### 1. Obtain exact target references and compile

The plugin project expects `ThunderRoad.dll` and `UnityEngine.CoreModule.dll` from the target Nomad game's managed assemblies. Keep proprietary DLLs out of the public repository.

Build command:

```sh
dotnet build src/EchoCloneMotionRecorder.Nomad/EchoCloneMotionRecorder.Nomad.csproj -c Release
```

Resolve compile errors against the actual target references; do not silence errors by guessing method signatures or substituting PCVR DLLs.

### 2. Validate body retargeting in Quest

Confirm the vanilla NPC has a humanoid Animator and the expected Head/LeftHand/RightHand bones. Test whether setting those transforms in `ScriptUpdate` survives the game's animation and ragdoll update order. If the animator or ragdoll overwrites them, use a supported post-animation hook or drive the appropriate ragdoll/animation targets instead of forcing transforms.

### 3. Track the held weapon

Use the target game's documented player/hand/item APIs to detect the item held in either hand and record its actual transform. If there is no held item, record an explicit “no weapon” state rather than treating the hand as a weapon. During playback, the clone needs a real held item or a defined replay strategy; moving an invisible transform will not recreate weapon swings.

### 4. Hostile behavior, damage, and dismemberment

Use supported Nomad creature AI/faction and damage APIs found in the exact target assemblies or official SDK examples. Hostile mode must have a real, reversible effect. Prefer the game's native weapon/damager system for hits and dismemberment rather than implementing a parallel damage simulation. Test with short recordings and one clone first.

### 5. Package and test

A distributable mod folder should contain at least:
- `manifest.json`;
- the compiled `EchoCloneMotionRecorder.Nomad.dll`;
- any additional files genuinely required by the mod.

Do not include game DLLs. Zip the mod folder with the manifest and DLL at the expected folder level (avoid an extra nested directory). Test on the exact Nomad build in this order: mod loads; options appear; spawn works; recording starts/stops; playback moves clone; body retargeting; held weapon; hostile AI; damage; native dismemberment. Inspect the game's Player.log after failures.

The in-headset Mod.io manager is the easiest install route once the mod is published for the correct Nomad game version. Private manual testing may require Quest file access that is restricted on newer Android versions.

## Phone-only constraint

An iPhone can edit repository files and inspect GitHub Actions, but it cannot provide the game's private managed assemblies by itself. GitHub Actions can run the build only after exact references are made available securely to the runner. Do not publish proprietary game assemblies in the public repository or expose them in public build artifacts. If the references cannot be obtained from a lawful, safe source, plugin compilation and runtime verification remain blocked.

## Acceptance checklist

- [ ] Plugin compiles against exact target Nomad references.
- [ ] CI has a genuine plugin build job (not just source-token checks).
- [ ] Mod loads on Quest and exposes all actions.
- [ ] Spawn, record, stop, playback tested in-game.
- [ ] Head/hand retargeting verified after the animation/ragdoll update.
- [ ] Actual held weapon recording/replay tested.
- [ ] Hostile mode changes real AI behavior.
- [ ] Hits damage the player using native game systems.
- [ ] Dismemberment behavior tested.
- [ ] Final ZIP inspected and installed successfully.
- [ ] Release notes state exact supported game version and remaining limitations.
