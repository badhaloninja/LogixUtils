using System;
using FrooxEngine;
using FrooxEngine.LogiX;
using HarmonyLib;
using System.Reflection;
using BaseX;
using NeosModLoader;
using System.Text;
using System.Collections.Generic;
using System.Linq;
namespace LogixUtils
{
    class TryRepairNodes : ToggleablePatch
    {
        public override void Patch(Harmony harmony, LogixUtils mod)
        {
            var genMenuItems = typeof(LogixTip).GetMethod("GenerateMenuItems", BindingFlags.Public | BindingFlags.Instance);
            var newItem = typeof(TryRepairNodes).GetMethod("TryRepairNodesContextItem");

            harmony.Patch(genMenuItems, postfix: new HarmonyMethod(newItem));
        }
        public override void Unpatch(Harmony harmony, LogixUtils mod)
        {
            var genMenuItems = typeof(LogixTip).GetMethod("GenerateMenuItems", BindingFlags.Public | BindingFlags.Instance);
            var newItem = typeof(TryRepairNodes).GetMethod("TryRepairNodesContextItem");

            harmony.Unpatch(genMenuItems, newItem);
        }
        public static void TryRepairNodesContextItem(LogixTip __instance, CommonTool tool, ContextMenu menu)
        {
            Slot root = LogixUtils.ReversePatches.GetHeldSlotReference(__instance, out ReferenceProxy referenceProxy);
            if (root == null || root.GetComponentsInChildren((LogixNode node) => !node.Enabled).Count == 0) return;

            var item = menu.AddItem("Try Repair Nodes", Assets.Images.CrashedNode, color.Red);
            item.Button.LocalPressed += 
                (btn, btnEv) => {
                    repairNodesUnderRoot(root);
                    __instance.ActiveTool.CloseContextMenu();
                };
        }

        private static void repairNodesUnderRoot(Slot root)
        {
            var crashedNodes = root.GetComponentsInChildren((LogixNode node) => !node.Enabled);

            crashedNodes.ForEach((LogixNode n) =>
            {
                n.Enabled = true;
            });

            NeosMod.Msg($"Found {crashedNodes.Count} crashed nodes under {root}");
            if (LogixUtils.config.GetValue(LogixUtils.ReportCrashedNodeRepair))
            { // Generate report
                var stringBuilder = new StringBuilder($"Found {crashedNodes.Count} crashed nodes under <b>{root.Name} > {root.Parent.Name}</b>");

                root.RunInUpdates(5, () =>
                { // Wait some arbitrary updates before checking if failed
                    Slot report = root.LocalUserSpace.AddSlot("Logix Repair Report", false);
                    report.PositionInFrontOfUser();

                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("<b><color=#f00>Crashed Nodes:</color></b>");

                    var sortedNodes = crashedNodes.OrderBy(n => n.Enabled); // Put all failed first

                    sortedNodes.Do(n =>
                    {
                        color color = n.Enabled ? color.Green : color.Red;
                        color white = color.White;
                        color = MathX.Lerp(color, white, 0.5f);

                        stringBuilder.AppendLine(string.Format("<color={0}>{1}</color> | {2} ({5}) > {3} > {4}", new object[]
                        {
                            color.ToHexString(false, "#"),
                            n.Enabled ? "Repaired" : "Failed",
                            FullTypeName.GetFormattedName(n.GetType()),
                            n.Slot.Parent.Name,
                            n.Slot.Parent.Parent.Name,
                            n.ReferenceID
                        }));
                    });


                    UniversalImporter.SpawnText(report, stringBuilder.ToString(), new color(0.8f, 0.8f, 0.8f, 0.5f), 10f);
                });
                
            }
        }
    }
}
