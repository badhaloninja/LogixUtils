using BaseX;
using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.LogiX.Cast;
using FrooxEngine.LogiX.Data;
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
        public override string Version => "1.5.0";
        public override string Link => "https://github.com/badhaloninja/LogixUtils";
        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("me.badhaloninja.LogixUtils");
            harmony.PatchAll();
        }

        // Various Scale fixes
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


        // UI align item backwards or forwards
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


        // Extract Ref Node of any type
        [HarmonyPatch(typeof(LogixTip), "OnSecondaryPress")]
        class LogixTip_OnSecondaryPress_Patch
        {
            public static bool Prefix(LogixTip __instance, Sync<LogixTip.ExtractMode> ____extract, ref IInputElement ____input, ref Slot ____tempWire)
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
                if (____input != null && ____input.InputType.IsConstructedGenericType)
                { // Spawn Register inputs for write nodes
                    LogixNode register = createRegisterInput(__instance, ____input);
                    if (register != null)
                    {
                        ____input.TryConnectTo(register);
                        if (____input.TargetNode != null)
                        { // Offset new ref node
                            LogixNode refNode = ____input.TargetNode;

                            float3 localPosition = refNode.Slot.LocalPosition;
                            floatQ localRotation = register.Slot.LocalRotation;

                            float3 direction = localRotation * (register.Slot.LocalScale * float3.Right);
                            float3 offset = direction * 0.036f; // Output proxy location
                            refNode.Slot.LocalPosition = localPosition + offset;
                        }
                        ____input = null;
                        Slot tempWire = ____tempWire;
                        if (tempWire != null)
                        {
                            tempWire.Destroy();
                        }
                        ____tempWire = null;
                        return false;
                    };
                }
                return true;
            }

            private static bool handleExtractRefNode(LogixTip instance)
            {
                ReferenceProxy referenceProxy;
                Slot heldSlotReference = GetHeldSlotReference(instance, out referenceProxy);

                // Why is this a thing??? On secondary if you are holding a slot ref named "LogiX_Pack" it will unpack that slot?
                if (heldSlotReference != null && heldSlotReference.Name == LogixHelper.LOGIX_PACK_NAME) return true;


                IWorldElement worldElement = (referenceProxy != null) ? referenceProxy.Reference.Target : null;
                //if (worldElement as IField != null) return true; // Continue original behavior if it is a field

                Slot nodeSlot = instance.LocalUserSpace.AddSlot("Reference Node");

                //Original functionality 
                IReferenceNode referenceNode = PrefixReferenceNode(nodeSlot, worldElement, worldElement.GetType());

                if (referenceNode == null) return false;

                PositionSpawnedNode(instance, referenceNode.Slot);
                Slot display = AttachOutput(instance, referenceNode);
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

            private static LogixNode createRegisterInput(LogixTip logixTip, IInputElement input)
            {
                Type baseType = input.InputType.GetGenericTypeDefinition();

                if (baseType != typeof(IValue<>)) return null;
                Type genericType = input.InputType.GetGenericArguments()[0];

                Type targetGeneric = (Coder.IsNeosPrimitive(genericType)) ? typeof(ValueRegister<>) : typeof(ReferenceRegister<>);

                Type type = targetGeneric.MakeGenericType(new Type[]
                {
                    genericType
                });

                return (LogixNode)CreateNewNodeSlot(logixTip, LogixHelper.GetNodeName(type)).AttachComponent(type);
            }




            // Reflections go brrrrrr
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(LogixTip), "GetHeldSlotReference")]
            public static Slot GetHeldSlotReference(LogixTip instance, out ReferenceProxy referenceProxy)
            {
                throw new NotImplementedException("It's a stub");
            }
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(LogixTip), "CreateNewNodeSlot")]
            public static Slot CreateNewNodeSlot(LogixTip instance, string name)
            {
                throw new NotImplementedException("It's a stub");
            }
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(LogixTip), "PositionSpawnedNode")]
            public static void PositionSpawnedNode(LogixTip instance, Slot node)
            {
                throw new NotImplementedException("It's a stub");
            }
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(LogixTip), "AttachOutput")]
            public static Slot AttachOutput(LogixTip instance, IWorldElement output)
            {
                throw new NotImplementedException("It's a stub");
            }
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(ReferenceNode), "PrefixReferenceNode")]
            public static IReferenceNode PrefixReferenceNode(Slot slot, IWorldElement target, Type targetType)
            {
                throw new NotImplementedException("It's a stub");
            }
        }


        // Make RefNode Texture Clamp
        [HarmonyPatch(typeof(ReferenceNode), "PrefixReferenceNode")]
        class ReferenceNodeTexture_Clamp
        {
            public static void Postfix(ref IReferenceNode __result)
            {
                if (__result == null) return;
                __result.World.GetSharedComponentOrCreate("LogiX_RefNodeTexture", delegate (StaticTexture2D tex)
                {
                    tex.URL.Value = NeosAssets.Testing.Logix.Reference;
                }).WrapMode = TextureWrapMode.Clamp;
            }
        }
    }
}