# Nomad plugin integration project

This folder is the integration boundary for the game plugin. It intentionally does not pretend the existing netstandard recorder library is itself a mod.

## Why the plugin build is not checked in yet

A valid Nomad plugin must compile against the exact installed game's ThunderRoad and Unity reference assemblies. The repository's GitHub runner does not have those assemblies, and proprietary game DLLs must not be copied into this public repository. The target is Nomad 1.3.2.261012003, so older desktop/PCVR references are not an acceptable substitute.

## Verified API starting points

- `ThunderScript` is the game script base class; public mods implement `ScriptLoaded(ModManager.ModData)` and `ScriptUpdate()`.
- The ModOptions API supports static `[ModOption]` members and `[ModOptionButton]` methods. A button method uses an option value source and accepts its selected value parameter.
- Player pose transforms commonly used by public mods are `Player.local.head.transform`, `Player.local.handLeft.transform` and `Player.local.handRight.transform`.
- Vanilla human spawning commonly uses `Catalog.GetData<CreatureData>("HumanMale")` and `CreatureData.SpawnAsync(position, rotationY, ..., callback)`.

These are starting points, not proof that the entire clone feature works. The critical unsolved feature is safe retargeting of a normal NPC's physics-driven humanoid rig from recorded head/hand/weapon motion. Root-transform playback is not equivalent to body motion playback and would not meet the request.

## Required implementation and acceptance gates

1. Compile a minimal `ThunderScript` against the matching Nomad references.
2. Verify it appears in the in-game Mods options and logs startup.
3. Implement and test clone spawn/despawn.
4. Implement real recording and durable recording data.
5. Implement safe full-body motion playback and held weapon playback.
6. Implement hostile mode and verify damage.
7. Test native dismemberment on the spawned creature.
8. Build a valid Nomad mod package and test installation on Quest 3S.

Do not publish this directory as a playable mod until those gates pass.