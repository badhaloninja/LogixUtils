using FrooxEngine;
using FrooxEngine.LogiX;
using HarmonyLib;
using System.Reflection;

namespace LogixUtils
{
    class WireBeGone : ToggleablePatch
    {
        public override void Patch(Harmony harmony, LogixUtils mod)
        {
            var onAttachMethod = typeof(ConnectionWire).GetMethod("OnAttach", BindingFlags.NonPublic | BindingFlags.Instance);

            var wireBeGonePatchMethod = typeof(WireBeGone).GetMethod("wireBeGonePatch");

            harmony.Patch(onAttachMethod, postfix: new HarmonyMethod(wireBeGonePatchMethod));
        }

        public override void Unpatch(Harmony harmony, LogixUtils mod)
        {
            var onAttachMethod = typeof(ConnectionWire).GetMethod("OnAttach", BindingFlags.NonPublic | BindingFlags.Instance);

            var wireBeGonePatchMethod = typeof(WireBeGone).GetMethod("wireBeGonePatch");

            harmony.Unpatch(onAttachMethod, wireBeGonePatchMethod);
        }

        public static void wireBeGonePatch(ConnectionWire __instance)
        {
            if (__instance.Slot.Name.Equals("LinkPoint"))
            {
                __instance.Slot.ActiveSelf = false;
            }
        }
    }
}