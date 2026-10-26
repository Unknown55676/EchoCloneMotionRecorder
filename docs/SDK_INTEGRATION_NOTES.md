# SDK integration notes

This note records verified documentation leads before writing game-dependent code. It is not a claim that the plugin has been implemented.

## Mod controls

The BasSDK ModOptions guide documents static C# members decorated with attributes such as `[ModOption]`. Supported types include booleans and methods; methods can be displayed as buttons using `[ModOptionButton]`. Options load from the compiled mod assembly, and the mod needs a normal `manifest.json`.

Official community documentation: https://kospy.github.io/BasSDK/Components/Guides/ModOptions/

This looks like a candidate for the first control prototype:
- Spawn EchoClone
- Start recording
- Stop recording
- Play recording
- Hostile mode toggle

Before implementing these methods, the project must reference the exact BasSDK assemblies matching the installed Nomad version. Controls must call real game integration methods, not placeholders.

## Creature and combat integration

The game documentation identifies `Creature` as a core component of NPCs, but that does not by itself document the safe runtime spawn call, exact vanilla creature ID, rig retargeting, damage handling, or dismemberment guarantees.

Creature reference: https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Creature.html

Those APIs and identifiers must be confirmed from the matching SDK/source examples before implementation. Do not guess an API name or assume a normal spawned NPC can be driven directly by replaying controller poses.

## Nomad packaging and installation

Nomad is the Android version of the game. The official community guide recommends installing published mods through the in-game Mods manager; manual file installation on recent Android versions is more difficult and generally requires a computer.

References:
- https://kospy.github.io/BasSDK/Components/Guides/HowToInstallMods.html
- https://kospy.github.io/BasSDK/Components/Guides/FAQ/AndroidFAQ.html

## Current implementation boundary

The repository's .NET project builds only the game-independent motion data/recording core. It is intentionally not a game plugin and does not reference Unity, ThunderRoad, XR tracking, creature spawning, or in-game UI APIs. A successful core build is only an early quality check.
