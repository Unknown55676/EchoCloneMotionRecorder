# Building the Nomad plugin scaffold

This project is a real C# plugin scaffold, but it is not yet a finished gameplay mod.

## Required references

Use reference assemblies from the exact target installation of Blade & Sorcery: Nomad. Do not substitute PCVR assemblies and do not commit proprietary game DLLs to this public repository.

Set `NOMAD_GAME_MANAGED_DIR` to the directory containing at least:

- `ThunderRoad.dll`
- `UnityEngine.CoreModule.dll`

Then build with the .NET SDK:

```sh
dotnet build src/EchoCloneMotionRecorder.Nomad/EchoCloneMotionRecorder.Nomad.csproj -c Release
```

The project intentionally fails with a clear error when those references are missing. A successful build only proves that the scaffold compiles against the supplied references; it does not prove it loads on Quest.

## Current implementation boundary

The current ThunderScript is only an entry point. It does not yet register the requested buttons, read player poses, spawn a clone, retarget a humanoid rig, replay weapons, or configure hostile AI. Those pieces must be added against the matching APIs and tested in Nomad. Do not package or publish this scaffold as a completed mod.
