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

## Create a ZIP after a successful build

From the repository root in PowerShell, run:

```powershell
./tools/package-mod.ps1
```

The script checks the manifest and compiled plugin, then creates `artifacts/EchoCloneMotionRecorder-Nomad.zip` with the manifest and plugin DLL at the ZIP root. It deliberately refuses to package the game's proprietary DLLs. It cannot create a valid plugin DLL if the game references are missing, and the resulting ZIP is not considered verified until installed and tested in the target game.

## Current prototype

The ThunderScript attempts to spawn a vanilla `HumanMale` NPC, record player head/hand transforms, and replay the recorded head path by moving the NPC root. It now also attempts best-effort retargeting to the spawned Unity humanoid Animator's Head, LeftHand, and RightHand bones when available. Recording timestamps use an accumulated monotonic recording clock.

The bone transforms are set during the ThunderScript update. The game's animation and ragdoll update order may overwrite them or cause poor physics; this is experimental until tested on the exact Nomad build.

## Remaining major features

- The weapon pose is still a right-hand placeholder; actual held-item detection and weapon replay are not implemented.
- Hostile Mode still only toggles a flag; faction/AI behavior is not connected.
- Reliable player damage and native dismemberment are not implemented or verified.
- The exact target game assemblies have not been used to compile this source, and no in-game Quest test has been completed.

Do not package or publish this prototype as a completed mod until the game-side project builds and it has been tested in Nomad.
