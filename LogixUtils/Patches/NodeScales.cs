using BaseX;
using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.LogiX.Cast;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;



namespace LogixUtils
{
    class NodeScales : ToggleablePatch
    {
        public override void Patch(Harmony harmony, LogixUtils mod) {
            var drivers = typeof(LogixTip).GetMethod("AttachDriver", BindingFlags.NonPublic | BindingFlags.Instance);
            var casts = typeof(CastNode).GetMethod("AttachCastNode");
            var refrenceNodes = typeof(LogixHelper).GetMethod("GetReferenceNode");
            var relays = typeof(LogixTip).GetMethod("OnGrabbing");

            var postfix = typeof(NodeScales).GetMethod("SetScale");
            var relayTranspiler = typeof(NodeScales).GetMethod("RelayTranspiler");



            harmony.Patch(drivers, postfix: new HarmonyMethod(postfix));
            harmony.Patch(casts, postfix: new HarmonyMethod(postfix));
            harmony.Patch(refrenceNodes, postfix: new HarmonyMethod(postfix));
            
            harmony.Patch(relays, transpiler: new HarmonyMethod(relayTranspiler));
        }
        public override void Unpatch(Harmony harmony, LogixUtils mod)
        {
            var drivers = typeof(LogixTip).GetMethod("AttachDriver", BindingFlags.NonPublic | BindingFlags.Instance);
            var casts = typeof(CastNode).GetMethod("AttachCastNode");
            var refrenceNodes = typeof(LogixHelper).GetMethod("GetReferenceNode");
            var relays = typeof(LogixTip).GetMethod("OnGrabbing");

            var postfix = typeof(NodeScales).GetMethod("SetScale");
            var relayTranspiler = typeof(NodeScales).GetMethod("RelayTranspiler");

            harmony.Unpatch(drivers, postfix);
            harmony.Unpatch(casts, postfix);
            harmony.Unpatch(refrenceNodes, postfix);

            harmony.Unpatch(relays, relayTranspiler);
        }

        public static void SetScale(ref IComponent __result)
        {
            Slot targetSlot = __result.Slot;
            targetSlot.GlobalScale = targetSlot.World.LocalUserGlobalScale;
        }


        public static IEnumerable<CodeInstruction> RelayTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
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

/*
        // Various Scale fixes
        [HarmonyPatch(typeof(LogixTip), "AttachDriver")]
        class LogixTip_AttachDriver_Patch
        {
            public static void Postfix(ref IComponent __result)
            {
                NeosMod.Msg("YES");
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
        }*/
    }
}
