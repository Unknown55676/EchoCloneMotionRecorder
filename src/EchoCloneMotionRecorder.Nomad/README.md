# EchoClone Motion Recorder — Nomad integration

This folder contains an experimental gameplay-side prototype.

Current code attempts to:
- spawn the vanilla `HumanMale` creature;
- record player head and both hand transforms;
- stop and retain the recording;
- replay the recorded head path by moving the creature root;
- cache the spawned creature's Unity humanoid Head/LeftHand/RightHand bones and apply recorded poses to available bones during playback;
- use a monotonic recording clock rather than deriving sample times from the current frame-list duration.

The humanoid bone retargeting is best-effort and **not yet verified in Nomad**. The game's Animator/ragdoll update order may override the transforms or cause unwanted physics behavior, so it must be tested on the target build before treating it as functional.

Still not implemented:
- detecting and recording the actual held weapon transform;
- replaying weapon swings with a real held item;
- making Hostile Mode change the NPC's faction/AI behavior;
- guaranteed player damage during playback;
- verified native dismemberment behavior.

The plugin has not been compiled against the exact target game assemblies or tested in Quest. See [BUILDING.md](BUILDING.md) and [the research/completion report](../../docs/RESEARCH_AND_COMPLETION_STATUS.md).
