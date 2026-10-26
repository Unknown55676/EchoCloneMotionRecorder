# Research and completion status (2026-10-26)

This document separates source implementation, cloud compilation, and physical Quest validation.

## Current repository state

Repository: https://github.com/Unknown55676/EchoCloneMotionRecorder

The gameplay plugin attempts to:
- expose ModOptions actions for spawning a clone, recording, stopping, playback, and a prototype hostile flag;
- resolve a vanilla `HumanMale` creature from the runtime catalog and call its spawn API;
- record the player's head and both hand/controller transforms;
- replay head movement by moving the clone's root;
- best-effort retarget the clone's head and hands if its Animator exposes matching humanoid bones.

Game API calls for player, catalog, creature spawning, and bone lookup use reflection to avoid compiling against unverified implementation signatures. Compile-only reference contracts under `build/contracts/` are a local build aid and are not included in output ZIP files.

## Build path now in repository

The previous cloud workflow stopped at the plugin build unless private game reference DLLs were provided. The current `Nomad cloud build` workflow instead:
1. builds and tests the game-independent motion core;
2. compiles the Nomad plugin against small build-only API contracts when exact references are not configured, or against optionally supplied exact references;
3. packages `manifest.json`, the plugin DLL, the `EchoCloneMotionRecorder.dll` core dependency, and a `BUILD_STATUS.txt`;
4. uploads `EchoCloneMotionRecorder-Nomad-candidate` as a GitHub Actions artifact.

The contract build does not embed or redistribute game DLLs. It only proves the source compiles against the contract surface. The plugin is **not** yet proven to load on Nomad; the artifact must be clearly treated as a test candidate.

## Current implementation limitations

Not implemented or not validated:
- recording the actual held weapon and replaying it in the clone's hand;
- hostile NPC faction/AI behavior;
- reliably damaging the player during playback;
- native dismemberment;
- whether humanoid bone retargeting fights the game Animator or ragdoll;
- successful plugin load and live gameplay on Quest 3S.

## Official/source references

- [ThunderScript guide](https://kospy.github.io/BasSDK/Components/Guides/ThunderScript/)
- [ModOptions guide](https://kospy.github.io/BasSDK/Components/Guides/ModOptions/)
- [JSON modding guide](https://kospy.github.io/BasSDK/Components/Guides/SDK-HowTo/JSONModding.html)
- [Nomad installation guide](https://kospy.github.io/BasSDK/Components/Guides/HowToInstallMods.html)
- [Nomad-specific FAQ](https://kospy.github.io/BasSDK/Components/Guides/FAQ/AndroidFAQ.html)
- [October 14, 2026 SDK update: `Unity.XR.OpenXR` assembly list addition](https://steamdb.info/patchnotes/25901209/)
- [Khar-Tib SDK notes: custom ScriptBridge and DLL export](https://steamdb.info/patchnotes/23943016/)

The exact public version-matched Nomad API reference bundle is still not verified. The cloud contracts are not a substitute for it and not proof of runtime compatibility.

## Validation checklist

- [ ] GitHub Actions reports a successful contract-mode plugin build.
- [ ] Artifact ZIP contains the manifest, plugin DLL, core DLL, and status note, with no contract or game DLLs.
- [ ] The mod loads in Nomad and exposes its controls.
- [ ] Clone spawn succeeds in-game.
- [ ] Recording starts/stops and playback moves the clone.
- [ ] Head/hand pose retargeting behaves correctly after animation/ragdoll updates.
- [ ] Actual held weapon can be recorded and replayed.
- [ ] Hostile mode produces a real AI/faction change.
- [ ] Recorded attacks damage the player using the native gameplay path.
- [ ] Native dismemberment behaves correctly.
- [ ] Final supported game version and limitations are documented.
