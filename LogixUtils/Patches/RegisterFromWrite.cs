using BaseX;
using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.LogiX.Data;
using HarmonyLib;
using System;
using System.Reflection;

namespace LogixUtils
{
    class RegisterFromWrite : ToggleablePatch
    {
        public override void Patch(Harmony harmony, LogixUtils mod)
        {
            var lgxtSecondary = typeof(LogixTip).GetMethod("OnSecondaryPress", BindingFlags.Public | BindingFlags.Instance);
            var patch = typeof(RegisterFromWrite).GetMethod("Prefix");

            harmony.Patch(lgxtSecondary, prefix: new HarmonyMethod(patch));
        }
        public override void Unpatch(Harmony harmony, LogixUtils mod)
        {
            var lgxtSecondary = typeof(LogixTip).GetMethod("OnSecondaryPress", BindingFlags.Public | BindingFlags.Instance);
            var patch = typeof(RegisterFromWrite).GetMethod("Prefix");

            harmony.Unpatch(lgxtSecondary, patch);
        }

        public static bool Prefix(LogixTip __instance, ref IInputElement ____input, ref Slot ____tempWire)
        {
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

            return (LogixNode)LogixUtils.ReversePatches.CreateNewNodeSlot(logixTip, LogixHelper.GetNodeName(type)).AttachComponent(type);
        }


    }
}
