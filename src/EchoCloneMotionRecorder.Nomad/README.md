# EchoClone Motion Recorder — Nomad integration

This folder contains an experimental gameplay-side prototype for Blade & Sorcery: Nomad.

Current code attempts to:
- expose in-game actions for spawning a clone, recording, stopping, playback, and a prototype hostile-mode flag;
- spawn the vanilla `HumanMale` creature through the game's catalog APIs;
- record player head and both hand/controller transforms at roughly 20 samples per second;
- replay the recorded head path by moving the spawned creature root;
- best-effort retarget the clone's humanoid head and hand bones during playback.

Player tracking, catalog lookup, creature spawning, and Animator bone lookup use runtime reflection to minimize compile-time dependency on unverified game implementation signatures. The plugin still derives from `ThunderScript` and uses the documented ModOptions attributes.

## Build status

A cloud build can now compile a **test candidate** without downloading private game DLLs. It uses build-only contract assemblies located in `build/contracts/`; those assemblies are not included in the ZIP. This fixes the previous compile blocker but does **not** prove that the candidate loads or works on the exact Quest build. See [BUILDING.md](BUILDING.md) and [Nomad IL2CPP reference status](../../docs/NOMAD_IL2CPP_REFERENCE_STATUS.md).

The candidate ZIP contains both the plugin and its recorder-core dependency. The package also includes a `BUILD_STATUS.txt` warning that the build has not been verified in-game.

## Not implemented or not verified

- Actual held-weapon tracking and weapon swings replayed with an equipped item.
- Real hostile faction/AI behavior.
- Reliable damage to the player during clone playback.
- Native dismemberment.
- The effect of manually retargeting humanoid bones on Nomad's animation/ragdoll update order.

The code has not been tested on a physical Quest. A successful compile is not proof of runtime compatibility.
