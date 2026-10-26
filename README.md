# EchoClone Motion Recorder

A work-in-progress Blade & Sorcery: Nomad mod project for Meta Quest 3S.

**Target reported by the user:** Nomad `1.3.2.261012003`.

## Planned features

- In-game controls: Spawn EchoClone, Record, Stop Recording, Playback, and Hostile Mode On/Off.
- Record head, hand/controller, and weapon motion.
- Spawn a vanilla human NPC and replay recorded motion.
- Let the clone attack and damage the player.
- Support the game's native dismemberment system.

## Current status

**The first gameplay integration is now written, but it is not yet a verified installable mod.**

The Nomad ThunderScript now has ModOptions actions to:
- spawn a vanilla `HumanMale` creature using the game's creature catalog,
- record the player's head and both hand transforms at roughly 20 samples per second,
- stop and keep a recording,
- play the recording by replaying the recorded head path as movement of the spawned creature root,
- toggle a logged prototype hostile-mode flag.

Important limitations:
- The recorded weapon pose is currently a placeholder copied from the right-hand pose.
- Playback moves the NPC root along the recorded head path. It does **not** retarget the NPC's head/arms/hands to the recorded poses, and it does not replay weapon handling.
- Hostile Mode is only a flag; faction changes, AI, damage behavior, and native dismemberment have not been implemented.
- The plugin has not yet been compiled against the target Nomad assemblies or tested on Quest.

The source and [build instructions](src/EchoCloneMotionRecorder.Nomad/BUILDING.md) require the exact game's reference assemblies. Do not commit proprietary game DLLs or reference assemblies to this public repository.

## Build requirements

A real Nomad plugin must use matching ThunderRoad and Unity reference assemblies. A generic .NET build of the recorder core is not proof that the plugin compiles or loads. Do not package or publish the plugin as a completed mod until the game-side project builds and it is tested in Nomad.

## Project docs

- [Technical plan](docs/TECHNICAL_PLAN.md)
- [Quest 3S + iPhone workflow](docs/QUEST_ONLY_WORKFLOW.md)
- [SDK integration notes](docs/SDK_INTEGRATION_NOTES.md)
- [Remote build and Quest connection gates](docs/REMOTE_BUILD_AND_DEVICE_CONNECTION.md)
- [Nomad plugin scaffold](src/EchoCloneMotionRecorder.Nomad/README.md)
- [Plugin build instructions](src/EchoCloneMotionRecorder.Nomad/BUILDING.md)

## Automated checks

GitHub Actions checks required files, parses project XML and manifest JSON, builds the game-independent recorder core, and runs its behavior checks. A green result is not proof that the game plugin compiles or works in-game.
