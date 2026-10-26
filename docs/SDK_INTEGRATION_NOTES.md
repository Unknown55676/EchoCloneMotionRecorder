# SDK integration notes

Target game reported by the user: **Blade & Sorcery: Nomad 1.3.2.261012003** on Quest 3S.

## Current build approach

The original plugin project failed without game reference DLLs, so a cloud-build path has been added.

- The default cloud build uses **build-only contracts** under `build/contracts/` to compile the small set of Unity and ThunderRoad types referenced directly by the C# plugin.
- The gameplay plugin now resolves player tracking, catalog, creature spawning, and Animator bone members reflectively to reduce compile-time coupling to signatures that have not been confirmed for this exact runtime.
- The contracts are not official/version-matched game assemblies. They are compiler inputs only and are never copied into the mod ZIP.
- GitHub Actions now builds the core, compiles the plugin candidate, packages the plugin and core dependency, and uploads an artifact when all steps pass.
- If matching references are supplied securely as Actions secrets, the workflow can instead attempt a compile against those assemblies.

**Important:** a successful compile against the local contracts proves only that the C# source is syntactically/type consistent with those contracts. It does not prove that the target Quest runtime will load the DLL or that reflected methods, option attributes, spawning, animation, or physics work. The downloadable package is a test candidate until it is installed and exercised on the exact Nomad game build.

## SDK references checked

- ThunderScript guide: https://kospy.github.io/BasSDK/Components/Guides/ThunderScript/
- ModOptions guide: https://kospy.github.io/BasSDK/Components/Guides/ModOptions/
- JSON modding guide: https://kospy.github.io/BasSDK/Components/Guides/SDK-HowTo/JSONModding.html
- Nomad installation guide: https://kospy.github.io/BasSDK/Components/Guides/HowToInstallMods.html
- Nomad-specific FAQ: https://kospy.github.io/BasSDK/Components/Guides/FAQ/AndroidFAQ.html
- Current patch note (14 October 2026) says the SDK game-assemblies list added `Unity.XR.OpenXR`: https://steamdb.info/patchnotes/25901209/
- Khar-Tib SDK notes describe the script bridge for custom Catalog Editor scripts and automatic DLL export: https://steamdb.info/patchnotes/23943016/

The old public BasSDK GitHub repository says it migrated to WarpFrog's Azure DevOps repository: https://github.com/KospY/BasSDK and https://dev.azure.com/Warpfrog/BasSDK/_git/BasSDK. A version-matched reference pack for the exact Nomad build has not yet been verified.

## What the source currently attempts

- ModOptions actions for spawning a clone, recording, stopping, playback, and toggling the prototype hostile flag.
- A vanilla human catalog lookup and asynchronous spawn attempt.
- Head and both hand/controller pose capture at about 20 samples per second.
- Motion playback by moving the clone root based on the recorded head path.
- Best-effort reflective lookup and retargeting of humanoid head and hand bones.

Public source examples support catalog lookup plus `CreatureData.SpawnAsync`, but don't prove compatibility with this exact Nomad runtime:
- https://github.com/lyneca/dire-wand/blob/main/Modules/Polymorph.cs
- https://github.com/Wully616/ThunderRoadMCP/blob/main/docs/game-guide.md

## Still unfinished or unverified

- Actual held-item detection and weapon motion playback (the weapon sample remains a right-hand placeholder).
- Real hostile faction/AI behavior.
- Reliable player damage during playback.
- Verified native dismemberment.
- Whether manually applying bone transforms survives Nomad's animation/ragdoll update order.
- Actual loading/testing on a physical Quest with the exact game build.

The cloud build is a step forward: it can compile a candidate without making you obtain proprietary DLLs. It is not a replacement for the target game runtime, and it must not be described as a completed Quest-tested mod until it passes in-game validation.
