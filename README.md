# LogixUtils

A [NeosModLoader](https://github.com/zkxs/NeosModLoader) mod for [Neos VR](https://neos.com/) that tweaks some logix things

All features are toggleable in the config, most are on by default

### Current features: 
 - Scales relays, drives, casts, and reference nodes relative to user
 - Updates `LogixHelper.GetNodeName` to display generic type names nicer
 - Allows aligning logix backwards with the UI_TargettingController
 - Changes Extract Ref Node to allow any refrence instead of only IField
 - Allows spawning a Value/Reference register from a Write node target
 - Sets various logix node textures to clamp
 - Adds a repair crashed nodes contex menu item to the logix tip when holding a slot with crashed nodes under it

## Installation
1. Install [NeosModLoader](https://github.com/zkxs/NeosModLoader).
1. Place [LogixUtils.dll](https://github.com/badhaloninja/LogixUtils/releases/latest/download/LogixUtils.dll) into your `nml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods` for a default install. You can create it if it's missing, or if you launch the game once with NeosModLoader installed it will create the folder for you.
1. Start the game. If you want to verify that the mod is working you can check your Neos logs.
