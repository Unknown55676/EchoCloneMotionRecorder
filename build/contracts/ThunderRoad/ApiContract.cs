// Build-only ThunderRoad API contract for EchoClone's compile check.
// This assembly is NOT shipped. It contains only public signatures used by the
// plugin and is not a replacement for a version-matched official SDK assembly.
using System;
using UnityEngine;

namespace ThunderRoad
{
    public abstract class ThunderScript : MonoBehaviour
    {
        public virtual void ScriptLoaded(ModManager.ModData modData) { }
        public virtual void ScriptUpdate() { }
    }

    public class ModManager
    {
        public class ModData
        {
            public string Name { get; set; }
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ModOptionCategoryAttribute : Attribute
    {
        public ModOptionCategoryAttribute(string category, int order) { }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ModOptionDontSaveAttribute : Attribute
    {
        public ModOptionDontSaveAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ModOptionAttribute : Attribute
    {
        public int defaultValueIndex { get; set; }

        public ModOptionAttribute(string name, string tooltip, string valueSourceName = null) { }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ModOptionButtonAttribute : Attribute
    {
        public ModOptionButtonAttribute() { }
    }

    public sealed class ModOptionString
    {
        public ModOptionString(string name, string value) { }
    }
}
