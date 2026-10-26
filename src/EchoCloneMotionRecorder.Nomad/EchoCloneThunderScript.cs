using ThunderRoad;

namespace EchoCloneMotionRecorder.Nomad
{
    /// <summary>
    /// Minimal game-side entry point for the Nomad integration.
    /// This deliberately does not pretend to implement recording or clone control yet.
    /// Build it only against the exact ThunderRoad/Unity references for the target game.
    /// </summary>
    public sealed class EchoCloneThunderScript : ThunderScript
    {
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            // Keep startup side effects minimal until the target API references are supplied.
            // Next integration step: register/verify ModOptions and connect the core recorder.
        }

        public override void ScriptUpdate()
        {
            // Per-frame sampling will be added after the target game's player-pose API
            // is compiled and verified against the matching Nomad assemblies.
        }
    }
}
