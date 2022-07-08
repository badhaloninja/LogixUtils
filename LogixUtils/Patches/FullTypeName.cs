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
            NodeName nodeName = nodeType.GetCustomAttributes(typeof(NodeName), false).Cast<NodeName>().FirstOrDefault<NodeName>();
            if ((__result = ((nodeName != null) ? nodeName.Name : null)) == null)
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
                return $"{StringHelper.BeautifyName(type.Name.Substring(0, type.Name.IndexOf("`")))}<{genericArguments}>";
            }
            return StringHelper.BeautifyName(type.Name);
        }



        /*
                // Logix label tweak
                [HarmonyPatch(typeof(LogixHelper), "GetNodeName")]
                class getNodeNamePatch
                {
                    public static bool Prefix(Type nodeType, ref string __result)
                    {
                        NodeName nodeName = nodeType.GetCustomAttributes(typeof(NodeName), false).Cast<NodeName>().FirstOrDefault<NodeName>();
                        if ((__result = ((nodeName != null) ? nodeName.Name : null)) == null)
                        {
                            __result = (StringHelper.BeautifyName(LogixHelper.GetOverloadName(nodeType)) ?? GetFormattedName(nodeType));
                        }
                        return false;
                    }

                    public static string GetFormattedName(Type type)
                    {
                        if (type.IsConstructedGenericType)
                        {
                            string genericArguments = string.Join(",", type.GetGenericArguments().Select(GetFormattedName));
                            return $"{StringHelper.BeautifyName(type.Name.Substring(0, type.Name.IndexOf("`")))}<{genericArguments}>";
                        }
                        return StringHelper.BeautifyName(type.Name);
                    }


                    [HarmonyPatch(typeof(LogixTip))]
                    class MultiplayerFix  // Fix Label fighting when in session with users who don't have the mod
                    {
                        *//* Apparently Last Modifying user gets cleared immediately???
                         * 
                        [HarmonyReversePatch]
                        [HarmonyPatch("UpdateSpawnLabel")]
                        public static void UpdateSpawnLabel(LogixTip instance)
                        {
                            // its a stub so it has no initial content
                            throw new NotImplementedException("It's a stub");
                        }
                        [HarmonyPostfix]
                        [HarmonyPatch("Update")]
                        public static void Update(LogixTip __instance, SyncRef<TextRenderer> ___Label) // Update label of own tool if it was last updated by someone else
                        {
                            if (___Label.Target == null) return;
                            Sync<string> LabelTextField = ___Label.Target.Text;
                            if (LabelTextField.WasLastModifiedBy(__instance.LocalUser) || LabelTextField.LastModifyingUser == null) return; //Return if last modifying user was not current user
                            UpdateSpawnLabel(__instance);
                        }
                        [HarmonyPrefix] 
                        [HarmonyPatch("OnChanges")]
                        public static bool OnChanges(LogixTip __instance, SyncType ___ActiveNodeType) // Prevent triggering on other users' LogixTips
                        {
                            Msg(___ActiveNodeType.LastModifyingUser);
                            return (__instance.Slot.ActiveUser == __instance.LocalUser || // Continue if activeUser is local user OR
                                __instance.Slot.ActiveUser == null && ___ActiveNodeType.WasLastModifiedBy(__instance.LocalUser)); // if ActiveUser is null AND node type was last modified by LocalUser
                        }*//*

                        // ValueCopy version
                        // This works fine, but I don't want to resort to making it local
                        [HarmonyPrefix]
                        [HarmonyPatch("OnChanges")]
                        public static void OnChanges(LogixTip __instance, SyncRef<TextRenderer> ___Label)
                        {
                            if ((__instance.Slot.ActiveUser != null && __instance.Slot.ActiveUser != __instance.LocalUser) || ___Label.Target == null) return; //Return if ActiveUser is not local user unless null   Also Return if Label is null

                            Sync<string> LabelTextField = ___Label.Target.Text;
                            if (LabelTextField.IsDriven || LabelTextField.IsHooked) return;
                            LabelTextField.DriveFrom(LabelTextField, writeBack: true).Persistent = false;
                        }
                    }
                }*/
    }
}
