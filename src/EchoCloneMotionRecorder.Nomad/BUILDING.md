# Building the Nomad plugin

## Phone-only/cloud build

The default project configuration can now compile without private game DLLs. It uses small build-only reference contracts for the Unity/ThunderRoad types that must appear in the plugin's metadata, and the gameplay code resolves player, catalog, and creature APIs reflectively at runtime.

From the repository root:

```sh
dotnet build src/EchoCloneMotionRecorder.Nomad/EchoCloneMotionRecorder.Nomad.csproj -c Release -p:UseNomadApiContracts=true
```

Those contract assemblies live under `build/contracts/`. They are **compiler-only**; the package script does not ship them. The contracts describe only the small API surface needed to compile and are not a substitute for the official, exact-version Nomad scripting references. This removes the build-time dependency blocker, but it does not prove that the resulting plugin can be loaded or that every runtime API is compatible.

The `Nomad cloud build` GitHub Actions workflow builds the motion core and plugin, packages a test candidate, and publishes it as the `EchoCloneMotionRecorder-Nomad-candidate` artifact. The candidate is not Quest-verified.

## Build against supplied game references

If a matching reference set is legitimately available, use:

```sh
dotnet build src/EchoCloneMotionRecorder.Nomad/EchoCloneMotionRecorder.Nomad.csproj -c Release -p:UseNomadApiContracts=false -p:NomadManagedDir=/path/to/managed
```

The directory must contain `ThunderRoad.dll` and `UnityEngine.CoreModule.dll`. Do not assume PCVR assemblies are equivalent to the target Nomad scripting runtime, and do not commit or distribute proprietary game DLLs.

## Package after a successful build

From the repository root in PowerShell:

```powershell
./tools/package-mod.ps1 -BuildConfiguration Release -ReferenceMode Contract
```

Use `-ReferenceMode Exact` only when the plugin really was built against supplied game references.

The script validates the manifest and includes:
- `manifest.json`
- `EchoCloneMotionRecorder.Nomad.dll`
- `EchoCloneMotionRecorder.dll` (the recorder-core dependency)
- `BUILD_STATUS.txt`

The script refuses to package either build-only contract DLL or proprietary game DLLs. It does not certify runtime compatibility.

## Current prototype

The ThunderScript attempts to spawn a vanilla `HumanMale` NPC, record player head and hand/controller transforms, and replay the recorded head path by moving the NPC root. It also makes a best-effort attempt to retarget the spawned humanoid's Head, LeftHand, and RightHand bones during playback. Runtime access to player, catalog, creature-data, and animator members is reflective to reduce compile-time binding to unverified implementation details.

The bone transforms are set during the ThunderScript update. The game's animation and ragdoll update order may overwrite them or cause poor physics; this remains experimental until tested on the exact Nomad build.

## Remaining major features

- The weapon pose is still a right-hand placeholder; actual held-item detection and weapon replay are not implemented.
- Hostile Mode still only toggles a flag; faction/AI behavior is not connected.
- Reliable player damage and native dismemberment are not implemented or verified.
- The exact target game's runtime behavior has not been validated on Quest.

Do not publish the candidate as a completed or Quest-tested mod until it has been installed and tested in Nomad.
