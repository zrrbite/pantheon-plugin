﻿using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace PantheonPlugin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        Log.LogInfo($"MyPlugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}
