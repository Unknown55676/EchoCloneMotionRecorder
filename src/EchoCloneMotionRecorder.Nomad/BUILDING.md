# Building the Nomad plugin

## Required references

Use reference assemblies from the exact target installation of Blade & Sorcery: Nomad. Do not substitute PCVR assemblies and do not commit proprietary game DLLs to this public repository.

Set `NOMAD_GAME_MANAGED_DIR` to the directory containing at least:

- `ThunderRoad.dll`
- `UnityEngine.CoreModule.dll`

Then build with the .NET SDK:

```sh
dotnet build src/EchoCloneMotionRecorder.Nomad/EchoCloneMotionRecorder.Nomad.csproj -c Release
```

The project intentionally fails with a clear error when those references are missing. A successful build only proves that the source compiles against the supplied references; it does not prove it loads or behaves correctly on Quest.

## Implemented gameplay prototype

The current ThunderScript adds ModOptions actions to spawn a vanilla `HumanMale` NPC, record player head/hand transforms, stop recording, and replay the recorded head path by moving the NPC root. It also includes a placeholder hostile-mode flag.

## Known gaps

- The right-hand pose is currently used as a placeholder for the weapon pose.
- NPC head/arms/hands are not retargeted from the recording.
- Weapon replay, hostile faction/AI, player damage, and dismemberment are not implemented.
- The exact target game assemblies have not been used to compile this source, and no in-game Quest test has been completed.

Do not package or publish this prototype as a completed mod until the game-side project builds and it has been tested in Nomad.
