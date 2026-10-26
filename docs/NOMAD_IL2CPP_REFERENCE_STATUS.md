# Nomad IL2CPP scripting references — status

**Target:** Blade & Sorcery: Nomad on Quest 3S, user-reported game build `1.3.2.261012003`.

## What the cloud build needs

EchoClone's current game-side source derives from `ThunderScript` and uses `ThunderRoad` and Unity types. It cannot be meaningfully compiled by building only the game-independent motion library. A compiler needs API references that define those types.

For Nomad, do **not** assume that copying the ordinary PCVR `BladeAndSorcery_Data/Managed` assemblies produces a compatible Android plugin. Nomad uses IL2CPP and its scripted-mod support depends on a compatible scripting bridge/API contract. PCVR references might help autocomplete or prove generic C# syntax, but they do not establish that the resulting DLL will load on Quest.

## Public leads checked on 2026-10-26

- SteamDB lists an `il2cpp` branch described as **“Il2CPP build (Nomad scripting)”** for the PCVR Steam app, depot/build `22364244`. SteamDB's patch-notes page says that build had **not been seen in a public branch**. The listing is evidence that a Nomad-scripting build exists; it is not a download link to a public reference pack.
  - https://steamdb.info/app/629730/depots/
  - https://steamdb.info/patchnotes/22364244/
- The old public BasSDK GitHub repository explicitly says it migrated to WarpFrog's Azure DevOps repository and will no longer be updated. It contains SDK project source, not a clearly identified, ready-to-use reference bundle for the user's exact Nomad build.
  - https://github.com/KospY/BasSDK
  - https://dev.azure.com/Warpfrog/BasSDK/_git/BasSDK
- A recent mod.io listing, “Nomad Cybernetics: Regen & Hex Shield,” says its Android DLL was compiled against the “current public SDK API contract,” but the listing does not identify or link the exact reference package. It also says the build had not been tested on a physical Quest. This is a useful lead, **not proof that EchoClone can compile or run with it**.
  - https://mod.io/g/blade-and-sorcery/m/nomad-cybernetics-regen-hex-shield

## Build decision

At this time, no public, version-matched Nomad scripting reference package has been verified for the target build. Consequently:

1. Do not package PCVR-only references as a Quest-compatible build.
2. Do not create fake `ThunderRoad`/Unity stubs and treat a DLL that compiles against them as a real mod; such stubs cannot prove runtime type identity or API compatibility.
3. Keep the cloud workflow for core tests, but mark the game plugin as blocked until a legitimate public reference/API-contract package is identified or supplied.
4. After compiling, the resulting plugin still must be tested in the actual Quest game. A successful compile does not prove spawning, recording, weapon replay, hostile AI, player damage, or dismemberment works.

## Current feature status

The plugin remains a prototype. Human spawning, head/hand recording, root motion playback, and experimental humanoid-bone retargeting exist in source but have not been validated in Nomad. Actual held-weapon replay, real hostile AI, reliable player damage, and verified native dismemberment are not implemented. No verified installable ZIP is available yet.
