# SDK integration notes

Target game confirmed by the user: **Blade & Sorcery: Nomad 1.3.2.261012003** on Quest 3S.

This note records verified documentation leads before writing game-dependent code. It is not a claim that the plugin has been implemented or tested on Quest.

## Version-specific SDK concern

The October 2026 1.3.2 update notes say the SDK assembly list was updated to include `Unity.XR.OpenXR`, to prevent mod loading issues. The matching SDK/reference set for the user's exact Nomad build must be used; do not assume an older SDK setup is compatible.

Update notes:
- https://gameupdatenotifier.com/g/blade-and-sorcery/v/1-3-2
- https://steamdb.info/patchnotes/25901209/

The game-independent .NET project in this repository is not a game plugin. It currently does not reference Unity, ThunderRoad, XR tracking, creature spawning, or in-game UI APIs.

## Mod controls

The BasSDK ModOptions guide documents static C# members decorated with attributes such as `[ModOption]`. Supported types include booleans and methods; methods can be displayed as buttons using `[ModOptionButton]`. Options load from the compiled mod assembly, and the mod needs a normal `manifest.json`.

Official community documentation: https://kospy.github.io/BasSDK/Components/Guides/ModOptions/

Candidate controls:
- Spawn EchoClone
- Start recording
- Stop recording
- Play recording
- Hostile mode toggle

These controls must call actual game integration methods, not placeholders.

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

The repository's .NET project builds only the game-independent motion data/recording core. A successful core build is an early quality check only, not proof that the game mod loads or is playable.
