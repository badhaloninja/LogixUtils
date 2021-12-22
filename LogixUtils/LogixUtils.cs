using BaseX;
using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.LogiX.Cast;
using HarmonyLib;
using NeosModLoader;

namespace LogixUtils
{
    public class LogixUtils : NeosMod
    {
        public override string Name => "LogixUtils";
        public override string Author => "badhaloninja";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/badhaloninja/LogixUtils";
        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("me.badhaloninja.LogixUtils");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(LogixTip), "AttachDriver")]
        class LogixTip_AttachDriver_Patch
        {
            public static void Postfix(ref IDriverNode __result)
            {
                Slot targetSlot = __result.Slot;
                targetSlot.GlobalScale = targetSlot.World.LocalUserGlobalScale;
            }
        }
        [HarmonyPatch(typeof(CastNode), "AttachCastNode")]
        class CastNode_AttachCastNode_Patch
        {
            public static void Postfix(ref ICastNode __result)
            {
                Slot targetSlot = __result.Slot;
                targetSlot.GlobalScale = targetSlot.World.LocalUserGlobalScale;

            }
        }
        [HarmonyPatch(typeof(LogixHelper), "GetReferenceNode")]
        class LogixHelper_GetReferenceNode_Patch
        {
            public static void Postfix(ref IReferenceNode __result)
            {
                Slot targetSlot = __result.Slot;
                targetSlot.GlobalScale = targetSlot.World.LocalUserGlobalScale;
            }
        }
        [HarmonyPatch(typeof(ComponentBase<Component>), "OnAttach")] // ;-;
        class ComponentAttach_Patch
        {
            public static void Postfix(Component __instance)
            {
                if (!(__instance is ImpulseRelay || __instance is RelayNode<dummy>))
                    return;
                Slot targetSlot = __instance.Slot;
                targetSlot.GlobalScale = targetSlot.World.LocalUserGlobalScale;
            }
        }
        //[HarmonyPatch(typeof(Slot), nameof(Slot.AttachComponent<ImpulseRelay>)] // how do I patch generic methods?
    }
}

