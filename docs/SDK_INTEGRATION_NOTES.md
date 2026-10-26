# SDK integration notes

Target game confirmed by the user: **Blade & Sorcery: Nomad 1.3.2.261012003** on Quest 3S.

This note records verified documentation and public-source leads before writing game-dependent code. It is not a claim that the plugin has been implemented or tested on Quest.

**Important build constraint:** Nomad is IL2CPP. The project needs API references compatible with Nomad's scripting bridge, not merely the ordinary PCVR Managed assemblies. The public reference route for the exact target build is still unverified. See [Nomad IL2CPP reference status](NOMAD_IL2CPP_REFERENCE_STATUS.md).

## Version-specific SDK concern

The October 2026 1.3.2 update notes say the SDK assembly list was updated to include `Unity.XR.OpenXR`, to prevent mod loading issues. The matching SDK/reference set for the user's exact Nomad build must be used; do not assume an older SDK setup is compatible.

Update notes:
- https://gameupdatenotifier.com/g/blade-and-sorcery/v/1-3-2
- https://steamdb.info/patchnotes/25901209/

The game-independent .NET project in this repository is not a game plugin. It currently does not reference Unity, ThunderRoad, XR tracking, creature spawning, or in-game UI APIs.

## Mod controls

The BasSDK ModOptions guide documents static C# members decorated with attributes such as `[ModOption]`. Supported types include booleans and methods; methods can be displayed as buttons using `[ModOptionButton]`. Button methods must have a matching option-value source for the method parameter, and the mod must include a compiled DLL plus `manifest.json`.

Official community documentation: https://kospy.github.io/BasSDK/Components/Guides/ModOptions/

A public Blade & Sorcery mod source example also uses a `ThunderScript`, `[ModOption]`, `[ModOptionButton]`, and a `ModOptionString[]` value source:
- https://github.com/Kingo64/TheOuterRim/blob/master/GlobalSettings.cs

Candidate controls:
- Spawn EchoClone
- Start recording
- Stop recording
- Play recording
- Hostile mode toggle

These controls must call actual game integration methods, not placeholders.

## Vanilla creature spawning

Public Blade & Sorcery mod source and the ThunderRoadMCP game guide document the supported pattern as resolving a `CreatureData` catalog entry and calling `CreatureData.SpawnAsync`, rather than instantiating a raw prefab. The game guide documents the signature as:

```csharp
CreatureData.SpawnAsync(
    Vector3 position,
    float rotationY,
    Transform parent = null,
    bool pooled = true,
    WayPoint[] waypoints = null,
    Action<Creature> callback = null)
```

Sources:
- https://github.com/lyneca/dire-wand/blob/main/Modules/Polymorph.cs
- https://github.com/Wully616/ThunderRoadMCP/blob/main/docs/game-guide.md

A public Nomad game log shows the vanilla catalog IDs `HumanMale` and `HumanFemale` being pooled. This is evidence that the IDs exist in a game build, not a substitute for testing the user's exact installed build.

Reference:
- https://www.nexusmods.com/bladeandsorcerynomad/mods/4262?tab=posts

The spawn callback must be checked for null, and the clone must be tracked for cleanup. Spawn is only one part of the feature: it does not implement motion playback or hostile combat.

## Recording and rig playback

The public examples above do **not** establish a safe way to map recorded headset/controller/weapon poses onto a normal NPC's animated and physics-driven rig. This is a separate integration problem. Simply setting a creature root transform would not reproduce head, hands, weapon handling, attacks, or native dismemberment.

Additional source inspection has now verified these useful integration points in public mod code and the BasSDK guide:
- player head pose is accessed as `Player.local.head.transform`;
- hand poses are accessed as `Player.local.handLeft.transform` and `Player.local.handRight.transform` (some code uses `Player.local.creature.handLeft.transform` for creature-hand details);
- `ThunderScript` provides `ScriptUpdate`, `ScriptFixedUpdate`, and `ScriptLateUpdate`; the guide describes `ScriptLateUpdate` as running after animation/IK, which matters for pose retargeting;
- a held handle can be obtained in some contexts through `creature.equipment.GetHeldHandle(Side.Left/Right)`, but the precise item/weapon transform and handedness logic still needs a tested implementation;
- `Creature.ragdoll.SetState(Ragdoll.State.Destabilized)` is used by public mods, but it is not evidence that arbitrary recorded poses can safely drive a full ragdoll.

Sources:
- https://kospy.github.io/BasSDK/Components/Guides/ThunderScript/
- https://github.com/OWODevelopers/OWO_BladeAndSorcery/blob/fe917dd94f3b985317390fc00af47ca657719c12/OWO_BladeAndSorcery.cs
- https://github.com/Kingo64/TheOuterRim/blob/master/ItemBlaster.cs
- https://github.com/lyneca/dire-wand/blob/main/Modules/Push.cs

Still to confirm from a matching SDK build and actual in-game test:
- how to retarget the NPC's head, hands, and weapon without fighting the game's animator/physics;
- which weapon transform should be recorded and how to spawn/equip a matching weapon on the clone;
- AI faction/targeting and damage behavior;
- reliable cleanup when the level changes or the clone is killed/despawned.

Do not guess an API name or claim dismemberment works just because an NPC can spawn.

## Nomad packaging and installation

Nomad is the Android version of the game. The official community guide recommends installing published mods through the in-game Mods manager; manual file installation on recent Android versions is more difficult and generally requires a computer.

References:
- https://kospy.github.io/BasSDK/Components/Guides/HowToInstallMods.html
- https://kospy.github.io/BasSDK/Components/Guides/FAQ/AndroidFAQ.html

## Current implementation boundary

The repository's .NET project builds only the game-independent motion data/recording core. A successful core build is an early quality check only, not proof that the game mod loads or is playable. No game-specific plugin should be marked complete until it builds against the matching SDK/API contract and is tested in Nomad.
