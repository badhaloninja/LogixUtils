using FrooxEngine;
using FrooxEngine.LogiX;
using HarmonyLib;
using NeosModLoader;
using System;
using System.Collections.Generic;

namespace LogixUtils
{
    public class LogixUtils : NeosMod
    {
        public override string Name => "LogixUtils";
        public override string Author => "badhaloninja";
        public override string Version => "1.5.2";
        public override string Link => "https://github.com/badhaloninja/LogixUtils";

        internal static ModConfiguration config;
        private static Harmony harmony;


        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> NodeScaleFixesOption = new ModConfigurationKey<bool>("nodeScales", "Fix various nodes not scaling relative to user", () => true);
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> ShowFullTypeLogixLabel = new ModConfigurationKey<bool>("fullTypeLogixLabel", "Make logix label display full type name", () => true);
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> UIAlignItemsBackwards = new ModConfigurationKey<bool>("uiAlignItemsBackwards", "Allow aligning logix backwards", () => true);
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> GenRegisterFromWrite = new ModConfigurationKey<bool>("genRegisterFromWrite", "Allow spawning a Value/Reference register from a Write node target", () => true);
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> ExtractRefOfAny = new ModConfigurationKey<bool>("extractRefOfAny", "Make 'Extract Ref Node' allow any refrence instead of only IField", () => true);
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> ClampNodeTextures = new ModConfigurationKey<bool>("clampNodeTextures", "Clamp various node textures", () => true);


        //
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> AddInputNodes = new ModConfigurationKey<bool>("addInputNodes", "Add unused input nodes to input node list", () => true);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> AddOtherInputNodes = new ModConfigurationKey<bool>("addOtherInputNodes", "Also add other input nodes", () => true);

        //
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> RepairCrashedNodesContext = new ModConfigurationKey<bool>("repairCrashedNodesContext", "Add context menu item to attempt repairing crashed nodes", () => true);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> ReportCrashedNodeRepair = new ModConfigurationKey<bool>("reportCrashedNodeRepair", "Generate a report after attempting to repair crashed nodes", () => false);


        public static Dictionary<ModConfigurationKey<bool>, ToggleablePatch> TogglePatchList = new Dictionary<ModConfigurationKey<bool>, ToggleablePatch>() {
            { NodeScaleFixesOption, new NodeScales() },
            { ShowFullTypeLogixLabel, new FullTypeName() },
            { UIAlignItemsBackwards, new UIAlignItemTweaks() },
            { GenRegisterFromWrite, new RegisterFromWrite() },
            { ExtractRefOfAny, new ExtractRef() },
            { ClampNodeTextures, new ClampNodes() },
            { AddInputNodes, new InputNodes() },
            { RepairCrashedNodesContext, new TryRepairNodes() }
        };

        public override void OnEngineInit()
        {
            config = GetConfiguration();

            harmony = new Harmony("me.badhaloninja.LogixUtils");
            harmony.PatchAll(); //For reverse patches


            foreach(var patch in TogglePatchList)
            {
                patch.Value.Initialize(harmony, this, config);
                config.OnThisConfigurationChanged += patch.Value.OnThisConfigurationChanged;
                if (!config.GetValue(patch.Key)) continue;
                patch.Value.Patch(harmony, this);
            }

            config.OnThisConfigurationChanged += HandleConfigChanged;
        }




        private void HandleConfigChanged(ConfigurationChangedEvent configurationChangedEvent)
        {
            var BoolKey = configurationChangedEvent.Key as ModConfigurationKey<bool>;

            if (!config.TryGetValue(BoolKey, out bool value)) return;

            if (TogglePatchList.TryGetValue(BoolKey, out ToggleablePatch patch)) {
                if (value)
                {
                    Msg($"Patching: {patch.GetType().Name}");
                    patch.Patch(harmony, this);
                    return;
                }
                Msg($"Unpatching: {patch.GetType().Name}");
                patch.Unpatch(harmony, this);
            }
        }












        [HarmonyPatch]
        public static class ReversePatches
        {
            // Reflections go brrrrrr
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(LogixTip), "GetHeldSlotReference")]
            public static Slot GetHeldSlotReference(LogixTip instance, out ReferenceProxy referenceProxy)
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
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(LogixTip), "CreateNewNodeSlot")]
            public static Slot CreateNewNodeSlot(LogixTip instance, string name)
            {
                throw new NotImplementedException("It's a stub");
            }
        }
    }
}