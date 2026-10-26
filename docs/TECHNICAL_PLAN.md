# EchoClone Motion Recorder — Technical Plan

## Target

- Game: Blade & Sorcery: Nomad
- Target reported by the user: Nomad `1.3.2.261012003` on Quest 3S
- Headset: Meta Quest 3S

## Planned features

- In-game controls: Spawn EchoClone, Record, Stop Recording, Playback, and Hostile Mode On/Off.
- Record head, hand/controller, and weapon movement.
- Spawn a vanilla human NPC and replay recorded motion.
- Let the clone attack and damage the player.
- Use the game's native dismemberment system.

## Implementation stages

1. **Game-independent motion core (partially complete)** — store timestamped head, left-hand, right-hand, and weapon poses; validate timestamps and finite values; return isolated frame snapshots; calculate duration relative to the first sample; interpolate poses; and advance playback with optional looping. Automated core checks cover these behaviors.
2. **SDK verification** — identify the exact supported Nomad SDK/reference assemblies and verify the scripting lifecycle and options API.
3. **Recording** — sample XR poses at a stable rate, define coordinate-space conversions, and add durable save/load recordings.
4. **NPC spawning** — spawn a vanilla human NPC through the supported API.
5. **Playback/retargeting** — connect the tested game-independent playback clock to supported animation or ragdoll APIs and map recorded poses to the NPC rig.
6. **Combat** — implement a real hostile toggle and verify attacks cause normal game damage.
7. **Dismemberment** — verify the spawned creature supports native slicing.
8. **UI and headset testing** — add the five controls, test each feature on Quest, and package for Nomad.

## Important limitations

This repository is not yet a playable mod. The game-independent recorder core does not access XR tracking, spawn NPCs, drive a humanoid rig, add a book page, enable combat, or implement dismemberment. The core has not yet been compiled against the Nomad SDK or tested in-game.

The exact API depends on the installed game version and its matching SDK. Do not guess method names or ship placeholder calls as working game integration. Do not commit proprietary game DLLs or reference assemblies to a public repository.

A PCVR/desktop mod build must not be assumed to work on Nomad. Build and package specifically for the Nomad target.

## Acceptance checklist

- [ ] Compiles against the matching Nomad SDK.
- [ ] Loads on Quest 3S without errors.
- [ ] All five controls appear and work.
- [ ] Recording can be stopped and replayed.
- [ ] A vanilla human clone spawns.
- [ ] Playback moves the clone safely.
- [ ] Hostile mode can be toggled and attacks damage the player.
- [ ] Native dismemberment works on the clone.
- [ ] Package installs through the intended Nomad mod installation path.
