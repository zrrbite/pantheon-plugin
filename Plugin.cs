﻿using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using PantheonPersist;

namespace PantheonPlugin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public Harmony Harmony { get; } = new("pantheon");
    internal static new ManualLogSource Log;

    public override void Load()
    {
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        // [Info   :   BepInEx] 1 plugin to load
        // [Info   :   BepInEx] Loading [Pantheon Plugin 1.3.7]
        // [Info   :Pantheon Plugin] Plugin PantheonPlugin is loaded!
        // [Info   :Pantheon Plugin] Patched function NetworkStart
        // [Info   :Pantheon Plugin] Patched function NetworkStop
        // [Message:   BepInEx] Chainloader startup complete

        Harmony.PatchAll();
        foreach (var method in Harmony.GetPatchedMethods())
            Log.LogInfo($"Patched function {method.Name}");
    }

    [HarmonyPatch]
    internal class PlayerNetwork
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EntityPlayerGameObject), nameof(EntityPlayerGameObject.NetworkStart))]
        private static void NetworkStart(EntityPlayerGameObject __instance)
        {
            // [Info   :Pantheon Plugin] NetworkStart, got Id = 1
            // [Info   :Pantheon Plugin] NetworkStart, got Id = 114979
            Log.LogInfo("NetworkStart, got Id: " + __instance.NetworkId.Value);

            if (__instance.NetworkId.Value == 1)
                return;
            
            // check if network id is the same as player id
            if (__instance.NetworkId.Value != EntityPlayerGameObject.LocalPlayerId.Value)
                return;
            
            // do something with __instance which is an instance of EntityPlayerGameObject

            // [Info   :Pantheon Plugin] Got player
            // [Info   :Pantheon Plugin] name: Inolwecool
            // [Info   :Pantheon Plugin] level: 1
            // [Info   :Pantheon Plugin] race: Human
            // [Info   :Pantheon Plugin] class: Warrior
            // [Info   :Pantheon Plugin] current health: 35.49179
            // [Info   :Pantheon Plugin] current max health: 35.491787
            // [Info   :Pantheon Plugin] current breath: 100
            // [Info   :Pantheon Plugin] current max breath: 100

            Log.LogInfo($"Got player");
            Log.LogInfo($"name: {__instance.info.DisplayName}");
            Log.LogInfo($"level: {__instance.Experience.Level}");
            Log.LogInfo($"race: {__instance.info.Race.ToString()}");
            Log.LogInfo($"class: {__instance.info.Class.ToString()}");

            // __instance.Pools = health, mana, focus etc.
            Log.LogInfo($"current health: {__instance.Pools.GetCurrent(PoolType.Health)}");
            Log.LogInfo($"current max health: {__instance.Pools.GetMax(PoolType.Health)}");
            Log.LogInfo($"current breath: {__instance.Pools.GetCurrent(PoolType.Breath)}");
            Log.LogInfo($"current max breath: {__instance.Pools.GetMax(PoolType.Breath)}");
        }

        [HarmonyPatch(typeof(CharacterMover), nameof(CharacterMover.SetMoveSpeedMultiplier), [typeof(float)])]
        public static class CharacterMoverPatch
        {
            public static void Prefix(ref float moveSpeedMultiplier)
            {
                Log.LogInfo("SetMoveSpeedMultiplier before: " + moveSpeedMultiplier);
                moveSpeedMultiplier = 2f;
                Log.LogInfo("SetMoveSpeedMultiplier after: " + moveSpeedMultiplier);
            }
        }
  
/*
        [HarmonyPatch(typeof(StatCombinationFormulas), nameof(StatCombinationFormulas.CalculateAttackSpeed), [typeof(float)])]
        public static class AttackSpeedPatch
        {
            public static void Prefix(ref float attackSpeed)
            {
                Log.LogInfo("SetAttackSpeed before: " + attackSpeed );
                attackSpeed = 200;
                Log.LogInfo("SetAttackSpeed after: " + attackSpeed );
            }
        }        
*/

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EntityPlayerGameObject), nameof(EntityPlayerGameObject.NetworkStop))]
        private static void NetWorkStop(EntityPlayerGameObject __instance)
        {
            Log.LogInfo("Network stop");
        }
    }
}
