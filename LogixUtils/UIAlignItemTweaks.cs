using BaseX;
using FrooxEngine;
using HarmonyLib;
using System.Reflection;


namespace LogixUtils
{
    class UIAlignItemTweaks : ToggleablePatch
    {
        public override void Patch(Harmony harmony, LogixUtils mod)
        {
            var uiAlignItem = typeof(UI_TargettingController).GetMethod("AlignItem", BindingFlags.Public | BindingFlags.Instance);
            var alignItem = typeof(UIAlignItemTweaks).GetMethod("alignItem");

            harmony.Patch(uiAlignItem, postfix: new HarmonyMethod(alignItem));
        }
        public override void Unpatch(Harmony harmony, LogixUtils mod)
        {
            var uiAlignItem = typeof(UI_TargettingController).GetMethod("AlignItem", BindingFlags.Public | BindingFlags.Instance);
            var alignItem = typeof(UIAlignItemTweaks).GetMethod("alignItem");

            harmony.Unpatch(uiAlignItem, alignItem);
        }

        public static void alignItem(UI_TargettingController __instance, Slot root, ref floatQ orientation)
        {
            floatQ globalViewRotation = __instance.ViewSpace.LocalRotationToGlobal(__instance.ViewRotation);
            if (MathX.Dot(root.Forward, globalViewRotation * float3.Forward) < 0f)
            {
                orientation = floatQ.LookRotation(orientation * float3.Backward, orientation * float3.Up);
            }
        }



        /*// UI align item backwards or forwards
        [HarmonyPatch(typeof(UI_TargettingController), "AlignItem")]
        class UIAlign_Patch
        {
            public static void Postfix(UI_TargettingController __instance, Slot root, ref floatQ orientation)
            {
                floatQ globalViewRotation = __instance.ViewSpace.LocalRotationToGlobal(__instance.ViewRotation);
                if (MathX.Dot(root.Forward, globalViewRotation * float3.Forward) < 0f)
                {
                    orientation = floatQ.LookRotation(orientation * float3.Backward, orientation * float3.Up);
                }
            }
        }*/
    }
}
