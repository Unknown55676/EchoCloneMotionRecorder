# EchoClone Motion Recorder

A work-in-progress Blade & Sorcery: Nomad mod concept for Meta Quest 3S.

## Planned features

- In-game controls: Spawn EchoClone, Record, Stop Recording, Playback, and Hostile Mode On/Off.
- Record head, controller/hand, and weapon motion.
- Spawn a vanilla human NPC and replay recorded motion.
- Let the clone attack and damage the player.
- Support the game's native dismemberment system.

## Current status

**Planning/data-model stage — not a playable mod yet.**

The repository now contains a technical plan, a game-independent motion frame data model, and a .gitignore. These files have not been compiled or tested in Blade & Sorcery: Nomad.

The motion data class does not yet read Quest tracking, spawn NPCs, drive a humanoid rig, add the in-game book controls, enable hostile AI, apply damage, or verify dismemberment. Those features require integration with the exact SDK and APIs for the installed Nomad version.

## Build requirements

A working build needs the matching Nomad SDK/reference assemblies and a compatible build setup. A generic .NET build alone is not enough. Do not commit proprietary game DLLs or reference assemblies to this public repository.

See [docs/TECHNICAL_PLAN.md](docs/TECHNICAL_PLAN.md) for implementation stages and acceptance criteria.

For the iPhone + Quest-only workflow and the requirements before the mod can be installed, see [docs/QUEST_ONLY_WORKFLOW.md](docs/QUEST_ONLY_WORKFLOW.md).
