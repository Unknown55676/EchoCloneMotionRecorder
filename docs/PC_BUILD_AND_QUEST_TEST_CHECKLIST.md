# PC build and Quest test checklist

This checklist is for finishing EchoClone Motion Recorder for Blade & Sorcery: Nomad **1.3.2.261012003**. The current repository is still a prototype: the game plugin has not been compiled against the exact target assemblies, and weapon replay, real hostile AI, damage, and verified native dismemberment are not finished.

## 1. Prepare the Windows PC

Install:
- .NET 8 SDK
- Git for Windows (or download the repository ZIP)
- A trusted Quest file-access tool such as SideQuest, if needed to inspect files on the headset

Get the repository:
```powershell
git clone https://github.com/Unknown55676/EchoCloneMotionRecorder.git
cd EchoCloneMotionRecorder
```

## 2. Locate the exact Nomad assemblies

The plugin requires `ThunderRoad.dll` and `UnityEngine.CoreModule.dll` from the **same target Nomad build**. Nomad is an Android/Quest game; do not use PCVR assemblies or DLLs from a different version. Depending on how the game is packaged, these files may not be directly visible in the ordinary Quest user-data folder. Do not download random DLLs or publish proprietary game files to GitHub.

If you cannot find these files in a legitimate local game installation/extraction, stop here and share the folder/file listing (filenames only) so the next step can be chosen safely. Do not upload the game DLLs to the public repository.

Once you have a directory that genuinely contains both files, set the path in PowerShell (replace the example path):
```powershell
$env:NOMAD_GAME_MANAGED_DIR = "C:\path\to\verified\Nomad\Managed"
Test-Path "$env:NOMAD_GAME_MANAGED_DIR\ThunderRoad.dll"
Test-Path "$env:NOMAD_GAME_MANAGED_DIR\UnityEngine.CoreModule.dll"
```
Both checks must return `True`.

## 3. Compile and keep the full error log

From the repository root:
```powershell
dotnet --info
dotnet build src/EchoCloneMotionRecorder.Nomad/EchoCloneMotionRecorder.Nomad.csproj -c Release 2>&1 | Tee-Object -FilePath nomad-build.log
```

If the build fails, share the text from `nomad-build.log` (or the compiler error section). Do **not** share the game DLLs. Compile errors should be fixed against the exact references rather than by guessing API names.

## 4. Package only after a successful build

```powershell
./tools/package-mod.ps1
Get-FileHash ./artifacts/EchoCloneMotionRecorder-Nomad.zip -Algorithm SHA256
```

The ZIP should contain `manifest.json` and `EchoCloneMotionRecorder.Nomad.dll` at its root. This only proves the archive was created; it does not prove the mod works in-game.

## 5. Test in Nomad, one feature at a time

Use a safe Sandbox session and start with no other script-heavy mods enabled. Record the exact game version and keep a copy of the game log after each run.

1. Confirm the mod loads and the EchoClone options appear.
2. Spawn one clone; confirm it appears in front of the player.
3. Record a short head/hand movement, stop, and play it back.
4. Check whether the clone's animation/ragdoll overrides the experimental bone retargeting.
5. Test held-weapon detection/replay only after the exact hand/item API is confirmed in the target assemblies.
6. Test hostile AI with a reversible faction/brain change and confirm it can be disabled.
7. Test damage using the game's supported damage/damager pipeline.
8. Verify dismemberment with a normal supported weapon. Do not claim it works merely because the clone moves or collides.

If a step fails, capture the first relevant error and the game's version. Do not publish the ZIP as a completed mod until these checks pass.

## Current known limits

- Current weapon pose is only a right-hand placeholder.
- Hostile Mode currently changes a boolean and logs; it does not change the creature's AI/faction.
- The plugin currently does not apply verified damage or implement/verify dismemberment.
- Head/hand bone retargeting is experimental and can be overwritten by the game's animation/physics update.
- Source CI checks the core library and source structure; it does not compile the Nomad plugin.
