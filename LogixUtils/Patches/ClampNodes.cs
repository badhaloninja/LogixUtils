using FrooxEngine;
using FrooxEngine.LogiX;
using HarmonyLib;
using System.Reflection;

namespace LogixUtils
{
    class ClampNodes : ToggleablePatch
    {
        public override void Patch(Harmony harmony, LogixUtils mod)
        {
            var drivers = typeof(LogixTip).GetMethod("AttachDriver", BindingFlags.NonPublic | BindingFlags.Instance);
            var refnode =  typeof(ReferenceNode).GetMethod("PrefixReferenceNode", BindingFlags.Static | BindingFlags.NonPublic);


            var clampDrive = typeof(ClampNodes).GetMethod("driveNode");
            var clampRef = typeof(ClampNodes).GetMethod("refNode");


            harmony.Patch(drivers, postfix: new HarmonyMethod(clampDrive));
            harmony.Patch(refnode, postfix: new HarmonyMethod(clampRef));
        }
        public override void Unpatch(Harmony harmony, LogixUtils mod)
        {
            var drivers = typeof(LogixTip).GetMethod("AttachDriver", BindingFlags.NonPublic | BindingFlags.Instance);
            var refnode = typeof(ReferenceNode).GetMethod("PrefixReferenceNode", BindingFlags.Static | BindingFlags.NonPublic);

            var clampDrive = typeof(ClampNodes).GetMethod("driveNode");
            var clampRef = typeof(ClampNodes).GetMethod("refNode");


            harmony.Unpatch(drivers, clampDrive);
            harmony.Unpatch(refnode, clampRef);
        }

        public static void refNode(ref IReferenceNode __result)
        {
            if (__result == null) return;
            __result.World.GetSharedComponentOrCreate("LogiX_RefNodeTexture", delegate (StaticTexture2D tex)
            {
                tex.URL.Value = NeosAssets.Testing.Logix.Reference;
            }).WrapMode = TextureWrapMode.Clamp;
        }


        public static void driveNode(ref IDriverNode __result)
        {
            if (__result == null) return;
            __result.World.GetSharedComponentOrCreate("LogiX_DriverNodeTexture", delegate (StaticTexture2D tex)
            {
                tex.URL.Value = NeosAssets.Testing.UI.Driver;
            }).WrapMode = TextureWrapMode.Clamp;
        }


/*        // Make RefNode Texture Clamp
        [HarmonyPatch(typeof(ReferenceNode), "PrefixReferenceNode")]
        class ReferenceNodeTexture_Clamp
        {
            public static void Postfix(ref IReferenceNode __result)
            { // Add Drive Node
                if (__result == null) return;
                __result.World.GetSharedComponentOrCreate("LogiX_RefNodeTexture", delegate (StaticTexture2D tex)
                {
                    tex.URL.Value = NeosAssets.Testing.Logix.Reference;
                }).WrapMode = TextureWrapMode.Clamp;
            }
        }*/
    }
}
