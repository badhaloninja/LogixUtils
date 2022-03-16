using BaseX;
using FrooxEngine;
using FrooxEngine.LogiX;
using HarmonyLib;
using System.Reflection;

namespace LogixUtils
{
    class ExtractRef : ToggleablePatch
    {
        public override void Patch(Harmony harmony, LogixUtils mod)
        {
            var lgxtSecondary = typeof(LogixTip).GetMethod("OnSecondaryPress", BindingFlags.Public | BindingFlags.Instance);
            var patch = typeof(ExtractRef).GetMethod("Prefix");

            harmony.Patch(lgxtSecondary, prefix: new HarmonyMethod(patch));
        }
        public override void Unpatch(Harmony harmony, LogixUtils mod)
        {
            var lgxtSecondary = typeof(LogixTip).GetMethod("OnSecondaryPress", BindingFlags.Public | BindingFlags.Instance);
            var patch = typeof(ExtractRef).GetMethod("Prefix");

            harmony.Unpatch(lgxtSecondary, patch);
        }

        public static bool Prefix(LogixTip __instance, Sync<LogixTip.ExtractMode> ____extract)
        {
            CommonTool activeTool = __instance.ActiveTool;

            //Initial Checks
            bool isHoldingObject = (activeTool != null && activeTool.Grabber != null && activeTool.Grabber.IsHoldingObjects);
            if (isHoldingObject)
            {
                if (____extract.Value == LogixTip.ExtractMode.ReferenceNode)
                { // Extract ref node of any reference
                    return handleExtractRefNode(__instance);
                }
            }
            return true;
        }



        private static bool handleExtractRefNode(LogixTip instance)
        {
            ReferenceProxy referenceProxy;
            Slot heldSlotReference = LogixUtils.ReversePatches.GetHeldSlotReference(instance, out referenceProxy);

            // Why is this a thing??? On secondary if you are holding a slot ref named "LogiX_Pack" it will unpack that slot?
            if (heldSlotReference != null && heldSlotReference.Name == LogixHelper.LOGIX_PACK_NAME) return true;


            IWorldElement worldElement = (referenceProxy != null) ? referenceProxy.Reference.Target : null;
            //if (worldElement as IField != null) return true; // Continue original behavior if it is a field

            Slot nodeSlot = instance.LocalUserSpace.AddSlot("Reference Node");

            //Original functionality 
            IReferenceNode referenceNode = LogixUtils.ReversePatches.PrefixReferenceNode(nodeSlot, worldElement, worldElement.GetType());

            if (referenceNode == null) return false;

            LogixUtils.ReversePatches.PositionSpawnedNode(instance, referenceNode.Slot);
            Slot display = LogixUtils.ReversePatches.AttachOutput(instance, referenceNode);
            if (display != null)
            {
                float3 localPosition = display.LocalPosition;
                floatQ localRotation = referenceNode.Slot.LocalRotation;

                float3 direction = localRotation * (float3.Right + float3.Up);
                float3 offset = direction * 0.1f;
                display.LocalPosition = localPosition + offset;
            }

            return false;
        }

    }
}
