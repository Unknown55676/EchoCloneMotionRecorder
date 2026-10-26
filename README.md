# EchoClone Motion Recorder

A work-in-progress Blade & Sorcery: Nomad mod project for Meta Quest 3S.

**Target reported by the user:** Nomad `1.3.2.261012003`.

## Planned features

- In-game controls: Spawn EchoClone, Record, Stop Recording, Playback, and Hostile Mode.
- Record head, hand/controller, and weapon motion.
- Spawn a vanilla human NPC and replay recorded movement.
- Let the clone attack and damage the player.
- Use the game's native dismemberment system.

## Current status

The gameplay prototype and a cloud compile path are now in the repository. The default cloud build uses build-only contract assemblies instead of requiring private game DLLs, and the code resolves player tracking, catalog, creature spawning, and Animator bone APIs reflectively. That allows GitHub Actions to attempt a full plugin compile and make a candidate ZIP without a PC on the user's side.

**This is a build candidate, not a verified Quest release.** The small compile contracts are not official exact-version Nomad assemblies and are never included in the package; a successful compile does not establish that the game will load the plugin.

Implemented in source:
- ModOptions entry points for spawn, record, stop, playback, and a logged hostile-mode flag.
- Player head/hand pose sampling at about 20 samples per second.
- Motion recording/playback core with checks.
- Vanilla-human spawn attempt and root-motion playback.
- Experimental humanoid head/hand bone retargeting.

Remaining or not verified:
- Actual held-weapon recording and replay.
- Real hostile AI/faction changes.
- Reliable player damage.
- Native dismemberment.
- In-game compatibility and physics/animation interaction on the target Quest build.

## Build and artifact

GitHub Actions workflow: [Nomad cloud build](.github/workflows/nomad-cloud-build.yml)

On successful workflow completion, download the `EchoCloneMotionRecorder-Nomad-candidate` artifact from the repository's [Actions page](https://github.com/Unknown55676/EchoCloneMotionRecorder/actions). The package contains the mod manifest, plugin DLL, core dependency DLL, and a build-status note. Do not upload it as a verified release until it has been tested in Nomad.

## Project docs

- [Technical plan](docs/TECHNICAL_PLAN.md)
- [Quest 3S + iPhone workflow](docs/QUEST_ONLY_WORKFLOW.md)
- [SDK integration notes](docs/SDK_INTEGRATION_NOTES.md)
- [Nomad IL2CPP reference status](docs/NOMAD_IL2CPP_REFERENCE_STATUS.md)
- [Nomad plugin source notes](src/EchoCloneMotionRecorder.Nomad/README.md)
- [Plugin build instructions](src/EchoCloneMotionRecorder.Nomad/BUILDING.md)

GitHub Actions checks the motion core, source files, project XML, contracts, and the plugin compile candidate. A green build is not proof of in-game compatibility.
