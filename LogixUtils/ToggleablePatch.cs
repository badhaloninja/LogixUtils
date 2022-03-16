using NeosModLoader;
using HarmonyLib;

namespace LogixUtils
{
    public abstract class ToggleablePatch
    {
        public virtual void Initialize(Harmony harmony, LogixUtils mod, ModConfiguration config) { }
        public virtual void Patch(Harmony harmony, LogixUtils mod) { }
        public virtual void Unpatch(Harmony harmony, LogixUtils mod) { }
        public virtual void OnThisConfigurationChanged(ConfigurationChangedEvent configurationChangedEvent) { }
    }
}
