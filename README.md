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

The repository contains a timestamped pose data model, game-independent recording/playback sampling logic, a technical plan, SDK integration notes, and a GitHub Actions workflow. The workflow now builds the core and runs dependency-free behavior checks.

The current source does not read Quest tracking, spawn NPCs, drive a humanoid rig, add the in-game book controls, enable hostile AI, apply damage, or verify dismemberment. Those features require integration with the exact SDK and APIs for the installed Nomad version.

## Build requirements

The October 2026 1.3.2 update notes include an SDK assembly-list change adding `Unity.XR.OpenXR`. A game plugin must use matching references and be built for Nomad/Android, not merely for PCVR. A generic .NET build alone is not enough. Do not commit proprietary game DLLs or reference assemblies to this public repository.

## Project docs

- [Technical plan](docs/TECHNICAL_PLAN.md)
- [Quest 3S + iPhone workflow](docs/QUEST_ONLY_WORKFLOW.md)
- [SDK integration notes](docs/SDK_INTEGRATION_NOTES.md)
- [Remote build and Quest connection gates](docs/REMOTE_BUILD_AND_DEVICE_CONNECTION.md)

## Automated checks

GitHub Actions checks required files, builds the game-independent recorder core, and runs basic behavior checks for recording, sampling, validation, and clearing. See [Source validation workflow](.github/workflows/source-check.yml). A green result is not proof that the mod loads or works in-game.
