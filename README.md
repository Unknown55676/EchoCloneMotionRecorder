# EchoClone Motion Recorder

A work-in-progress Blade & Sorcery: Nomad mod project for Meta Quest 3S.

## Planned features

- In-game controls: Spawn EchoClone, Record, Stop Recording, Playback, and Hostile Mode On/Off.
- Record head, hand/controller, and weapon motion.
- Spawn a vanilla human NPC and replay recorded motion.
- Let the clone attack and damage the player.
- Support the game's native dismemberment system.

## Current status

**Core recording logic is in progress; this is not a playable mod yet.**

The repository contains a technical plan, a timestamped pose data model, game-independent recording/playback sampling logic, and a GitHub Actions workflow that builds the game-independent core. The automated .NET build does not compile or validate a Blade & Sorcery plugin.

The current source does not read Quest tracking, spawn NPCs, drive a humanoid rig, add the in-game book controls, enable hostile AI, apply damage, or verify dismemberment. Those features require integration with the exact SDK and APIs for the installed Nomad version.

## Build requirements

A working in-game mod needs the matching Nomad SDK/reference assemblies and a compatible Unity/BasSDK build setup. A generic .NET build alone is not enough. Do not commit proprietary game DLLs or reference assemblies to this public repository.

## Project docs

- [Technical plan](docs/TECHNICAL_PLAN.md)
- [Quest 3S + iPhone workflow](docs/QUEST_ONLY_WORKFLOW.md)
- [SDK integration notes](docs/SDK_INTEGRATION_NOTES.md)

## Automated check

GitHub Actions checks the repository files and builds the game-independent recorder core. See [Source validation workflow](.github/workflows/source-check.yml). A green result is not proof that the mod works in-game.
