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
            var alignItem = typeof(UIAlignItemTweaks).GetMethod("AlignItem");

            harmony.Patch(uiAlignItem, postfix: new HarmonyMethod(alignItem));
        }
        public override void Unpatch(Harmony harmony, LogixUtils mod)
        {
            var uiAlignItem = typeof(UI_TargettingController).GetMethod("AlignItem", BindingFlags.Public | BindingFlags.Instance);
            var alignItem = typeof(UIAlignItemTweaks).GetMethod("AlignItem");

            harmony.Unpatch(uiAlignItem, alignItem);
        }

        public static void AlignItem(UI_TargettingController __instance, Slot root, ref floatQ orientation, ref float3? scale)
        {
            if (__instance.InputInterface.GetKey(LogixUtils.config.GetValue(LogixUtils.AlignScaleModifierKey))) // Key modifier to set scale
            {
                scale = float3.One;
                if (LogixUtils.config.GetValue(LogixUtils.ModifiedScaleToUserScale))
                {
                    scale *= root.LocalUserRoot.GlobalScale;
                }
            }

            floatQ globalViewRotation = __instance.ViewSpace.LocalRotationToGlobal(__instance.ViewRotation);

            if (LogixUtils.config.GetValue(LogixUtils.SnapToAngleOnAlign)) // Key modifier to snap rotation
            {
                var snapAngle = LogixUtils.config.GetValue(LogixUtils.SnapAngle);
                floatQ localRotation = floatQ.InvertedMultiply(globalViewRotation, root.GlobalRotation);

                //MathX.Snap(localRotation, 45f) does not appear to give the desired result?
                floatQ roundedRotation = floatQ.Euler(MathX.Round(localRotation.EulerAngles / snapAngle) * snapAngle);
                orientation = globalViewRotation * roundedRotation; 
                return;
            }

            if (MathX.Dot(root.Forward, globalViewRotation * float3.Forward) < 0f)
            {
                orientation = floatQ.LookRotation(orientation * float3.Backward, orientation * float3.Up);
            }
        }
    }
}
