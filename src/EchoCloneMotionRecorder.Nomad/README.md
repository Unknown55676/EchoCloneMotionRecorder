# EchoClone Motion Recorder — Nomad integration

This folder contains the first gameplay-side prototype.

The ThunderScript currently attempts to spawn the vanilla `HumanMale` creature, record the player's head and hand transforms, and replay the recorded head path by moving the creature root. The ModOptions actions are declared in `EchoCloneThunderScript.cs`.

This is not full-body animation retargeting. The weapon pose is a right-hand placeholder, and hostile AI, damage, and dismemberment are not wired up.

See [BUILDING.md](BUILDING.md) for the exact-reference build gate. The plugin has not been compiled against the target game assemblies or tested in-game yet.
