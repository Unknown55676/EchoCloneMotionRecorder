# Quest 3S + iPhone workflow

## Current reality

The repository is connected to GitHub, but it is not yet a working Blade & Sorcery: Nomad mod. It currently contains planning documentation and a motion data model only. There is no compiled plugin, game manifest, NPC integration, or installable ZIP.

A Quest 3S and iPhone are enough to play Nomad and install already-published compatible mods through the in-game Mods menu. They are not, by themselves, a complete Unity/BasSDK development environment. The official community SDK workflow calls for Unity Editor and the matching Blade & Sorcery SDK, and Android/Nomad output must be built for the Android target.

## Safe path forward

1. Keep source code and planning in this GitHub repository.
2. Confirm the exact Blade & Sorcery: Nomad version installed on the Quest 3S.
3. Obtain a compatible desktop Unity environment with the matching BasSDK to compile and test the game-specific plugin. This can be the user's own computer or a trusted helper's computer; do not upload proprietary game DLLs or account credentials to public repositories.
4. Implement and compile the plugin against the matching SDK, then test it in Nomad. Do not label a source-only repository as a playable mod.
5. Package a valid Nomad mod folder with the required manifest and any compiled assets.
6. Once a valid package exists and is published for Nomad on Mod.io, install it from the game's in-headset Mods menu. Select the Nomad/Android version compatible with the installed game version.

## What can be done from an iPhone

- Read and edit source files in GitHub.
- Review commits and documentation.
- Once a genuine compatible mod is published, install it through the game's built-in Mods menu.
- Help test the mod in-headset and report visible errors or behavior.

## What cannot honestly be claimed yet

- The current repository cannot be installed into the game as-is.
- A ZIP containing only source files will not add the requested controls or clone behavior.
- No code has been compiled or tested in Nomad.
- The recording, NPC spawning, rig playback, hostile combat, damage, and native dismemberment features remain unimplemented.

## References

- [Blade & Sorcery SDK — Unity SDK Setup](https://kospy.github.io/BasSDK/Components/Guides/SDK-HowTo/UnitySDKSetup.html)
- [Blade & Sorcery SDK — Installing Mods](https://kospy.github.io/BasSDK/Components/Guides/HowToInstallMods.html)
- [Blade & Sorcery SDK — JSON Modding and Nomad mod folder](https://kospy.github.io/BasSDK/Components/Guides/SDK-HowTo/JSONModding.html)
