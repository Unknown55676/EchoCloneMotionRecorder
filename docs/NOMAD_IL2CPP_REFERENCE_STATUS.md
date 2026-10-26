# Nomad IL2CPP scripting references — current build approach

**Target:** Blade & Sorcery: Nomad on Quest 3S, user-reported game build `1.3.2.261012003`.

## What changed

The previous workflow stopped before compiling the gameplay plugin unless private game references were supplied. That has been changed.

The default build now uses build-only contract projects under `build/contracts/` for the small `UnityEngine.CoreModule` and `ThunderRoad` metadata surface used directly by the plugin. The plugin uses reflection to discover player tracking, catalog, creature-spawn, and Animator bone APIs at runtime instead of requiring compile-time references to every one of those members. The contract assemblies are reference-only build inputs and must never be included in the mod ZIP.

The cloud workflow now attempts to compile the game-specific plugin and package a test candidate without requiring the user to provide a PC or private game DLL files. It can optionally use exact DLLs when configured as repository secrets.

## Important limitation

These source-controlled contracts are a **build unblocker**, not official, version-matched SDK assemblies. They only guarantee that the C# code can be compiled against the signatures defined in those contracts. They do not prove that the exact Nomad runtime will accept the plugin, that the script bridge resolves every type/member, or that spawn/playback succeeds. The candidate ZIP includes `BUILD_STATUS.txt` to make that status explicit.

Do not label the candidate as Quest-compatible or a completed release until the target game has loaded and tested it. Do not package or redistribute proprietary game DLLs, and do not substitute PCVR implementation assemblies while claiming they are exact Nomad references.

## Public leads checked on 2026-10-26

- SteamDB lists an `il2cpp` branch described as **“Il2CPP build (Nomad scripting)”** for the PCVR Steam app, depot/build `22364244`. The branch listing is useful evidence of a distinct Nomad-scripting build, not a public download of a version-matched reference pack.
  - https://steamdb.info/app/629730/depots/
  - https://steamdb.info/patchnotes/22364244/
- The old public BasSDK GitHub repository says it migrated to WarpFrog's Azure DevOps repository.
  - https://github.com/KospY/BasSDK
  - https://dev.azure.com/Warpfrog/BasSDK/_git/BasSDK
- A recent mod.io listing for “Nomad Cybernetics: Regen & Hex Shield” describes an Android DLL compiled against the “current public SDK API contract,” but does not identify a downloadable reference package and explicitly had not yet been tested on a physical Quest.
  - https://mod.io/g/blade-and-sorcery/m/nomad-cybernetics-regen-hex-shield

## Remaining implementation status

Human spawning, head/hand recording, root-motion playback, and experimental humanoid-bone retargeting remain source-level prototypes that require Quest validation. Actual held-weapon replay, real hostile AI, reliable player damage, and native dismemberment have not been implemented. The cloud candidate build now exists as a path; it is not an in-game-verified mod.
