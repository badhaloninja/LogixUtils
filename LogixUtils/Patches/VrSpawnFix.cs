using FrooxEngine;
using FrooxEngine.LogiX;
using HarmonyLib;
using System.Reflection;

namespace LogixUtils
{
    class VrSpawnFix : ToggleablePatch
    {
        public override void Patch(Harmony harmony, LogixUtils mod)
        {
            var positionSpawnedNodeMethod = typeof(LogixTip).GetMethod("PositionSpawnedNode", BindingFlags.NonPublic | BindingFlags.Instance);

            var vrNodeRotationPatchMethod = typeof(VrSpawnFix).GetMethod("VrNodeRotationPatch");

            harmony.Patch(positionSpawnedNodeMethod, postfix: new HarmonyMethod(vrNodeRotationPatchMethod));
        }

        public override void Unpatch(Harmony harmony, LogixUtils mod)
        {
            var positionSpawnedNodeMethod = typeof(LogixTip).GetMethod("PositionSpawnedNode", BindingFlags.NonPublic | BindingFlags.Instance);

            var vrNodeRotationPatchMethod = typeof(VrSpawnFix).GetMethod("VrNodeRotationPatch");

            harmony.Unpatch(positionSpawnedNodeMethod, vrNodeRotationPatchMethod);
        }

        public static void VrNodeRotationPatch(LogixTip __instance, Slot node)
        {
            if (__instance.InputInterface.VR_Active)
            {
                node.Up = __instance.Slot.ActiveUserRoot.Slot.Up;
            }
        }
    }
}