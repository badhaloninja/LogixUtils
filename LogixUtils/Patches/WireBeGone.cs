using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.UIX;
using HarmonyLib;
using System.Reflection;

namespace LogixUtils
{
    class WireBeGone : ToggleablePatch
    {
        public override void Patch(Harmony harmony, LogixUtils mod)
        {
            var onAttachMethod = typeof(LogixInterfaceProxy).GetMethod("GetInterface", BindingFlags.Public | BindingFlags.Instance);

            var wireBeGonePatchMethod = typeof(WireBeGone).GetMethod("wireBeGonePatch");

            harmony.Patch(onAttachMethod, postfix: new HarmonyMethod(wireBeGonePatchMethod));
        }

        public override void Unpatch(Harmony harmony, LogixUtils mod)
        {
            var onAttachMethod = typeof(LogixInterfaceProxy).GetMethod("GetInterface", BindingFlags.Public | BindingFlags.Instance);

            var wireBeGonePatchMethod = typeof(WireBeGone).GetMethod("wireBeGonePatch");

            harmony.Unpatch(onAttachMethod, wireBeGonePatchMethod);
        }

        public static void wireBeGonePatch(Slot __result)
        {
            Slot toggleRoot = __result[0]?.Find("WireToggle");
            if (toggleRoot != null) return;

            var wire = __result.GetComponentInChildren<ConnectionWire>(c => c.Slot.Name.Equals("LinkPoint"));
            if (wire == null) return;


            UIBuilder ui = new UIBuilder(wire.Slot.Parent);


            var toggleButton = ui.Button("");
            toggleButton.Slot.Name = "WireToggle";
            toggleButton.RectTransform.OffsetMax.Value = new BaseX.float2(-122, 0);
            
            toggleButton.LabelTextField.DriveFromBool(wire.Slot.ActiveSelf_Field, "⦿", "◌");
            toggleButton.Slot.AttachComponent<ButtonToggle>().TargetValue.TrySet(wire.Slot.ActiveSelf_Field);
            wire.Slot.ActiveSelf_Field.GetUserOverride(true).Default.Value = false;

            wire.Slot.Parent = toggleButton.Slot;
        }
    }
}