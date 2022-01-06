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
        public override string Version => "1.3.0";
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
        }
        
        
        [HarmonyPatch(typeof(UI_TargettingController), "AlignItem")]
        class UIAlign_Patch
        {
            public static void Postfix(UI_TargettingController __instance, Slot root, ref floatQ orientation)
            {
                if (MathX.Dot(root.Forward, __instance.ViewRotation * float3.Forward) < 0f)
                {
                    orientation = orientation.Inverted;
                }
            }
        }
    }
}