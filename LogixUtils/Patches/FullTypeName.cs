using BaseX;
using FrooxEngine;
using FrooxEngine.LogiX;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace LogixUtils
{
    class FullTypeName : ToggleablePatch
    {
        public override void Patch(Harmony harmony, LogixUtils mod)
        {
            var getNodeName = typeof(LogixHelper).GetMethod("GetNodeName"); //, BindingFlags.NonPublic | BindingFlags.Instance
            var lgxtOnChanges = typeof(LogixTip).GetMethod("OnChanges", BindingFlags.NonPublic | BindingFlags.Instance);

            var prefix = typeof(FullTypeName).GetMethod("PatchNodeName");
            var multiplayerFix = typeof(FullTypeName).GetMethod("MultiplayerFix");

            harmony.Patch(getNodeName, prefix: new HarmonyMethod(prefix));
            harmony.Patch(lgxtOnChanges, prefix: new HarmonyMethod(multiplayerFix));
        }
        public override void Unpatch(Harmony harmony, LogixUtils mod)
        {
            var getNodeName = typeof(LogixHelper).GetMethod("GetNodeName"); //, BindingFlags.NonPublic | BindingFlags.Instance
            var lgxtOnChanges = typeof(LogixTip).GetMethod("OnChanges", BindingFlags.NonPublic | BindingFlags.Instance);

            var prefix = typeof(FullTypeName).GetMethod("PatchNodeName");
            var multiplayerFix = typeof(FullTypeName).GetMethod("MultiplayerFix");

            harmony.Unpatch(getNodeName, prefix);
            harmony.Unpatch(lgxtOnChanges,  multiplayerFix);
        }

        public static bool PatchNodeName(Type nodeType, ref string __result)
        {
            NodeName nodeName = nodeType.GetCustomAttributes(typeof(NodeName), false).Cast<NodeName>().FirstOrDefault();
            if ((__result = (nodeName?.Name)) == null)
            {
                __result = (StringHelper.BeautifyName(LogixHelper.GetOverloadName(nodeType)) ?? GetFormattedName(nodeType));
            }
            return false;
        }
        public static void MultiplayerFix(LogixTip __instance, SyncRef<TextRenderer> ___Label)
        {
            if ((__instance.Slot.ActiveUser != null && __instance.Slot.ActiveUser != __instance.LocalUser) || ___Label.Target == null) return; //Return if ActiveUser is not local user unless null   Also Return if Label is null

            Sync<string> LabelTextField = ___Label.Target.Text;
            if (LabelTextField.IsDriven || LabelTextField.IsHooked) return;
            LabelTextField.DriveFrom(LabelTextField, writeBack: true).Persistent = false;
        }



        internal static string GetFormattedName(Type type)
        {
            if (type.IsConstructedGenericType)
            {
                string genericArguments = string.Join(",", type.GetGenericArguments().Select(GetFormattedName));
                return $"{StringHelper.BeautifyName(type.Name.Split('`')[0])}<{genericArguments}>";
            }
            return StringHelper.BeautifyName(type.Name);
        }
    }
}
