using ThunderRoad;
using UnityEngine;

namespace EchoCloneMotionRecorder.Nomad
{
    /// <summary>
    /// Minimal game-side entry point for the Nomad integration.
    /// This prototype only verifies startup and exposes placeholder settings;
    /// it does not yet record motion or spawn/control a clone.
    /// </summary>
    public sealed class EchoCloneThunderScript : ThunderScript
    {
        public static EchoCloneThunderScript Instance { get; private set; }

        [ModOption("Prototype Enabled", "Prototype switch only; gameplay features are not wired up yet.")]
        public static void PrototypeEnabled(bool value)
        {
            Debug.Log("[EchoClone] Prototype Enabled set to " + value + ". Gameplay integration is not implemented yet.");
        }

        [ModOption("Hostile Mode", "Placeholder setting. This does not make a clone hostile yet.")]
        public static void HostileMode(bool value)
        {
            Debug.Log("[EchoClone] Hostile Mode set to " + value + ". Combat integration is not implemented yet.");
        }

        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);
            Instance = this;
            Debug.Log("[EchoClone] ThunderScript loaded for mod: " + modData.Name);
        }

        public override void ScriptUpdate()
        {
            base.ScriptUpdate();
            // Pose sampling is intentionally not added until the target game's
            // player tracking and creature-rig APIs are compiled against the
            // exact Nomad assemblies.
        }
    }
}