using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using PantheonPersist;
using UnityEngine;
using System;

//todo:
//public unsafe static float GetBonusSpellDamageFromSpellPower([DefaultParameterValue(null)] float spellPower)
//GetBonusHealingFromSpellPower
//StatFormulas.ModifyValueByHastePercent
namespace PantheonPlugin;


[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public Harmony Harmony { get; } = new("pantheon");
    internal static new ManualLogSource Log;
    static EntityPlayerGameObject LocalPlayer;

        public class Hmm : MonoBehaviour
        {
            private void Start()
            {
                Log.LogInfo("In Hmm:Start()");
                Log.LogInfo("Experience to level 10: " + Experience.Logic.CalculateExperienceRequiredToReachLevel(10));
            }

            private void OnUpdate() { }

            private void OnGUI()
            {
                GUI.Box(new Rect(10, 200, 100, 90), "Some menu");

                // Add more buttons
                if (GUI.Button(new Rect(20, 230, 100, 30), "+1 Level"))
                {
                    try
                    {
                        Log.LogInfo("Ding!");
                        LocalPlayer.Experience.AddLevel(1, true);
                        LocalPlayer.Skills.MaxAllSkillLevels();
                    }
                    catch (Exception ex)
                    {
                        Log.LogInfo("Ex: " + ex.Message);
                    }
                }
                
                if (GUI.Button(new Rect(20, 260, 100, 30), "+10 Gold"))
                {
                    try
                    {
                        Log.LogInfo("I'm rich! +10 gold to Bank and Stash. And atk speed.");
                        LocalPlayer.BankCurrency.Add(new Currency().Add(CurrencyType.Gold, 10));
                        LocalPlayer.StashCurrency.Add(new Currency().Add(CurrencyType.Gold, 10));
//                        LogicalGraphNodes.AttackSpeedCalculator.CalculateModifiedSpeed(EntityPlayerGameObject.LocalPlayer, 20);
                    }
                    catch (Exception ex)
                    {
                        Log.LogInfo("Ex: " + ex.Message);
                    }
                }    
//scp -r deck@192.168.86.42:'/run/media/mmcblk0p1/users/steamuser/Documents/My Games/Pantheon/App/BepInEx/interop/' interop_new
                if (GUI.Button(new Rect(20, 290, 100, 30), "Almost level up"))
                {
                    try
                    {
                        Log.LogInfo("Bubbleeeeeeesssss");
                        int level = LocalPlayer.Experience.Level;
                        int required_exp = Experience.Logic.CalculateExperienceRequiredToReachLevel(level + 1);
                        LocalPlayer.Experience.SetExperience(required_exp - 1, false); // Juuust barely
                    }
                    catch (Exception ex)
                    {
                        Log.LogInfo("Ex: " + ex.Message);
                    }
                }                                
            }
        }
    // -----------------------------

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
        AddComponent<Hmm>();
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
//            __instance.Experience.AddLevel(1, true);
//            __instance.Skills.MaxAllSkillLevels();
            LocalPlayer = __instance;

            //__instance.Experience.AddLevel(1, true);
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

        // -------------------------------
        // Stat Formulas
        // To try:
        //  StatFormulas.CalculateDamagePerStrength
        //  public static float CalculateDodge(float dodgeRating) { }
        // 
        // Do the "register" spells just fire once?
        // 
        // -------------------------------

        // This hook is hit
        [HarmonyPatch(typeof(StatFormulas), nameof(StatFormulas.ModifyValueByHastePercent))]
        public static class HasteValuePatch
        {
            public static void Prefix(ref float value, ref float hastePercent)
            {
                Log.LogInfo("Haste percent before: " + hastePercent);
                hastePercent = 1000f;
                Log.LogInfo("Haste percent after: " + hastePercent);
            }
        }

/*        // This hook is hit. Seems to hang the game if there are two patches. 
        [HarmonyPatch(typeof(StatFormulas), nameof(StatFormulas.GetBonusSpellDamageFromSpellPower), [typeof(float)])]
        public static class BonusSpellDamagePatch
        {
            public static void Prefix(ref float __result)
            {
                __result = 10000f;
            }
        }
*/
/*
        [HarmonyPatch(typeof(StatFormulas), nameof(StatFormulas.CalculateDamagePerStrength), [typeof(float), typeof(float)])]
        public static class DamagePerStrengthPatch
        {
            public static void Prefix(ref float weaponDelay, ref float strength)
            {
                Log.LogInfo("weapondelay before: " + weaponDelay);
                Log.LogInfo("strength before: " + strength);

                weaponDelay = 0.1f;
                strength = 100f;

                Log.LogInfo("DamagePerStrength after: " + strength);
                Log.LogInfo("strength after: " + strength);                
            }
        }
*/

        // MIGHT WORK
/*        [HarmonyPatch(typeof(StatFormulas), nameof(StatFormulas.GetBonusHealingFromSpellPower), [typeof(float)])]
        public static class BonusHealingPatch
        {
            public static void Prefix(ref float spellPower)
            {
                spellPower = 100f;
            }
        }
*/


//  public unsafe static float GetExperienceMultiplier([DefaultParameterValue(null)] int killerLevel, [DefaultParameterValue(null)] int victimLevel)
//  public unsafe static float CalculateHastePercent([DefaultParameterValue(null)] float hasteRating)

/*        [HarmonyPatch(typeof(StatCombinationFormulas), nameof(StatCombinationFormulas.CalculateHastePercent))]
        public static class HastePatch
        {
            public static void Prefix(ref float hasteRating)
            {
                Log.LogInfo("HasteRating before: " + hasteRating );
                hasteRating = 200;
                Log.LogInfo("HasteRating after: " + hasteRating );
            }
        }    
*/
/*
        [HarmonyPatch(typeof(ConsiderTable), nameof(ConsiderTable.GetExperienceMultiplier))]
        public static class XpPatch
        {
            public static void Prefix(ref int killerLevel, ref int victimLevel)
            {
                Log.LogInfo("Killer level: " + killerLevel );
                killerLevel = 1;
                victimLevel = 20;
            }
        }  
*/            
/*
        [HarmonyPatch(typeof(StatCombinationFormulas), nameof(StatCombinationFormulas.CalculateAttackSpeed))]
        public static class AttackSpeedPatch
        {
            public static void Prefix(ref float hastePercent)
            {
                Log.LogInfo("SetAttackSpeed before: " + hastePercent );
                hastePercent = 200;
                Log.LogInfo("SetAttackSpeed after: " + hastePercent );
            }
        }
*/

//public unsafe void SetStealth([DefaultParameterValue(null)] bool isStealth)
//public unsafe static bool IsHealthLowEnoughToCauseDeath([DefaultParameterValue(null)] float health, [DefaultParameterValue(null)] float min)

  /*      [HarmonyPatch(typeof(CombatEffects), nameof(CombatEffects.SetStealth), [typeof(bool)])]
        public static class StealthPatch
        {
            public static void Prefix(ref bool isStealth)
            {
                Log.LogInfo(" Stealthy ");
                isStealth = true;
            }
        }
*/

/*
        [HarmonyPatch(typeof(HealthPool), nameof(HealthPool.IsHealthLowEnoughToCauseDeath), [typeof(float), typeof(float)])]
        public static class ImmortalPatch
        {
            public static void Prefix(ref float health, ref float min)
            {
                Log.LogInfo("Is health low enough: " + health );
                health = 200;
                min = 100; // hit required?
                Log.LogInfo("Health " + health );
            }
        }
"*

//        public unsafe void SetIsFlying([DefaultParameterValue(null)] bool isFlying)     
/*   
        [HarmonyPatch(typeof(CharacterMover), nameof(CharacterMover.SetIsFlying), [typeof(bool)])]
        public static class FlyPatch
        {
            public static void Prefix(ref bool isFlying)
            {
                Log.LogInfo("Look mom, i can fly!");
                isFlying = true;
            }
        }
*/
//public unsafe static float CalculateModifiedSpeed([DefaultParameterValue(null)] tIEnity entity, [DefaultParameterValue(null)] float baseSpeed)
        [HarmonyPatch(typeof(LogicalGraphNodes.AttackSpeedCalculator), nameof(LogicalGraphNodes.AttackSpeedCalculator.CalculateModifiedSpeed), [typeof(IEntity), typeof(float)])]
        public static class AtkSpeedPatch
        {
            public static void Prefix(ref IEntity entity, ref float baseSpeed)
            {
                Log.LogInfo("Recalc atk speed");
                baseSpeed = 100;
            }
        }
        //public unsafe float CalculateMaxDamage([DefaultParameterValue(null)] float bonusDamage, [DefaultParameterValue(null)] float weaponMultiplier)
        [HarmonyPatch(typeof(ItemTemplate), nameof(ItemTemplate.CalculateMaxDamage), [typeof(float), typeof(float)])]
        public static class MaxDmg
        {
            public static void Prefix(ref float bonusDamage, ref float weaponMultiplier)
            {
                Log.LogInfo("ItemTemplate mod");
                bonusDamage = 20;
                weaponMultiplier = 2;
            }
        }
/*
        [HarmonyPatch(typeof(ActiveBuff), nameof(ActiveBuff.Apply), [typeof(double)])]
        public static class ActiveBuffPatch
        {
            public static void Prefix(ref double time)
            {
                Log.LogInfo("Buff: " + time);
                time = 10000;
                Log.LogInfo("Buff: " + time);
            }
        }
*/

//public unsafe void BuffAddedToMeEvent([DefaultParameterValue(null)] double time, [DefaultParameterValue(null)] ref ActiveBuff activeBuff)
 /*       [HarmonyPatch(typeof(ActiveBuff), nameof(ActiveBuff.BuffAddedToMeEvent))]
        public static class BuffPatch
        {
            public static void Prefix(ref double time, ref ActiveBuff activeBuff)
            {
                Log.LogInfo("Buffs: " + time);
                time = 10000;
                Log.LogInfo("Buffs: " + time);                
            }
        }
*/

/*
        [HarmonyPatch(typeof(Experience.Logic), nameof(Experience.Logic.SetLevel), [typeof(int), typeof(bool), typeof(bool)])]
        public static class LevelUpPatch
        {
            public static void Prefix(ref int level, ref bool resetCurrentExperience, ref bool levelUpEvent)
            {
                Log.LogInfo("level up: " + level); //Keep level as it is?
                level = 10;
                resetCurrentExperience = false;
                levelUpEvent = false;
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
