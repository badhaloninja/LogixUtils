using BaseX;
using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.LogiX.Cast;
using HarmonyLib;
using NeosModLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace LogixUtils
{
    public class LogixUtils : NeosMod
    {
        public override string Name => "LogixUtils";
        public override string Author => "badhaloninja";
        public override string Version => "1.3.2";
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

        //IL go brr
        [HarmonyPatch(typeof(LogixTip), "OnGrabbing")]
        class RelayScaleFix
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            { // Pretty good tutorial, https://gist.github.com/JavidPack/454477b67db8b017cb101371a8c49a1c
                var code = new List<CodeInstruction>(instructions);
                int insertionIndex = -1;
                Label relayAddSlotLabel = il.DefineLabel();
                
                for (int i = 0; i < code.Count; i++) //Find where to inject code
                {
                    if (code[i].opcode == OpCodes.Ldstr && (string)code[i].operand == "Relay" && code[i + 2].operand is MethodInfo) //Find relay string 2 instructions before a method
                    {
                        MethodInfo mi = code[i + 2].operand as MethodInfo;
                        if (mi.Name == "AddSlot") // Check if method is AddSlot
                        {
                            insertionIndex = i + 4; // 1 more than the stloc.s after the method instruction
                            code[insertionIndex].labels.Add(relayAddSlotLabel);
                        }
                        break;
                    }
                }

                var instructionsToInsert = new List<CodeInstruction>();

                instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, (sbyte)4));
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, (sbyte)4));
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Worker), "get_World"))); //Getters go brr
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(World), "get_LocalUserGlobalScale")));
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Slot), "set_GlobalScale", new Type[] { typeof(float3) }))); //Setters go brr

                if (insertionIndex != -1) // If AddSlot("Relay") found inject code
                {
                    code.InsertRange(insertionIndex, instructionsToInsert);
                }
                return code;
            }
        }
        
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
                    string genericArguments = string.Join(",",type.GetGenericArguments().Select(GetFormattedName));
                    return $"{StringHelper.BeautifyName(type.Name.Substring(0, type.Name.IndexOf("`")))}<{genericArguments}>";
                }
                return StringHelper.BeautifyName(type.Name);
            }


            [HarmonyPatch(typeof(LogixTip))]
            class MultiplayerFix  // Fix Label fighting when in session with users who don't have the mod
            {
                /* Apparently Last Modifying user gets cleared immediately???
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
                }*/



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
        }
        
        
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
        }





/*        [HarmonyPatch(typeof(LogixTip))]
        class debugFunny
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnAttach")]
            public static void OnAttach(LogixTip __instance)
            {
                string name = __instance.Slot.ActiveUser.UserName;
                Msg("OnAttach Triggered, ActiveUser: " + name + ", RefID: " + __instance.ReferenceID);
            }
            [HarmonyPrefix]
            [HarmonyPatch("OnChanges")]
            public static void OnChanges(LogixTip __instance)
            {
                string name = __instance.Slot.ActiveUser.UserName;
                Msg("OnChanges Triggered, ActiveUser: " + name + ", RefID: " + __instance.ReferenceID);
            }
            [HarmonyPrefix]
            [HarmonyPatch("SetActiveType")]
            public static void SetActiveType(LogixTip __instance)
            {
                string name = __instance.Slot.ActiveUser.UserName;
                Msg("SetActiveType Triggered, ActiveUser: " + name + ", RefID: " + __instance.ReferenceID);
            }
*//*            [HarmonyPrefix]
            [HarmonyPatch("Update")]
            public static void Update(LogixTip __instance)
            {
                string name = __instance.Slot.ActiveUser.UserName;
                Msg("Update Triggered, ActiveUser: " + name + ", RefID: " + __instance.ReferenceID);
            }*//*
        }*/
    }
}