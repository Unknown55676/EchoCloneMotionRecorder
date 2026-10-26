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

**This is not a playable or installable mod yet.**

The repository contains a timestamped pose model for the head, left hand, right hand, and weapon; game-independent recording/playback sampling logic; validation and core behavior checks; and an initial Nomad `ThunderScript` prototype. The prototype now includes a startup log, two placeholder ModOptions, and a manifest template. Those options only log their selected values; they do not spawn or control a clone.

The scaffold is in [src/EchoCloneMotionRecorder.Nomad](src/EchoCloneMotionRecorder.Nomad/README.md). Its [build instructions](src/EchoCloneMotionRecorder.Nomad/BUILDING.md) explain how to supply the exact game's reference assemblies without committing proprietary DLLs to this public repository.

The plugin has **not** been compiled against the target game's assemblies or tested on Quest. The current source does not yet read Quest tracking, spawn NPCs, drive a humanoid rig, replay weapon handling, enable hostile AI, apply damage, or verify dismemberment.

## Build requirements

A real Nomad plugin must use matching ThunderRoad and Unity reference assemblies. A generic .NET build of the recorder core is not proof that the game plugin compiles or loads. Do not commit proprietary game DLLs or reference assemblies to this public repository.

## Project docs

- [Technical plan](docs/TECHNICAL_PLAN.md)
- [Quest 3S + iPhone workflow](docs/QUEST_ONLY_WORKFLOW.md)
- [SDK integration notes](docs/SDK_INTEGRATION_NOTES.md)
- [Remote build and Quest connection gates](docs/REMOTE_BUILD_AND_DEVICE_CONNECTION.md)
- [Nomad plugin scaffold](src/EchoCloneMotionRecorder.Nomad/README.md)
- [Plugin build instructions](src/EchoCloneMotionRecorder.Nomad/BUILDING.md)

## Automated checks

GitHub Actions checks required files, parses the project XML and manifest JSON, builds the game-independent recorder core, and runs its behavior checks. A green result is not proof that the plugin loads or works in-game.
