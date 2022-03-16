using System;
using System.Collections.Generic;
using FrooxEngine;
using BaseX;
using HarmonyLib;
using FrooxEngine.LogiX;
using NeosModLoader;

namespace LogixUtils
{
    class InputNodes : ToggleablePatch
    { // Fix existing input nodes not being defined as input nodes

        public override void Initialize(Harmony harmony, LogixUtils mod, ModConfiguration config) {
            addOrRemoveInputnodes(unusedInputs, config.GetValue<bool>(LogixUtils.AddInputNodes));
            addOrRemoveInputnodes(newUtilInputs, config.GetValue<bool>(LogixUtils.AddOtherInputNodes));
        }
        public override void OnThisConfigurationChanged(ConfigurationChangedEvent configurationChangedEvent) {
            if (configurationChangedEvent.Key == LogixUtils.AddInputNodes)
            {
                addOrRemoveInputnodes(unusedInputs, configurationChangedEvent.Config.GetValue<bool>(LogixUtils.AddInputNodes));
                return;
            }

            if (configurationChangedEvent.Key == LogixUtils.AddOtherInputNodes)
            {
                addOrRemoveInputnodes(newUtilInputs, configurationChangedEvent.Config.GetValue<bool>(LogixUtils.AddOtherInputNodes));
                return;
            }
        }

        public static void addOrRemoveInputnodes(Dictionary<Type, Type> nodes, bool addNode)
        {
            Dictionary<Type, Type> inputNodes = Traverse.Create(typeof(DefaultNodes)).Field("inputNodes").GetValue<Dictionary<Type, Type>>();
            if (addNode)
            {
                foreach (var item in nodes)
                {
                    if (inputNodes.ContainsKey(item.Key)) continue;
                    inputNodes.Add(item.Key, item.Value);
                }
                return;
            }
            foreach (var item in nodes)
            {
                if (!inputNodes.ContainsKey(item.Key)) continue;
                inputNodes.Remove(item.Key);
            }
        }

        private static Dictionary<Type, Type> unusedInputs = new Dictionary<Type, Type>() {
              { typeof(IAssetProvider<AudioClip>), typeof(FrooxEngine.LogiX.Audio.AudioClipInput) },
              { typeof(TimeSpan), typeof(FrooxEngine.LogiX.Input.TimeSpanInput) },
              { typeof(bobool3ol), typeof(FrooxEngine.LogiX.Input.Bobool3ol) }, // :P
            };
        private static Dictionary<Type, Type> newUtilInputs = new Dictionary<Type, Type>() {
              { typeof(DateTime), typeof(FrooxEngine.LogiX.Input.UtcNowNode) },
              { typeof(User), typeof(FrooxEngine.LogiX.WorldModel.LocalUser) },
              { typeof(Slot), typeof(FrooxEngine.LogiX.WorldModel.LocalUserSpace) }
            };
    }
}