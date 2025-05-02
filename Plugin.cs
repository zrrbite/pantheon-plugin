using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using PantheonPersist;
//using ViNL;
using UnityEngine;
using System;
using System.IO;
using STimers = System.Timers;    // alias so “Timer” is unambiguous
using System.Text.RegularExpressions;

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
    static Equipment.Logic GlobalEquipment;
    static float SpeedMult = 1;
    static bool Fly = false;
    
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
                
                if (GUI.Button(new Rect(20, 260, 100, 30), "+0.5 speed mult"))
                {
                    try
                    {
                        //Log.LogInfo("I'm rich! +10 gold to Bank and Stash");
                        //LocalPlayer.BankCurrency.Add(new Currency().Add(CurrencyType.Gold, 10));
                        //LocalPlayer.StashCurrency.Add(new Currency().Add(CurrencyType.Gold, 10));
                        // LogicalGraphNodes.AttackSpeedCalculator.CalculateModifiedSpeed(EntityPlayerGameObject.LocalPlayer, 20);
                        Log.LogInfo("+0.5x speed.");
                        SpeedMult = SpeedMult + 0.5f;
                    }
                    catch (Exception ex)
                    {
                        Log.LogInfo("Ex: " + ex.Message);
                    }
                }    
                if (GUI.Button(new Rect(20, 290, 100, 30), "-0.5 speed mult"))
                {
                    try
                    {
                        Log.LogInfo("-0.5x speed.");
                        SpeedMult = SpeedMult - 0.5f;                       
                    }
                    catch (Exception ex)
                    {
                        Log.LogInfo("Ex: " + ex.Message);
                    }
                }         
                if (GUI.Button(new Rect(20, 310, 100, 30), "Fly Mode"))
                {
                    try
                    {
                        Fly = !Fly;
                        Log.LogInfo("Fly mode toggled");              
                    }
                    catch (Exception ex)
                    {
                        Log.LogInfo("Ex: " + ex.Message);
                    }
                }
                if (GUI.Button(new Rect(20, 340, 100, 30), "Something exp"))
                {
                    try
                    {
                        Log.LogInfo("xp");

                    }
                    catch (Exception ex)
                    {
                        Log.LogInfo("Ex: " + ex.Message);
                    }
                }                                            
//scp -r deck@192.168.86.42:'/run/media/mmcblk0p1/users/steamuser/Documents/My Games/Pantheon/App/BepInEx/interop/' interop_new
/*                if (GUI.Button(new Rect(20, 290, 100, 30), "Become GM"))
                {
                    try
                    {
                        Log.LogInfo("Become GM");
                        LocalPlayer.info.AccessLevel = AccessLevel.GameMaster;
                        Vector3 v = new Vector3(0,0,0);
                        
                        //EntityClientMessaging.Logic.SendTeleportEvent(v);
                        //EntityClientMessaging.TeleportPosition(v);
                    }
                    catch (Exception ex)
                    {
                        Log.LogInfo("Ex: " + ex.Message);
                    }
                }  
*/                
                /*
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
                }*/                                
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
        private static readonly string LocFile = "loc";
        private static readonly System.Random Rng = new System.Random();
    //    private static STimers.Timer _timer;
                // 3) Method group / named method
        void PrintItem(Item it)
        {
            Log.LogInfo("Item: " + it.ToString());
        }

        private static void OnTick(object sender, STimers.ElapsedEventArgs e)
        {
            var x = (int)Math.Round(LocalPlayer.Position.x); // Rng.Next(-2000, 2001);
            var y = (int)Math.Round(LocalPlayer.Position.y); //Rng.Next(-2000, 2001);
            //replace with loc from game.
            File.WriteAllText(LocFile, $"{x}.{y}");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Wrote {x}.{y}");
        }

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
            Log.LogInfo($"Access: {__instance.info.AccessLevel.ToString()}");

            // __instance.Pools = health, mana, focus etc.
            Log.LogInfo($"current health: {__instance.Pools.GetCurrent(PoolType.Health)}");
            Log.LogInfo($"current max health: {__instance.Pools.GetMax(PoolType.Health)}");
            Log.LogInfo($"current breath: {__instance.Pools.GetCurrent(PoolType.Breath)}");
            Log.LogInfo($"current max breath: {__instance.Pools.GetMax(PoolType.Breath)}");
            // Set Equipment
            Equipment.Logic e = __instance.Equipment; // 46906
            GlobalEquipment = __instance.Equipment;

            // Start timer
//            _timer = new STimers.Timer(5000);
//            _timer.Elapsed += OnTick;
//            _timer.AutoReset = true;
//            _timer.Enabled   = true;      // starts it immediately

            // modifiers
//            __instance.Modifiers

/*
public enum EquipSlotType : byte
{
    Head,
    Wrist,
    LeftEar,
    RightEar,
    Neck,
    HarvestingTool,
    Chest,
    Back,
    SkinningTool,
    FishingRod,
    LightSource,
    Hands,
    LeftFinger,
    RightFinger,
    Waist,
    Legs,
    Feet,
    PrimaryHand,
    SecondaryHand,
    Ranged,
    Ammo,
    Relic,
    HeadGlyph,
    ChestGlyph,
    ArmGlyph,
    HandGlyph,
    LegGlyph,
    FootGlyph,
    CraftingTool,
    MiningTool,
    WoodcuttingTool,
    Shoulders,
    ProfessionHead,
    ProfessionChest,
    ProfessionHands,
    ProfessionLegs,
    ProfessionFeet,
    Max
}
public enum StatModifierType
{
    None,
    Additive,
    Multiplicative
}
// Namespace: 
public enum StatType // TypeDefIndex: 17296
{
	// Fields
	public byte value__; // 0x0
	public const StatType Strength = 0;
	public const StatType Stamina = 1;
	public const StatType Agility = 2;
	public const StatType Dexterity = 3;
	public const StatType Constitution = 4;
	public const StatType Wisdom = 5;
	public const StatType Intellect = 6;
	public const StatType Charisma = 7;
	public const StatType MagicResistance = 8;
	public const StatType FireResistance = 9;
	public const StatType ColdResistance = 10;
	public const StatType PoisonResistance = 11;
	public const StatType DiseaseResistance = 12;
	public const StatType ShockResistance = 13;
	public const StatType CurseResistance = 14;
	public const StatType DivineResistance = 15;
	public const StatType ChemicalResistance = 16;
	public const StatType NatureResistance = 17;
	public const StatType Armor = 18;
	public const StatType Health = 19;
	public const StatType Mana = 20;
	public const StatType Endurance = 21;
	public const StatType Awareness = 22;
	public const StatType Concealment = 23;
	public const StatType AttackPower = 24;
	public const StatType SpellPower = 25;
	public const StatType PhysicalCritChance = 26;
	public const StatType SpellCritChance = 27;
	public const StatType HitPercent = 28;
	public const StatType DodgePercent = 29;
	public const StatType BlockPercent = 30;
	public const StatType ParryPercent = 31;
	public const StatType HitRating = 32;
	public const StatType DodgeRating = 33;
	public const StatType BlockRating = 34;
	public const StatType ParryRating = 35;
	[Description("Health Regeneration")]
	public const StatType HealthRegen = 36;
	[Description("Mana Regeneration")]
	public const StatType ManaRegen = 37;
	public const StatType HastePercent = 38;
	public const StatType HasteRating = 39;
	public const StatType CooldownReductionPercent = 40;
	public const StatType BlockValue = 41;
	[Description("Attack Speed")]
	public const StatType AttackSpeedPercent = 42;
	[Description("Movement Speed")]
	public const StatType MoveSpeedPercent = 43;
	public const StatType ArmorPercentDamageReduction = 44;
	public const StatType ModifiedAggroRange = 45;
	public const StatType FrigidEnvironmentResistance = 46;
	public const StatType ScorchingEnvironmentResistance = 47;
	public const StatType ToxicEnvironmentResistance = 48;
	public const StatType WindShearEnvironmentResistance = 49;
	public const StatType PressureEnvironmentResistance = 50;
	public const StatType AnaerobicEnvironmentResistance = 51;
	public const StatType CrowdControlDamageResilience = 52;
	public const StatType AdditionalCounterAttackPercent = 53;
	public const StatType PhysicalCritRating = 55;
	public const StatType ModifiedAssistRange = 56;
	[Description("Sprinting Speed")]
	public const StatType ModifiedSprintSpeedPercent = 57;
	public const StatType ConcentrationRating = 58;
	public const StatType ConcentrationChance = 59;
	public const StatType PenetrationRating = 60;
	public const StatType PenetrationPercent = 61;
	[Description("Health Recovered While Resting")]
	public const StatType RestingHealth = 70;
	[Description("Mana Recovered While Resting")]
	public const StatType RestingMana = 71;
	public const StatType CombatTraining = 72;
	public const StatType WeaponTraining = 73;
	public const StatType CastingTraining = 74;
	public const StatType PhysicalTraining = 75;
	public const StatType LearningRate = 76;
	[Description("Readiness Generation")]
	public const StatType ExtraReadinessGeneration = 79;
	[Description("Climbing Speed")]
	public const StatType ModifiedClimbSpeedPercent = 90;
	[Description("Swimming Speed")]
	public const StatType ModifiedSwimSpeedPercent = 92;
	public const StatType Breath = 94;
	public const StatType ShamanBuffPotency = 98;
	public const StatType ShamanBuffExtraDuration = 99;
	public const StatType ShamanGripLinePotency = 100;
	public const StatType DireLordDevourPotency = 101;
	public const StatType DireLordEssenceHarvesterPotency = 102;
	public const StatType DireLordLifeTapPotency = 103;
	public const StatType ShamanSlowLinePotency = 104;
	public const StatType SpellCritRating = 105;
	public const StatType MaxCarryWeight = 106;
	public const StatType Max = 107;
}
*/

// Equipment.Logic
// 	// RVA: 0xBA9860 Offset: 0xBA8A60 VA: 0x180BA9860
// 	public void ForEach(Action<Item> callback) { }

// StatModifiers (85987)
// Item (48127)
// ItemTemplate 86040
            // For all equipped items, if IsWeapon, something about statModifiers (85987)
            // public void set_Stat(StatType value) { }
            // public void set_ModifierType(StatModifierType value) { }
            Item item_0 = e.GetEquippedItem(EquipSlotType.PrimaryHand); //. EquipSlotType
            Item item_1 = e.GetEquippedItem(EquipSlotType.Chest); //. EquipSlotType
            Item item_2 = GlobalEquipment.GetEquippedItem(EquipSlotType.Legs); //. EquipSlotType

//            InventoryHelperEntity.Info = LocalPlayer;

            Log.LogInfo("Item: " + item_0.ToString());
            Log.LogInfo("id: " + item_0.Template.ItemId);
            Log.LogInfo("Item: " + item_1.ToString());            
            Log.LogInfo("Global Item: " + item_2.ToString());    

            item_0.Template.StatModifiers = new StatModifier[1];

            // 2) Create the modifier via its ctor:
            var strengthMod = new StatModifier(
                StatModifierType.Additive,  // modifierType  
                50f,                        // amount  
                StatType.Strength           // stat  
            );

             item_0.Template.StatModifiers[0] = strengthMod;
             //Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.ValueTuple<StatType, Stat.Modifier>> modifiers = item_0.Internal_GetStatModifiers();
             
/*
            item_0.Template.Delay = 0.1f;
            item_0.Template.MaxDamage = 400;
            item_0.Template.ItemName = "Uber Weapon";
            item_0.Template.CoinValue = 1000;
            Log.LogInfo("Item 0 delay: " + item_0.Template.Delay);
          

            item_0.Template.EquipBuffId = 10;
            item_0.Template.ItemWeight = 0.5f;
            //ItemId ? might impact weight

 */                       

            //item_0.Template.ItemId = 14055; //some rusty sword
            // 3) Stick it in the Strength slot (Strength == 0)
           
            
            Action<Item> callback = itm =>  Log.LogInfo(itm.ToString());
            e.ForEach(callback);

            //Action<Item> callback2 = itm =>  Log.LogInfo("weapon: " + itm.IsWeapon());
            //e.ForEach(callback2);
        }

        [HarmonyPatch(typeof(CharacterMover), nameof(CharacterMover.SetGravityMultiplier), [typeof(float)])]
        public static class CharacterGravityPatch
        {
            public static void Prefix(ref float gravityMultiplier)
            {
                Log.LogInfo("SetGravityMultiplier before: " + gravityMultiplier);
                gravityMultiplier = 3.0f;
                Log.LogInfo("SetGravityMultiplier after: " + gravityMultiplier);
            }
        }

        [HarmonyPatch(typeof(CharacterMover), nameof(CharacterMover.SetMoveSpeedMultiplier), [typeof(float)])]
        public static class CharacterMoverPatch
        {
            public static void Prefix(ref float moveSpeedMultiplier)
            {
                Log.LogInfo("SetMoveSpeedMultiplier before: " + moveSpeedMultiplier);
                //moveSpeedMultiplier = 2.5f;
                moveSpeedMultiplier = SpeedMult;
                Log.LogInfo("SetMoveSpeedMultiplier after: " + moveSpeedMultiplier);

                //TODO
                // worldtometers. Go in and measure
                //  Sort by distance?
                // 


                //Also check for GM?
                /* 
                    public bool IsDev { get; }
                    public bool IsGM { get; set; }
                    public bool GMImmortal { get; set; }
                    public bool GMCanBeAggroed { get; set; }
                    public bool GMInvisible { get; set; }
                    public GMFlags GMFlags { get; set; }
                    public SocialFlags SocialFlags { get; set; }
                    public string DefaultDisplayName { get; set; }
                */

                foreach (BaseEntityGameObject entity in GameObject.FindObjectsOfType<EntityGameObject>())
                {
                    string entity_str = Regex.Replace(entity.ToString(), @"\s+\(NetworkId\(\d+\)\)$", "");
                    var unitsPerMeter = 2.0f;
                    var metersPerUnit = 1.0f / unitsPerMeter;

                    float WorldUnitsToMeters(float units) => units * metersPerUnit;
                    //float MetersToWorldUnits(float meters) => meters * unitsPerMeter;

                    Vector3 mpos = entity.Position;
                    Vector3 mypos = LocalPlayer.Position;
                    Vector3 diff = mpos - mypos;

                    Vector3 toMonster = (entity.Transform.position - LocalPlayer.Transform.position).normalized;
                    Vector3 forward = LocalPlayer.Transform.forward;
                    Vector3 right = LocalPlayer.Transform.right;

                    // Angle between forward and toMonster
                    float dotForward = Vector3.Dot(forward, toMonster);   // front vs back
                    float dotRight = Vector3.Dot(right, toMonster); 
                    string heading = "";
                    float angleThreshold = Mathf.Cos(45f * Mathf.Deg2Rad);  // = 0.7071...

                    if (dotForward > angleThreshold)
                    {
                        heading = " in front of you.";
                    }
                    else if (dotForward < -angleThreshold)
                    {
                        heading = " behind you.";
                    }
                    else if (dotRight > 0)
                    {
                        heading = " to the right of you.";
                    }
                    else
                    {
                        heading = " to the left of you.";
                    }

                    //Log.LogInfo(entity.ToString()); // Log everything
                      Log.LogInfo(entity_str + " - " + Mathf.Round(WorldUnitsToMeters(Vector3.Distance(mpos, mypos)) * 100f) / 100f + "m" + heading);
//                      if (dot > Mathf.Cos(30 * Mathf.Deg2Rad))  // 60° field of view (±30°)
//                        {
//                            Debug.Log("It is in front of you!");
//                        }

                    if(entity.Info.IsGM || entity.Info.IsDev)
                    {
                        Log.LogInfo("!!!! WARNING!!!! GM/Dev: " + entity.Info.DefaultDisplayName + " detected at " +  entity.Position.ToString() + ". Flags = " + entity.Info.GMFlags.ToString() + ". Invisible = " + entity.Info.GMInvisible);
                    }
                }
            }
        }

/// <summary>
/// Teleport. This needs to be patched in EntityClientMessaging. Then NetworkStart() can cal 	public EntityClientMessaging.Logic GetLogic() { }
/// 
/// Teleportposition is an RPC event
/// [Rpc]
// RVA: 0xBE4FA0 Offset: 0xBE41A0 VA: 0x180BE4FA0
//	private void TeleportPosition(Vector3 position) { }
/// </summary>
/// 

        [HarmonyPatch(typeof(EntityClientMessaging.Logic), nameof(EntityClientMessaging.Logic.SendTeleportEvent), [typeof(Vector3)])]
        public static class TeleportPatch
        {
            public static void Prefix(ref float endPosition)
            {
                Log.LogInfo("Teleport event!!!1!!1" + endPosition);
            }
        }

/*
        [HarmonyPatch(typeof(Experience), nameof(Experience.SetExperienceFromServer))]
        public static class xppatch
        {
            public static void Prefix(ref Peer _, ref int totalExperience)
            {
                Log.LogInfo("totalExperience " + totalExperience);
                int level = LocalPlayer.Experience.Level;
                int required_exp = Experience.Logic.CalculateExperienceRequiredToReachLevel(level + 1);
                totalExperience = required_exp;
            }
        }

        [HarmonyPatch(typeof(Pools), nameof(Pools.__RpcMethods), [typeof(Pools), typeof(NetworkObject)])]
        public static class poolspatch
        {
            public static void Prefix(ref Pools __target, NetworkObject networkObject)
            {
                Log.LogInfo("Pools patch ");
                //__target.PoolChangedRpc()
            }
        }        
*/
        // -------------------------------
        // Stat Formulas
        // To try:
        //  StatFormulas.CalculateDamagePerStrength
        //  public static float CalculateDodge(float dodgeRating) { }
        // 
        // Do the "register" spells just fire once?
        // 
        // -------------------------------

// todo: 	public EntityMultipliers.Logic Multipliers { get; set; } : 43284. Might be readonly

        // This hook is hit, sometimes
        [HarmonyPatch(typeof(StatFormulas), nameof(StatFormulas.ModifyValueByHastePercent))]
        public static class HasteValuePatch
        {
            public static void Prefix(ref float value, ref float hastePercent)
            {
                Log.LogInfo("Haste percent before: " + hastePercent + ". value " + value);
                hastePercent = 500f;
                Log.LogInfo("Haste percent after: " + hastePercent + ". value " + value);
            }
        }

        // This hook is hit!
        // public unsafe void SetIsFlying([DefaultParameterValue(null)] bool isFlying)      
        [HarmonyPatch(typeof(CharacterMover), nameof(CharacterMover.SetIsFlying), [typeof(bool)])]
        public static class FlyPatch
        {
            public static void Prefix(ref bool isFlying)
            {
                isFlying = Fly;
                if(isFlying)
                    Log.LogInfo("Look mom, i can fly!");
                else
                    Log.LogInfo("Cant fly anymore.");
            }
        }        

        // -------------------------------
        // Equipment Logic
        // Equipment.Logic GetEquippedItem(EquipSlotType equipSlotType) { }
        // -------------------------------

        // Hmm, doesnt make sense to hook this
        // public unsafe void SetIsFlying([DefaultParameterValue(null)] bool isFlying)      
/*        [HarmonyPatch(typeof(Equipment.Logic), nameof(Equipment.Logic.GetEquippedItem), [typeof(EquipSlotType)])]
        public static class EquipPatch
        {
            public static void Prefix(ref EquipSlotType equipSlotType)
            {
                Log.LogInfo("Item " + equipSlotType.ToString());
            }
        }  
*/
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
                Log.LogInfo("ItemTemplate modddd");
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
