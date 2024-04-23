// Project:         PhysicalCombatOverhaul mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2024 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    2/13/2024, 9:00 PM
// Last Edit:		4/22/2024, 11:30 PM
// Version:			1.50
// Special Thanks:  Hazelnut, Ralzar, and Kab
// Modifier:		

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Utility;
using System;
using DaggerfallConnect.FallExe;

namespace PhysicalCombatOverhaul
{
    public partial class PhysicalCombatOverhaulMain : MonoBehaviour
    {
        public static PhysicalCombatOverhaulMain Instance;

        static Mod mod;

        // Options
        public static bool archeryModuleCheck { get; set; }
        public static bool critStrikeModuleCheck { get; set; }
        public static bool fadingEnchantedItemsModuleCheck { get; set; }
        public static bool armorHitFormulaModuleCheck { get; set; }
        public static bool condBasedEffectModuleCheck { get; set; }
        public static bool softMatRequireModuleCheck { get; set; }

        // Mod Compatibility Check Values
        public static bool RolePlayRealismArcheryModuleCheck { get; set; }
        public static bool RalzarMeanerMonstersEditCheck { get; set; }

        // Global Variables
        public static readonly short[] weightMultipliersByMaterial = { 4, 5, 4, 4, 3, 4, 4, 2, 4, 5 };
        //public static ImmersiveFootstepsObject footstepComponent { get; set; }
        //public static AudioClip LastFootstepPlayed { get { return lastFootstepPlayed; } set { lastFootstepPlayed = value; } }

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<PhysicalCombatOverhaulMain>(); // Add script to the scene.

            mod.LoadSettingsCallback = LoadSettings; // To enable use of the "live settings changes" feature in-game.

            mod.IsReady = true;
        }

        private void Start()
        {
            Debug.Log("Begin mod init: Physical Combat And Armor Overhaul");

            Instance = this;

            mod.LoadSettings();

            ModCompatibilityChecking();

            FormulaHelper.RegisterOverride(mod, "DamageModifier", (Func<int, int>)DamageModifier);

            FormulaHelper.RegisterOverride(mod, "CalculateAttackDamage", (Func<DaggerfallEntity, DaggerfallEntity, bool, int, DaggerfallUnityItem, int>)CalculateAttackDamage);
            FormulaHelper.RegisterOverride(mod, "CalculateSwingModifiers", (Func<FPSWeapon, ToHitAndDamageMods>)CalculateSwingModifiers);
            FormulaHelper.RegisterOverride(mod, "CalculateProficiencyModifiers", (Func<DaggerfallEntity, DaggerfallUnityItem, ToHitAndDamageMods>)CalculateProficiencyModifiers);
            FormulaHelper.RegisterOverride(mod, "CalculateRacialModifiers", (Func<DaggerfallEntity, DaggerfallUnityItem, PlayerEntity, ToHitAndDamageMods>)CalculateRacialModifiers);
            FormulaHelper.RegisterOverride(mod, "CalculateWeaponToHit", (Func<DaggerfallUnityItem, int>)CalculateWeaponToHit);
            FormulaHelper.RegisterOverride(mod, "CalculateArmorToHit", (Func<DaggerfallEntity, int, int>)CalculateArmorToHit);
            FormulaHelper.RegisterOverride(mod, "CalculateAdrenalineRushToHit", (Func<DaggerfallEntity, DaggerfallEntity, int>)CalculateAdrenalineRushToHit);
            FormulaHelper.RegisterOverride(mod, "CalculateStatDiffsToHit", (Func<DaggerfallEntity, DaggerfallEntity, int>)CalculateStatDiffsToHit);
            FormulaHelper.RegisterOverride(mod, "CalculateSkillsToHit", (Func<DaggerfallEntity, DaggerfallEntity, int>)CalculateSkillsToHit);
            FormulaHelper.RegisterOverride(mod, "CalculateAdjustmentsToHit", (Func<DaggerfallEntity, DaggerfallEntity, int>)CalculateAdjustmentsToHit);
            FormulaHelper.RegisterOverride(mod, "CalculateWeaponAttackDamage", (Func<DaggerfallEntity, DaggerfallEntity, int, int, DaggerfallUnityItem, int>)CalculateWeaponAttackDamage);
            FormulaHelper.RegisterOverride(mod, "CalculateSuccessfulHit", (Func<DaggerfallEntity, DaggerfallEntity, int, int, bool>)CalculateSuccessfulHit);

            // Overridden Due To FormulaHelper.cs Private Access Modifiers, otherwise would not be included here.
            FormulaHelper.RegisterOverride(mod, "CalculateStruckBodyPart", (Func<int>)CalculateStruckBodyPart);
            FormulaHelper.RegisterOverride(mod, "CalculateBackstabChance", (Func<PlayerEntity, DaggerfallEntity, bool, int>)CalculateBackstabChance);
            FormulaHelper.RegisterOverride(mod, "CalculateBackstabDamage", (Func<int, int, int>)CalculateBackstabDamage);
            //FormulaHelper.RegisterOverride(mod, "GetBonusOrPenaltyByEnemyType", (Func<DaggerfallEntity, EnemyEntity, int>)GetBonusOrPenaltyByEnemyType);

            FormulaHelper.RegisterOverride(mod, "AdjustWeaponHitChanceMod", (Func<DaggerfallEntity, DaggerfallEntity, int, int, DaggerfallUnityItem, int>)AdjustWeaponHitChanceMod);
            FormulaHelper.RegisterOverride(mod, "AdjustWeaponAttackDamage", (Func<DaggerfallEntity, DaggerfallEntity, int, int, DaggerfallUnityItem, int>)AdjustWeaponAttackDamage);

            Debug.Log("Finished mod init: Physical Combat And Armor Overhaul");
        }

        private static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            //ReverseCycleDirection = mod.GetSettings().GetValue<bool>("GeneralSettings", "ReverseCycleDirections");
        }

        private void ModCompatibilityChecking()
        {
            /*
            // Better Ambience mod: https://www.nexusmods.com/daggerfallunity/mods/139
            Mod betterAmbience = ModManager.Instance.GetModFromGUID("d5655077-ba38-4dbc-a41f-2b358cb1d680");
            if (betterAmbience != null)
            {
                BetterAmbienceCheck = true;
                ModSettings betterAmbienceSettings = betterAmbience.GetSettings();
                BetterAmbienceFootstepsModuleCheck = betterAmbienceSettings.GetBool("Better Footsteps", "enable");
            }
            else { BetterAmbienceCheck = false; }
            */
        }

        private void Update()
        {
            if (GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress)
                return;

            // Handle mouse wheel
            float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
            if (mouseScroll != 0)
            {
                // sdf
            }
        }

        // -- Newly Added Stuff 4-17-2024 --

        /// <summary>'EnemyCombatProps' Struct for various combat related values and properties of an enemy.</summary>
        public struct ECP
        {
            public EnemyEntity entityObj;
            public MobileEnemy enemyData;
            public DFCareer enemyCareer;
            public DaggerfallUnityItem mainHandItem;
            public DaggerfallUnityItem offHandItem;
            public BodySize bodySize;
            public int strn;
            public int will;
            public int agil;
            public int endu;
            public int sped;
            public int luck;
        }

        /// <summary>'PlayerCombatProps' Struct for various combat related values and properties of the player.</summary>
        public struct PCP
        {
            public PlayerEntity entityObj;
            public DFCareer playerCareer;
            public BodySize bodySize;
            public int strn;
            public int will;
            public int agil;
            public int endu;
            public int sped;
            public int luck;
        }

        /// <summary>Fill in basic data for a new 'EnemyCombatProps' struct variable.</summary>
        public static ECP GetEnemyCombatProps(EnemyEntity enemy)
        {
            ECP ecp = new ECP();
            ecp.entityObj = enemy;
            ecp.enemyData = enemy.MobileEnemy;
            ecp.enemyCareer = enemy.Career;
            ecp.bodySize = GetCreatureBodySize(enemy);
            ecp.strn = enemy.Stats.LiveStrength;
            ecp.will = enemy.Stats.LiveWillpower;
            ecp.agil = enemy.Stats.LiveAgility;
            ecp.endu = enemy.Stats.LiveEndurance;
            ecp.sped = enemy.Stats.LiveSpeed;
            ecp.luck = enemy.Stats.LiveLuck;

            return ecp;
        }

        /// <summary>Fill in basic data for a new 'PlayerCombatProps' struct variable.</summary>
        public static PCP GetPlayerCombatProps(PlayerEntity player)
        {
            PCP pcp = new PCP();
            pcp.entityObj = player;
            pcp.playerCareer = player.Career;
            pcp.bodySize = GetPlayerBodySize(player);
            pcp.strn = player.Stats.LiveStrength;
            pcp.will = player.Stats.LiveWillpower;
            pcp.agil = player.Stats.LiveAgility;
            pcp.endu = player.Stats.LiveEndurance;
            pcp.sped = player.Stats.LiveSpeed;
            pcp.luck = player.Stats.LiveLuck;

            return pcp;
        }

        /// <summary>Return the size category of the provided creature.</summary>
        public static BodySize GetCreatureBodySize(EnemyEntity enemy)
        {
            if (enemy.EntityType == EntityTypes.EnemyClass)
            {
                return BodySize.Average;
            }
            else
            {
                switch (enemy.CareerIndex)
                {
                    case (int)MonsterCareers.Imp:
                        return BodySize.Tiny;
                    case (int)MonsterCareers.Rat:
                    case (int)MonsterCareers.GiantBat:
                    case (int)MonsterCareers.Spider:
                    case (int)MonsterCareers.Slaughterfish:
                        return BodySize.Small;
                    default:
                        return BodySize.Average;
                    case (int)MonsterCareers.GrizzlyBear:
                    case (int)MonsterCareers.SabertoothTiger:
                    case (int)MonsterCareers.GiantScorpion:
                    case (int)MonsterCareers.Centaur:
                    case (int)MonsterCareers.Gargoyle:
                    case (int)MonsterCareers.Giant:
                    case (int)MonsterCareers.Daedroth:
                        return BodySize.Large;
                    case (int)MonsterCareers.Dragonling_Alternate:
                        return BodySize.Huge;
                }
            }
        }

        /// <summary>Return the size category of the player.</summary>
        public static BodySize GetPlayerBodySize(PlayerEntity player)
        {
            return BodySize.Average;
        }

        /// <summary>Class to hopefully keep from spamming new UID values from enemies being assigned 'fake' weapons, will see if it works or not.</summary>
        public class DummyDFUItem
        {
            public ItemGroups itemGroup { get; set; }
            public int groupIndex { get; set; }
            public int nativeMaterialValue { get; set; }
            public int templateIndex { get; set; }
            public float weightInKg { get; set; }

            // Default constructor
            public DummyDFUItem()
            {
                // Default values
                itemGroup = ItemGroups.None;
                groupIndex = 0;
                nativeMaterialValue = -1;
                templateIndex = -1;
                weightInKg = 0.0f;
            }

            public short GetWeaponSkillIDAsShort()
            {
                int skill = GetWeaponSkillUsed();
                switch (skill)
                {
                    case (int)DFCareer.ProficiencyFlags.ShortBlades:
                        return (int)Skills.ShortBlade;
                    case (int)DFCareer.ProficiencyFlags.LongBlades:
                        return (int)Skills.LongBlade;
                    case (int)DFCareer.ProficiencyFlags.Axes:
                        return (int)Skills.Axe;
                    case (int)DFCareer.ProficiencyFlags.BluntWeapons:
                        return (int)Skills.BluntWeapon;
                    case (int)DFCareer.ProficiencyFlags.MissileWeapons:
                        return (int)Skills.Archery;

                    default:
                        return (int)Skills.None;
                }
            }

            public int GetWeaponSkillUsed()
            {
                switch (templateIndex)
                {
                    case (int)Weapons.Dagger:
                    case (int)Weapons.Tanto:
                    case (int)Weapons.Wakazashi:
                    case (int)Weapons.Shortsword:
                        return (int)DFCareer.ProficiencyFlags.ShortBlades;
                    case (int)Weapons.Broadsword:
                    case (int)Weapons.Longsword:
                    case (int)Weapons.Saber:
                    case (int)Weapons.Katana:
                    case (int)Weapons.Claymore:
                    case (int)Weapons.Dai_Katana:
                        return (int)DFCareer.ProficiencyFlags.LongBlades;
                    case (int)Weapons.Battle_Axe:
                    case (int)Weapons.War_Axe:
                        return (int)DFCareer.ProficiencyFlags.Axes;
                    case (int)Weapons.Flail:
                    case (int)Weapons.Mace:
                    case (int)Weapons.Warhammer:
                    case (int)Weapons.Staff:
                        return (int)DFCareer.ProficiencyFlags.BluntWeapons;
                    case (int)Weapons.Short_Bow:
                    case (int)Weapons.Long_Bow:
                        return (int)DFCareer.ProficiencyFlags.MissileWeapons;

                    default:
                        return (int)Skills.None;
                }
            }
        }

        /// <summary>Generates a dummy weapon.</summary>
        public static DummyDFUItem CreateDummyWeapon(Weapons weapon, WeaponMaterialTypes material)
        {
            // Create dummy item
            DummyDFUItem newItem = new DummyDFUItem();
            newItem.itemGroup = ItemGroups.Weapons;
            newItem.groupIndex = DaggerfallUnity.Instance.ItemHelper.GetGroupIndex(ItemGroups.Weapons, (int)weapon);
            newItem.nativeMaterialValue = (int)material;

            ItemTemplate itemTemplate = DaggerfallUnity.Instance.ItemHelper.GetItemTemplate(newItem.itemGroup, newItem.groupIndex);

            newItem.templateIndex = itemTemplate.index;

            int quarterKgs = (int)(itemTemplate.baseWeight * 4);
            float matQuarterKgs = (float)(quarterKgs * weightMultipliersByMaterial[(int)material]) / 4;
            newItem.weightInKg = Mathf.Round(matQuarterKgs) / 4;

            return newItem;
        }

        public static int CalcPlayerVsMonsterAttack(PlayerEntity attacker, EnemyEntity target, bool enemyAnimStateRecord, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            short wepType = 30;
            float matReqDamMulti = 1f;
            int chanceToHitMod = 0;
            bool critSuccess = false;
            float critDamMulti = 1f;
            int critHitAddi = 0;
            int damageModifiers = 0;
            int backstabChance = 0;
            int damage = 0;
            bool unarmedAttack = false;

            PCP pProps = GetPlayerCombatProps(attacker);
            ECP eProps = GetEnemyCombatProps(target);

            // Now that I have the "body size" thing, maybe have that effect the hit chances somewhat depending on the size of the attacker compared to the target, will see.
            // Also maybe have the material requirement stuff only factor in after armor is actually penetrated, if at all? Once again, will see.

            if (weapon != null)
            {
                wepType = weapon.GetWeaponSkillIDAsShort();

                if (softMatRequireModuleCheck)
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                    {
                        int targetMatRequire = (int)target.MinMetalToHit;
                        int weaponMatValue = weapon.NativeMaterialValue;
                        matReqDamMulti = targetMatRequire - weaponMatValue;

                        if (matReqDamMulti <= 0) // There is no "bonus" damage for meeting material requirements, nor for exceeding them, just normal unmodded damage.
                            matReqDamMulti = 1;
                        else // There is a damage penalty for attacking a target with below the minimum material requirements of that target, more as the difference between becomes greater.
                            matReqDamMulti = (Mathf.Min(matReqDamMulti * 0.2f, 0.9f) - 1) * -1; // Keeps the damage multiplier penalty from going above 90% reduced damage.
                    }
                }
                else
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                    {
                        DaggerfallUI.Instance.PopupMessage(TextManager.Instance.GetLocalizedText("materialIneffective"));
                        return 0;
                    }
                }
            }

            int playerWeaponSkill = attacker.Skills.GetLiveSkillValue(wepType);
            playerWeaponSkill = (int)Mathf.Ceil(playerWeaponSkill * 1.5f); // Makes it so player weapon skill has 150% of the effect it normally would on hit chance. So now instead of 50 weapon skill adding +50 to the end, 50 will now add +75.
            chanceToHitMod = playerWeaponSkill;

            critSuccess = CriticalStrikeHandler(attacker);

            if (critSuccess)
            {
                critDamMulti = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 5);
                critHitAddi = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 4);

                critDamMulti = (critDamMulti * .05f) + 1;
                chanceToHitMod += critHitAddi;
            }

            // Apply swing modifiers
            ToHitAndDamageMods swingMods = CalculateSwingModifiers(GameManager.Instance.WeaponManager.ScreenWeapon);
            damageModifiers += swingMods.damageMod;
            chanceToHitMod += swingMods.toHitMod;

            // Apply proficiency modifiers
            ToHitAndDamageMods proficiencyMods = CalculateProficiencyModifiers(attacker, weapon);
            damageModifiers += proficiencyMods.damageMod;
            chanceToHitMod += proficiencyMods.toHitMod;

            // Apply racial bonuses
            ToHitAndDamageMods racialMods = CalculateRacialModifiers(attacker, weapon, attacker);
            damageModifiers += racialMods.damageMod;
            chanceToHitMod += racialMods.toHitMod;

            backstabChance = CalculateBackstabChance(attacker, null, enemyAnimStateRecord);
            chanceToHitMod += backstabChance;

            int struckBodyPart = CalculateStruckBodyPart();

            // Get damage for weaponless attacks
            if (wepType == (short)DFCareer.Skills.HandToHand)
            {
                unarmedAttack = true;

                if (CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                {
                    damage = CalculateHandToHandAttackDamage(attacker, target, damageModifiers, true); // Added my own, non-overriden version of this method for modification.

                    damage = CalculateBackstabDamage(damage, backstabChance);
                }
            }
            // Handle weapon attacks
            else if (weapon != null)
            {
                // Apply weapon material modifier.
                chanceToHitMod += CalculateWeaponToHit(weapon);

                // Mod hook for adjusting final hit chance mod. (is a no-op in DFU)
                if (archeryModuleCheck)
                    chanceToHitMod = AdjustWeaponHitChanceMod(attacker, target, chanceToHitMod, weaponAnimTime, weapon);

                if (CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                {
                    damage = CalculateWeaponAttackDamage(attacker, target, damageModifiers, weaponAnimTime, weapon);

                    damage = CalculateBackstabDamage(damage, backstabChance);
                }

                // Handle poisoned weapons
                if (damage > 0 && weapon.poisonType != Poisons.None)
                {
                    FormulaHelper.InflictPoison(attacker, target, weapon.poisonType, false);
                    weapon.poisonType = Poisons.None;
                }
            }

            damage = Mathf.Max(0, damage); // I think this is just here to keep damage from outputting a negative value.

            if (critSuccess)
            {
                damage = (int)Mathf.Round(damage * critDamMulti); // Multiplies 'Final' damage values, before reductions, with the critical damage multiplier.
            }

            // I'm thinking of having the armor and shield stuff happen before all the mat-damage reduction and natural dam reduction. The idea being those parts would only really
            // be taken into consideration once the protection of the armor fails or is penetrated in someway to actually harm the wearer in someway, that's the idea atleast.
            DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand); // Checks if character is using a shield or not.
            bool shieldStrongSpot = false;
            bool shieldBlockSuccess = false;
            if (shield != null)
            {
                BodyParts[] protectedBodyParts = shield.GetShieldProtectedBodyParts();

                for (int i = 0; (i < protectedBodyParts.Length) && !shieldStrongSpot; i++)
                {
                    if (protectedBodyParts[i] == (BodyParts)struckBodyPart)
                        shieldStrongSpot = true;
                }
                shieldBlockSuccess = ShieldBlockChanceCalculation(target, attacker, shieldStrongSpot, shield, weapon);
                // Guess continue reworking this aspect tomorrow, will see. What I'm thinking is maybe having a block actually be a full block of an attack, but obviously alot less likely to happen.
                // And "hardpoint" spots will just act as damage reduction and damage threshold if you don't "fully block" an attack, and it lands in a spot that shield type considers
                // a "hardpoint." Also maybe make it not 100% in those cases, but some smaller percentage that it instead hits the armor below the shield, if any, that sort of thing, will see.

                if (shieldBlockSuccess)
                    shieldBlockSuccess = CompareShieldToUnderArmor(target, shield, struckBodyPart, naturalDamResist);
            }

            float damCheckBeforeMatMod = damage;

            damage = (int)Mathf.Round(damage * matReqDamMulti); // Could not find much better place to put there, so here seems fine, right after crit multiplier is taken into account.

            float damCheckAfterMatMod = damage;

            if (softMatRequireModuleCheck)
            {
                if (damCheckBeforeMatMod > 0 && (damCheckAfterMatMod / damCheckBeforeMatMod) <= 0.45f)
                    DaggerfallUI.AddHUDText("This Weapon Is Not Very Effective Against This Creature.", 1.00f);
            }

            int targetEndur = target.Stats.LiveEndurance - 50;
            int targetStren = target.Stats.LiveStrength - 50; // Every point of these does something, positive and negative between 50.
            int targetWillp = target.Stats.LiveWillpower - 50;

            float naturalDamResist = (targetEndur * .002f);
            naturalDamResist += (targetStren * .001f);
            naturalDamResist += (targetWillp * .001f);

            Mathf.Clamp(naturalDamResist, -0.2f, 0.2f);
        }

        public static int CalcMonsterVsPlayerAttack(EnemyEntity attacker, PlayerEntity target, bool enemyAnimStateRecord, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            short wepType = 30;
            float matReqDamMulti = 1f;
            int chanceToHitMod = 0;
            bool critSuccess = false;
            float critDamMulti = 1f;
            int critHitAddi = 0;
            int damageModifiers = 0;
            int damage = 0;
            bool unarmedAttack = false;
            bool hasSpecialMonsterWeapon = false;
            DummyDFUItem monsterWeapon = null;

            // Choose whether weapon-wielding enemies use their weapons or weaponless attacks.
            // In classic, weapon-wielding enemies use the damage values of their weapons, instead of their weaponless values.
            // For some enemies this gives lower damage than similar-tier monsters and the weaponless values seems more appropriate, so here
            // enemies will choose to use their weaponless attack if it is more damaging.
            if (weapon != null)
            {
                int weaponAverage = (weapon.GetBaseDamageMin() + weapon.GetBaseDamageMax()) / 2;
                int noWeaponAverage = (attacker.MobileEnemy.MinDamage + attacker.MobileEnemy.MaxDamage) / 2;
                if (noWeaponAverage > weaponAverage)
                {
                    // Use hand-to-hand
                    weapon = null;
                }
            }

            if (weapon != null)
            {
                wepType = weapon.GetWeaponSkillIDAsShort();

                if (softMatRequireModuleCheck)
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                    {
                        int targetMatRequire = (int)target.MinMetalToHit;
                        int weaponMatValue = weapon.NativeMaterialValue;
                        matReqDamMulti = targetMatRequire - weaponMatValue;

                        if (matReqDamMulti <= 0) // There is no "bonus" damage for meeting material requirements, nor for exceeding them, just normal unmodded damage.
                            matReqDamMulti = 1;
                        else // There is a damage penalty for attacking a target with below the minimum material requirements of that target, more as the difference between becomes greater.
                            matReqDamMulti = (Mathf.Min(matReqDamMulti * 0.2f, 0.9f) - 1) * -1; // Keeps the damage multiplier penalty from going above 90% reduced damage.
                    }
                }
                else
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                    {
                        return 0;
                    }
                }
            }

            chanceToHitMod = attacker.Skills.GetLiveSkillValue(wepType);

            critSuccess = CriticalStrikeHandler(attacker);

            if (critSuccess)
            {
                critDamMulti = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 5);
                critHitAddi = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 10);

                critDamMulti = (critDamMulti * .025f) + 1;
                chanceToHitMod += critHitAddi;
            }

            int struckBodyPart = CalculateStruckBodyPart();

            // Get damage for weaponless attacks
            if (wepType == (short)DFCareer.Skills.HandToHand)
            {
                unarmedAttack = true; // Check for later if weapon is NOT being used.

                if (attacker.EntityType == EntityTypes.EnemyClass)
                {
                    if (CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                    {
                        damage = CalculateHandToHandAttackDamage(attacker, target, damageModifiers, false); // Added my own, non-overriden version of this method for modification.
                    }
                }
                else // attacker is a monster
                {
                    hasSpecialMonsterWeapon = SpecialWeaponCheckForMonsters(attacker);

                    if (hasSpecialMonsterWeapon)
                    {
                        unarmedAttack = false;
                        monsterWeapon = MonsterWeaponAssign(attacker);
                        wepType = monsterWeapon.GetWeaponSkillIDAsShort();
                    }

                    // Handle multiple attacks by AI
                    int minBaseDamage = 0;
                    int maxBaseDamage = 0;
                    int attackNumber = 0;
                    while (attackNumber < 3) // Classic supports up to 5 attacks but no monster has more than 3
                    {
                        if (attackNumber == 0)
                        {
                            minBaseDamage = attacker.MobileEnemy.MinDamage;
                            maxBaseDamage = attacker.MobileEnemy.MaxDamage;
                        }
                        else if (attackNumber == 1)
                        {
                            minBaseDamage = attacker.MobileEnemy.MinDamage2;
                            maxBaseDamage = attacker.MobileEnemy.MaxDamage2;
                        }
                        else if (attackNumber == 2)
                        {
                            minBaseDamage = attacker.MobileEnemy.MinDamage3;
                            maxBaseDamage = attacker.MobileEnemy.MaxDamage3;
                        }

                        int reflexesChance = 50 - (10 * ((int)target.Reflexes - 2));

                        if (DFRandom.rand() % 100 < reflexesChance && minBaseDamage > 0 && CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                        {
                            int hitDamage = UnityEngine.Random.Range(minBaseDamage, maxBaseDamage + 1);
                            // Apply special monster attack effects
                            if (hitDamage > 0 && attackNumber == 0)
                                FormulaHelper.OnMonsterHit(attacker, target, hitDamage);

                            damage += hitDamage;
                        }
                        ++attackNumber;
                    }
                    if (damage >= 1)
                        damage = CalculateHandToHandAttackDamage(attacker, target, damage, false); // Added my own, non-overriden version of this method for modification.
                }
            }
            // Handle weapon attacks
            else if (weapon != null)
            {
                // Apply weapon material modifier.
                chanceToHitMod += CalculateWeaponToHit(weapon);

                if (CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                {
                    damage = CalculateWeaponAttackDamage(attacker, target, damageModifiers, weaponAnimTime, weapon);
                }

                // Handle poisoned weapons
                if (damage > 0 && weapon.poisonType != Poisons.None)
                {
                    FormulaHelper.InflictPoison(attacker, target, weapon.poisonType, false);
                    weapon.poisonType = Poisons.None;
                }
            }

            damage = Mathf.Max(0, damage); // I think this is just here to keep damage from outputting a negative value.

            if (critSuccess)
            {
                damage = (int)Mathf.Round(damage * critDamMulti); // Multiplies 'Final' damage values, before reductions, with the critical damage multiplier.
            }

            damage = (int)Mathf.Round(damage * matReqDamMulti); // Could not find much better place to put there, so here seems fine, right after crit multiplier is taken into account.

            int targetEndur = target.Stats.LiveEndurance - 50;
            int targetStren = target.Stats.LiveStrength - 50; // Every point of these does something, positive and negative between 50.
            int targetWillp = target.Stats.LiveWillpower - 50;

            float naturalDamResist = (targetEndur * .002f);
            naturalDamResist += (targetStren * .001f);
            naturalDamResist += (targetWillp * .001f);

            Mathf.Clamp(naturalDamResist, -0.2f, 0.2f);
        }

        public static int CalcMonsterVsMonsterAttack(EnemyEntity attacker, EnemyEntity target, bool enemyAnimStateRecord, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            short wepType = 30;
            float matReqDamMulti = 1f;
            int chanceToHitMod = 0;
            bool critSuccess = false;
            float critDamMulti = 1f;
            int critHitAddi = 0;
            int damageModifiers = 0;
            int damage = 0;
            bool unarmedAttack = false;
            bool hasSpecialMonsterWeapon = false;
            DummyDFUItem monsterWeapon = null;

            // Choose whether weapon-wielding enemies use their weapons or weaponless attacks.
            // In classic, weapon-wielding enemies use the damage values of their weapons, instead of their weaponless values.
            // For some enemies this gives lower damage than similar-tier monsters and the weaponless values seems more appropriate, so here
            // enemies will choose to use their weaponless attack if it is more damaging.
            if (weapon != null)
            {
                int weaponAverage = (weapon.GetBaseDamageMin() + weapon.GetBaseDamageMax()) / 2;
                int noWeaponAverage = (attacker.MobileEnemy.MinDamage + attacker.MobileEnemy.MaxDamage) / 2;
                if (noWeaponAverage > weaponAverage)
                {
                    // Use hand-to-hand
                    weapon = null;
                }
            }

            if (weapon != null)
            {
                wepType = weapon.GetWeaponSkillIDAsShort();

                if (softMatRequireModuleCheck)
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                    {
                        int targetMatRequire = (int)target.MinMetalToHit;
                        int weaponMatValue = weapon.NativeMaterialValue;
                        matReqDamMulti = targetMatRequire - weaponMatValue;

                        if (matReqDamMulti <= 0) // There is no "bonus" damage for meeting material requirements, nor for exceeding them, just normal unmodded damage.
                            matReqDamMulti = 1;
                        else // There is a damage penalty for attacking a target with below the minimum material requirements of that target, more as the difference between becomes greater.
                            matReqDamMulti = (Mathf.Min(matReqDamMulti * 0.2f, 0.9f) - 1) * -1; // Keeps the damage multiplier penalty from going above 90% reduced damage.
                    }
                }
                else
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                    {
                        return 0;
                    }
                }
            }

            chanceToHitMod = attacker.Skills.GetLiveSkillValue(wepType);

            critSuccess = CriticalStrikeHandler(attacker);

            if (critSuccess)
            {
                critDamMulti = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 5);
                critHitAddi = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 10);

                critDamMulti = (critDamMulti * .025f) + 1;
                chanceToHitMod += critHitAddi;
            }

            int struckBodyPart = CalculateStruckBodyPart();

            // Get damage for weaponless attacks
            if (wepType == (short)DFCareer.Skills.HandToHand)
            {
                unarmedAttack = true; // Check for later if weapon is NOT being used.

                if (attacker.EntityType == EntityTypes.EnemyClass)
                {
                    if (CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                    {
                        damage = CalculateHandToHandAttackDamage(attacker, target, damageModifiers, false); // Added my own, non-overriden version of this method for modification.
                    }
                }
                else // attacker is a monster
                {
                    hasSpecialMonsterWeapon = SpecialWeaponCheckForMonsters(attacker);

                    if (hasSpecialMonsterWeapon)
                    {
                        unarmedAttack = false;
                        monsterWeapon = MonsterWeaponAssign(attacker);
                        wepType = monsterWeapon.GetWeaponSkillIDAsShort();
                    }

                    // Handle multiple attacks by AI
                    int minBaseDamage = 0;
                    int maxBaseDamage = 0;
                    int attackNumber = 0;
                    while (attackNumber < 3) // Classic supports up to 5 attacks but no monster has more than 3
                    {
                        if (attackNumber == 0)
                        {
                            minBaseDamage = attacker.MobileEnemy.MinDamage;
                            maxBaseDamage = attacker.MobileEnemy.MaxDamage;
                        }
                        else if (attackNumber == 1)
                        {
                            minBaseDamage = attacker.MobileEnemy.MinDamage2;
                            maxBaseDamage = attacker.MobileEnemy.MaxDamage2;
                        }
                        else if (attackNumber == 2)
                        {
                            minBaseDamage = attacker.MobileEnemy.MinDamage3;
                            maxBaseDamage = attacker.MobileEnemy.MaxDamage3;
                        }

                        int reflexesChance = 50 - (10 * ((int)GameManager.Instance.PlayerEntity.Reflexes - 2));

                        if (DFRandom.rand() % 100 < reflexesChance && minBaseDamage > 0 && CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                        {
                            int hitDamage = UnityEngine.Random.Range(minBaseDamage, maxBaseDamage + 1);
                            // Apply special monster attack effects
                            if (hitDamage > 0 && attackNumber == 0)
                                FormulaHelper.OnMonsterHit(attacker, target, hitDamage);

                            damage += hitDamage;
                        }
                        ++attackNumber;
                    }
                    if (damage >= 1)
                        damage = CalculateHandToHandAttackDamage(attacker, target, damage, false); // Added my own, non-overriden version of this method for modification.
                }
            }
            // Handle weapon attacks
            else if (weapon != null)
            {
                // Apply weapon material modifier.
                chanceToHitMod += CalculateWeaponToHit(weapon);

                if (CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                {
                    damage = CalculateWeaponAttackDamage(attacker, target, damageModifiers, weaponAnimTime, weapon);
                }

                // Handle poisoned weapons
                if (damage > 0 && weapon.poisonType != Poisons.None)
                {
                    FormulaHelper.InflictPoison(attacker, target, weapon.poisonType, false);
                    weapon.poisonType = Poisons.None;
                }
            }

            damage = Mathf.Max(0, damage); // I think this is just here to keep damage from outputting a negative value.

            if (critSuccess)
            {
                damage = (int)Mathf.Round(damage * critDamMulti); // Multiplies 'Final' damage values, before reductions, with the critical damage multiplier.
            }

            damage = (int)Mathf.Round(damage * matReqDamMulti); // Could not find much better place to put there, so here seems fine, right after crit multiplier is taken into account.

            int targetEndur = target.Stats.LiveEndurance - 50;
            int targetStren = target.Stats.LiveStrength - 50; // Every point of these does something, positive and negative between 50.
            int targetWillp = target.Stats.LiveWillpower - 50;

            float naturalDamResist = (targetEndur * .002f);
            naturalDamResist += (targetStren * .001f);
            naturalDamResist += (targetWillp * .001f);

            Mathf.Clamp(naturalDamResist, -0.2f, 0.2f);
        }

        // -- Newly Added Stuff 4-17-2024 --

        #region Overridden Base Methods

        private static int CalculateAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, bool enemyAnimStateRecord, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            if (attacker == null || target == null)
                return 0;

            PlayerEntity player = GameManager.Instance.PlayerEntity;

            if (attacker == player || target == player)
            {
                if (attacker == player)
                {
                    // Player vs Monster
                    PlayerEntity PlayerAttacker = attacker as PlayerEntity;
                    EnemyEntity AITarget = target as EnemyEntity;

                    return CalcPlayerVsMonsterAttack(PlayerAttacker, AITarget, enemyAnimStateRecord, weaponAnimTime, weapon);
                }
                else
                {
                    // Monster vs Player
                    EnemyEntity AIAttacker = attacker as EnemyEntity;
                    PlayerEntity PlayerTarget = target as PlayerEntity;

                    return CalcMonsterVsPlayerAttack(AIAttacker, PlayerTarget, enemyAnimStateRecord, weaponAnimTime, weapon);
                }
            }
            else
            {
                // Monster vs Monster
                EnemyEntity AIAttacker = attacker as EnemyEntity;
                EnemyEntity AITarget = target as EnemyEntity;

                return CalcMonsterVsMonsterAttack(AIAttacker, AITarget, enemyAnimStateRecord, weaponAnimTime, weapon);
            }

            int damageModifiers = 0;
            int damage = 0;
            int chanceToHitMod = 0;
            int backstabChance = 0;
            //PlayerEntity player = GameManager.Instance.PlayerEntity;
            short skillID = 0;
            bool unarmedAttack = false;
            bool weaponAttack = false;
            bool bluntWep = false;
            bool specialMonsterWeapon = false;
            bool monsterArmorCheck = false;
            bool critSuccess = false;
            float critDamMulti = 1f;
            int critHitAddi = 0;
            float matReqDamMulti = 1f;

            //EnemyEntity AITarget = null;
            AITarget = target as EnemyEntity;

            // Continue from here below.
            // Continue here tomorrow I suppose.

            if (condBasedEffectModuleCheck) // Only runs if "Condition Based Effectiveness" module is active. As well if a weapon is even being used.
            {
                if (attacker == player && weapon != null) // Only the player has weapon damage effected by condition value.
                {
                    damage = AlterDamageBasedOnWepCondition(damage, bluntWep, weapon);
                    //Debug.LogFormat("Damage Multiplier Due To Weapon Condition = {0}", damage);
                }
            }

            if (damage < 1) // Cut off the execution if the damage is still not anything higher than 1 at this point in the method.
                return damage;

            DamageEquipment(attacker, target, damage, weapon, struckBodyPart); // Might alter this later so that equipment damage is only calculated with the amount that was reduced, not the whole initial amount, will see.

            if (((target != player) && (AITarget.EntityType == EntityTypes.EnemyMonster)))
            {
                monsterArmorCheck = ArmorStruckVerification(target, struckBodyPart); // Check for if a monster has a piece of armor/shield hit by an attack, returns true if so.

                if (!monsterArmorCheck)
                {
                    //Debug.Log("------------------------------------------------------------------------------------------");
                    //Debug.LogFormat("Here is damage value before Monster 'Natural' Damage reduction is applied = {0}", damage);

                    damage = PercentageReductionCalculationForMonsters(attacker, target, damage, bluntWep, naturalDamResist);

                    //Debug.LogFormat("Here is damage value after Monster 'Natural' Damage reduction = {0}", damage);
                    //Debug.Log("------------------------------------------------------------------------------------------");
                }
                else
                {
                    if (unarmedAttack)
                    {
                        //Debug.Log("------------------------------------------------------------------------------------------");
                        //Debug.LogFormat("Here is damage value before armor reduction is applied = {0}", damage);

                        damage = CalculateArmorDamageReductionWithUnarmed(attacker, target, damage, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against unarmed.

                        //Debug.LogFormat("Here is damage value after armor reduction = {0}", damage);
                        //Debug.Log("------------------------------------------------------------------------------------------");
                    }
                    else if (weaponAttack)
                    {
                        //Debug.Log("------------------------------------------------------------------------------------------");
                        //Debug.LogFormat("Here is damage value before armor reduction is applied = {0}", damage);

                        damage = CalculateArmorDamageReductionWithWeapon(attacker, target, damage, weapon, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against weapons.

                        //Debug.LogFormat("Here is damage value after armor reduction = {0}", damage);
                        //Debug.Log("------------------------------------------------------------------------------------------");
                    }
                }
            }
            else
            {
                if (unarmedAttack)
                {
                    //Debug.Log("------------------------------------------------------------------------------------------");
                    //Debug.LogFormat("Here is damage value before armor reduction is applied = {0}", damage);
                    int damBefore = damage;

                    damage = CalculateArmorDamageReductionWithUnarmed(attacker, target, damage, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against unarmed.

                    int damAfter = damage;
                    //Debug.LogFormat("Here is damage value after armor reduction = {0}", damage);
                    if (damBefore > 0)
                    {
                        int damReduPercent = ((100 * damAfter / damBefore) - 100) * -1;
                        //Debug.LogFormat("Here is damage reduction percent = {0}%", damReduPercent);
                    }
                    //Debug.Log("------------------------------------------------------------------------------------------");
                }
                else if (weaponAttack)
                {
                    //Debug.Log("------------------------------------------------------------------------------------------");
                    //Debug.LogFormat("Here is damage value before armor reduction is applied = {0}", damage);
                    int damBefore = damage;

                    damage = CalculateArmorDamageReductionWithWeapon(attacker, target, damage, weapon, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against weapons.

                    int damAfter = damage;
                    //Debug.LogFormat("Here is damage value after armor reduction = {0}", damage);
                    if (damBefore > 0)
                    {
                        int damReduPercent = ((100 * damAfter / damBefore) - 100) * -1;
                        //Debug.LogFormat("Here is damage reduction percent = {0}%", damReduPercent);
                    }
                    //Debug.Log("------------------------------------------------------------------------------------------");
                }
            }

            // Apply Ring of Namira effect
            if (target == player)
            {
                DaggerfallUnityItem[] equippedItems = target.ItemEquipTable.EquipTable;
                DaggerfallUnityItem item = null;
                if (equippedItems.Length != 0)
                {
                    if (IsRingOfNamira(equippedItems[(int)EquipSlots.Ring0]) || IsRingOfNamira(equippedItems[(int)EquipSlots.Ring1]))
                    {
                        IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(RingOfNamiraEffect.EffectKey);
                        effectTemplate.EnchantmentPayloadCallback(EnchantmentPayloadFlags.None,
                            targetEntity: AIAttacker.EntityBehaviour,
                            sourceItem: item,
                            sourceDamage: damage);
                    }
                }
            }

            //Debug.LogFormat("Damage {0} applied, animTime={1}  ({2})", damage, weaponAnimTime, GameManager.Instance.WeaponManager.ScreenWeapon.WeaponState);

            return damage;
        }

        /// <summary>Does a roll for based on the critical strike chance of the attacker, if this roll is successful critSuccess is returned as 'true'.</summary>
        public static bool CriticalStrikeHandler(DaggerfallEntity attacker)
        {
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            int attackerLuckBonus = (int)Mathf.Floor((float)(attacker.Stats.LiveLuck - 50) / 25f);
            Mathf.Clamp(attackerLuckBonus, -2, 2); // This is meant to disallow crit odds from going higher than 50%, incase luck is allowed to go over 100 points.

            if (attacker == player)
            {
                if (Dice100.SuccessRoll(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / (4 - attackerLuckBonus))) // Player has a 25% chance of critting at level 100. 33% with 75 luck, and 50% with 100 luck.
                    return true;
                else
                    return false;
            }
            else
            {
                if (Dice100.SuccessRoll(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / (5 - attackerLuckBonus))) // Monsters have a 20% chance of critting at level 100, or level 14.
                    return true;
                else
                    return false;
            }
        }

        public static int CalculateHandToHandAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, int damageModifier, bool player)
        {
            int damage = 0;

            if (player)
            {
                int minBaseDamage = FormulaHelper.CalculateHandToHandMinDamage(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.HandToHand));
                int maxBaseDamage = FormulaHelper.CalculateHandToHandMaxDamage(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.HandToHand));
                damage = UnityEngine.Random.Range(minBaseDamage, maxBaseDamage + 1);

                // Apply damage modifiers.
                damage += damageModifier;

                // Apply strength modifier for players. It is not applied in classic despite what the in-game description for the Strength attribute says.
                damage += DamageModifier(attacker.Stats.LiveStrength);
            }
            else
                damage += damageModifier;

            if (damage < 1)
                damage = 0;

            if (damage >= 1)
                damage += GetBonusOrPenaltyByEnemyType(attacker, target); // Added my own, non-overriden version of this method for modification.

            return damage;
        }

        static int GetBonusOrPenaltyByEnemyType(DaggerfallEntity attacker, DaggerfallEntity target) // Possibly update at some point like 10.26 did so vampirism of the player is taken into account.
        {
            if (attacker == null || target == null) // So after observing the effects of adding large amounts of weight to an enemy, it does not seem to have that much of an effect on their ability to be stun-locked. As the knock-back/hurt state is probably the real issue here, as well as other parts of the AI choices. So I think this comes down a lot more to AI behavior than creature weight values. So with that, I will mostly likely make an entirely seperate mod to try and deal with this issue and continue on non-AI related stuff in this already large mod. So yeah, start another "proof of concept" mod project where I attempt to change the AI to make it more challenging/smarter.
                return 0;

            int attackerWillpMod = 0;
            int confidenceMod = 0;
            int courageMod = 0;
            EnemyEntity AITarget = null;
            PlayerEntity player = GameManager.Instance.PlayerEntity;

            if (target != player)
                AITarget = target as EnemyEntity;
            else
                player = target as PlayerEntity;

            if (player == attacker) // When attacker is the player
            {
                attackerWillpMod = (int)Mathf.Round((attacker.Stats.LiveWillpower - 50) / 5);
                confidenceMod = Mathf.Max(10 + attackerWillpMod - (target.Level / 2), 0);
                courageMod = Mathf.Max((target.Level / 2) - attackerWillpMod, 0);

                confidenceMod = UnityEngine.Random.Range(0, confidenceMod);
            }
            else // When attacker is anything other than the player // Apparently "32" is the maximum possible level cap for the player without cheating.
            {
                attackerWillpMod = (int)Mathf.Round((attacker.Stats.LiveWillpower - 50) / 5);
                confidenceMod = Mathf.Max(5 + attackerWillpMod + (attacker.Level / 4), 0);
                courageMod = Mathf.Max(target.Level - (attacker.Level + attackerWillpMod), 0);

                confidenceMod = UnityEngine.Random.Range(0, confidenceMod);
            }

            int damage = 0;
            // Apply bonus or penalty by opponent type.
            // In classic this is broken and only works if the attack is done with a weapon that has the maximum number of enchantments.
            if (AITarget != null && AITarget.GetEnemyGroup() == DFCareer.EnemyGroups.Undead)
            {
                if (((int)attacker.Career.UndeadAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                {
                    damage += confidenceMod;
                }
                if (((int)attacker.Career.UndeadAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                {
                    damage -= courageMod;
                }
            }
            else if (AITarget != null && AITarget.GetEnemyGroup() == DFCareer.EnemyGroups.Daedra)
            {
                if (((int)attacker.Career.DaedraAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                {
                    damage += confidenceMod;
                }
                if (((int)attacker.Career.DaedraAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                {
                    damage -= courageMod;
                }
            }
            else if ((AITarget != null && AITarget.GetEnemyGroup() == DFCareer.EnemyGroups.Humanoid) || player == target) // Apparently human npcs already are in the humanoid career, so "|| AITarget.EntityType == EntityTypes.EnemyClass" is unneeded.
            {
                if (((int)attacker.Career.HumanoidAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                {
                    damage += confidenceMod;
                }
                if (((int)attacker.Career.HumanoidAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                {
                    damage -= courageMod;
                }
            }
            else if (AITarget != null && AITarget.GetEnemyGroup() == DFCareer.EnemyGroups.Animals)
            {
                if (((int)attacker.Career.AnimalsAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                {
                    damage += confidenceMod;
                }
                if (((int)attacker.Career.AnimalsAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                {
                    damage -= courageMod;
                }
            }
            return damage;
        }

        private static int CalculateBackstabChance(PlayerEntity player, DaggerfallEntity target, bool isEnemyFacingAwayFromPlayer)
        {
            // If enemy is facing away from player
            if (isEnemyFacingAwayFromPlayer)
            {
                player.TallySkill(DFCareer.Skills.Backstabbing, 1);
                return player.Skills.GetLiveSkillValue(DFCareer.Skills.Backstabbing);
            }
            return 0;
        }

        private static int CalculateBackstabDamage(int damage, int backstabbingLevel)
        {
            if (backstabbingLevel > 1 && Dice100.SuccessRoll(backstabbingLevel))
            {
                damage *= 3;
                string backstabMessage = TextManager.Instance.GetLocalizedText("successfulBackstab");
                DaggerfallUI.Instance.PopupMessage(backstabMessage);
            }
            return damage;
        }

        private static int CalculateStruckBodyPart()
        {
            //int[] bodyParts = { 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 6 }; // Default Values.
            int[] bodyParts = { 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 5, 5, 5, 5, 6, 6 }; // Changed slightly. Head Now 5%, Left-Right Arms 15%, Chest 20%, Hands 15%, Legs 20%, Feet 10%. Plan on doing more with this, making it so when different parts of the body take damage, do different things, like extra damage, or less damage, but attribute drain until health restored or something.
            return bodyParts[UnityEngine.Random.Range(0, bodyParts.Length)];
        }

        public static int DamageModifier(int strength) // Fixes the inconsistency with Unity and Classic, changes the 5 to a 10 divider.
        {
            return (int)Mathf.Floor((strength - 50) / 10f);
        }

        private static ToHitAndDamageMods CalculateSwingModifiers(FPSWeapon onscreenWeapon) // Make this a setting option obviously. Possibly modify this swing mod formula, so that is works in a similar way to Morrowind, where the attack direction "types" are not universal like here, but each weapon type has a different amount of attacks that are "better" or worse depending. Like a dagger having better damage with a thrusting attacking, rather than slashing, and the same for other weapon types. Possibly as well, make the different attack types depending on the weapon, have some degree of "resistance penetration" or something, like thrusting doing increased damage resistance penatration.
        {
            ToHitAndDamageMods mods = new ToHitAndDamageMods();
            if (onscreenWeapon != null)
            {
                // The Daggerfall manual groups diagonal slashes to the left and right as if they are the same, but they are different.
                // Classic does not apply swing modifiers to unarmed attacks.
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeUp)
                {
                    mods.damageMod = -4;
                    mods.toHitMod = 10;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDownRight)
                {
                    mods.damageMod = -2;
                    mods.toHitMod = 5;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDownLeft)
                {
                    mods.damageMod = 2;
                    mods.toHitMod = -5;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDown)
                {
                    mods.damageMod = 4;
                    mods.toHitMod = -10;
                }
            }
            return mods;
        }

        public static ToHitAndDamageMods CalculateProficiencyModifiers(DaggerfallEntity attacker, DaggerfallUnityItem weapon)
        {
            ToHitAndDamageMods mods = new ToHitAndDamageMods(); // If I feel that 50 starting points is too much for a level 1 character, I could always make the benefits only start past that 50 mark or something, maybe 40.
            if (weapon != null)
            {
                // Apply weapon proficiency
                if (((int)attacker.Career.ExpertProficiencies & weapon.GetWeaponSkillUsed()) != 0)
                {
                    switch (weapon.GetWeaponSkillIDAsShort())
                    {
                        case (short)DFCareer.Skills.Archery:
                            mods.damageMod = (attacker.Stats.LiveStrength / 25) + (attacker.Stats.LiveAgility / 25) + 1; //9
                            mods.toHitMod = (attacker.Stats.LiveAgility / 8) + (attacker.Stats.LiveSpeed / 20) + (attacker.Stats.LiveLuck / 20); //22.5
                            break;
                        case (short)DFCareer.Skills.Axe:
                            mods.damageMod = (attacker.Stats.LiveStrength / 20) + (attacker.Stats.LiveAgility / 33) + 1; //9
                            mods.toHitMod = (attacker.Stats.LiveStrength / 11) + (attacker.Stats.LiveAgility / 11) + (attacker.Stats.LiveLuck / 22); //22.5
                            break;
                        case (short)DFCareer.Skills.BluntWeapon:
                            mods.damageMod = (attacker.Stats.LiveStrength / 20) + (attacker.Stats.LiveEndurance / 33) + 1; //9
                            mods.toHitMod = (attacker.Stats.LiveStrength / 10) + (attacker.Stats.LiveAgility / 16) + (attacker.Stats.LiveLuck / 16); //22.5
                            break;
                        case (short)DFCareer.Skills.LongBlade:
                            mods.damageMod = (attacker.Stats.LiveAgility / 20) + (attacker.Stats.LiveStrength / 33) + 1; //9
                            mods.toHitMod = (attacker.Stats.LiveAgility / 8) + (attacker.Stats.LiveSpeed / 20) + (attacker.Stats.LiveLuck / 20); //22.5
                            break;
                        case (short)DFCareer.Skills.ShortBlade:
                            mods.damageMod = (attacker.Stats.LiveAgility / 25) + (attacker.Stats.LiveSpeed / 25) + 1; //9
                            mods.toHitMod = (attacker.Stats.LiveAgility / 10) + (attacker.Stats.LiveSpeed / 14) + (attacker.Stats.LiveLuck / 18); //22.5
                            break;
                        default:
                            break;
                    }
                }
            }
            // Apply hand-to-hand proficiency. Hand-to-hand proficiency is not applied in classic.
            else if (((int)attacker.Career.ExpertProficiencies & (int)DFCareer.ProficiencyFlags.HandToHand) != 0)
            {
                mods.damageMod = (attacker.Stats.LiveStrength / 50) + (attacker.Stats.LiveEndurance / 50) + (attacker.Stats.LiveAgility / 50) + (attacker.Stats.LiveSpeed / 50) + 1; //9
                mods.toHitMod = (attacker.Stats.LiveAgility / 22) + (attacker.Stats.LiveSpeed / 22) + (attacker.Stats.LiveStrength / 22) + (attacker.Stats.LiveEndurance / 22) + (attacker.Stats.LiveLuck / 22); //22.5
            }
            //Debug.LogFormat("Here is the damage modifier for this proficiency = {0}", mods.damageMod);
            //Debug.LogFormat("Here is the accuracy modifier for this proficiency = {0}", mods.toHitMod);
            return mods;
        }

        public static ToHitAndDamageMods CalculateRacialModifiers(DaggerfallEntity attacker, DaggerfallUnityItem weapon, PlayerEntity player)
        {
            ToHitAndDamageMods mods = new ToHitAndDamageMods();
            if (weapon != null)
            {
                switch (player.RaceTemplate.ID)
                {
                    case (int)Races.Argonian:
                        if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.ShortBlade)
                        {
                            mods.damageMod = (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveSpeed / 33); //6
                            mods.toHitMod = (attacker.Stats.LiveAgility / 16) + (attacker.Stats.LiveSpeed / 33) + (attacker.Stats.LiveLuck / 33); //12
                        }
                        break;
                    case (int)Races.DarkElf:
                        if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.LongBlade)
                        {
                            mods.damageMod = (attacker.Stats.LiveAgility / 25) + (attacker.Stats.LiveStrength / 25); //8
                            mods.toHitMod = (attacker.Stats.LiveAgility / 25) + (attacker.Stats.LiveSpeed / 33) + (attacker.Stats.LiveLuck / 33); //10
                        }
                        else if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.ShortBlade)
                        {
                            mods.damageMod = (attacker.Stats.LiveAgility / 50) + (attacker.Stats.LiveSpeed / 50); //4
                            mods.toHitMod = (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveSpeed / 33) + (attacker.Stats.LiveLuck / 33); //9
                        }
                        break;
                    case (int)Races.Khajiit:
                        if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.ShortBlade)
                        {
                            mods.damageMod = (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveSpeed / 50); //5
                            mods.toHitMod = (attacker.Stats.LiveAgility / 20) + (attacker.Stats.LiveSpeed / 33) + (attacker.Stats.LiveLuck / 50); //10
                        }
                        break;
                    case (int)Races.Nord:
                        if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.Axe)
                        {
                            mods.damageMod = (attacker.Stats.LiveStrength / 16) + (attacker.Stats.LiveAgility / 33); //9
                            mods.toHitMod = (attacker.Stats.LiveStrength / 33) + (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveLuck / 33); //9
                        }
                        else if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.BluntWeapon)
                        {
                            mods.damageMod = (attacker.Stats.LiveStrength / 25) + (attacker.Stats.LiveEndurance / 25); //8
                            mods.toHitMod = (attacker.Stats.LiveStrength / 25) + (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveLuck / 33); //10
                        }
                        break;
                    case (int)Races.Redguard:
                        if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.LongBlade)
                        {
                            mods.damageMod = (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveStrength / 50); //5
                            mods.toHitMod = (attacker.Stats.LiveAgility / 10) + (attacker.Stats.LiveSpeed / 25) + (attacker.Stats.LiveLuck / 25); //18
                        }
                        else if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.BluntWeapon)
                        {
                            mods.damageMod = (attacker.Stats.LiveStrength / 33) + (attacker.Stats.LiveEndurance / 33); //6
                            mods.toHitMod = (attacker.Stats.LiveStrength / 20) + (attacker.Stats.LiveAgility / 25) + (attacker.Stats.LiveLuck / 33); //12
                        }
                        else if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.Axe)
                        {
                            mods.damageMod = (attacker.Stats.LiveStrength / 16) + (attacker.Stats.LiveAgility / 33); //6
                            mods.toHitMod = (attacker.Stats.LiveStrength / 33) + (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveLuck / 33); //12
                        }
                        else if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.Archery)
                        {
                            mods.damageMod = (attacker.Stats.LiveStrength / 50) + (attacker.Stats.LiveAgility / 50); //4
                            mods.toHitMod = (attacker.Stats.LiveAgility / 25) + (attacker.Stats.LiveSpeed / 33) + (attacker.Stats.LiveLuck / 33); //10
                        }
                        break;
                    case (int)Races.WoodElf:
                        if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.ShortBlade)
                        {
                            mods.damageMod = (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveSpeed / 50); //5
                            mods.toHitMod = (attacker.Stats.LiveAgility / 20) + (attacker.Stats.LiveSpeed / 33) + (attacker.Stats.LiveLuck / 50); //10
                        }
                        else if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.Archery)
                        {
                            mods.damageMod = (attacker.Stats.LiveStrength / 25) + (attacker.Stats.LiveAgility / 25); //8
                            mods.toHitMod = (attacker.Stats.LiveAgility / 10) + (attacker.Stats.LiveSpeed / 25) + (attacker.Stats.LiveLuck / 25); //18
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (weapon == null)
            {
                if (player.RaceTemplate.ID == (int)Races.Khajiit)
                {
                    mods.damageMod = (attacker.Stats.LiveStrength / 33) + (attacker.Stats.LiveEndurance / 33) + (attacker.Stats.LiveAgility / 50) + (attacker.Stats.LiveSpeed / 50); //10
                    mods.toHitMod = (attacker.Stats.LiveAgility / 25) + (attacker.Stats.LiveSpeed / 50) + (attacker.Stats.LiveStrength / 50) + (attacker.Stats.LiveEndurance / 50) + (attacker.Stats.LiveLuck / 50); //12
                }
                else if (player.RaceTemplate.ID == (int)Races.Nord)
                {
                    mods.damageMod = (attacker.Stats.LiveStrength / 33) + (attacker.Stats.LiveEndurance / 50); //5
                }
            }
            //Debug.LogFormat("Here is the damage modifier for this Race and Weapon = {0}", mods.damageMod);
            //Debug.LogFormat("Here is the accuracy modifier for this Race and Weapon = {0}", mods.toHitMod);
            return mods;
        }

        /// <summary>Struct for return values of formula that affect damage and to-hit chance.</summary>
        public struct ToHitAndDamageMods
        {
            public int damageMod;
            public int toHitMod;
        }

        public static int CalculateWeaponToHit(DaggerfallUnityItem weapon)
        {
            return weapon.GetWeaponMaterialModifier() * 2 + 2;

        }

        private static int CalculateWeaponAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, int damageModifier, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            int damage = UnityEngine.Random.Range(weapon.GetBaseDamageMin(), weapon.GetBaseDamageMax() + 1) + damageModifier;

            PlayerEntity player = GameManager.Instance.PlayerEntity;
            EnemyEntity AITarget = null;

            if (target == player)
            {
                if (GameManager.Instance.PlayerEffectManager.HasLycanthropy() || GameManager.Instance.PlayerEffectManager.HasVampirism())
                {
                    if (weapon.NativeMaterialValue == (int)WeaponMaterialTypes.Silver)
                        damage *= 2;
                }
            }
            else
            {
                // Has most of the "obvious" enemies take extra damage from silver weapons, most of the lower level undead, as well as werebeasts.
                AITarget = target as EnemyEntity;
                switch (AITarget.CareerIndex)
                {
                    case (int)MonsterCareers.Werewolf:
                    case (int)MonsterCareers.Wereboar:
                    case (int)MonsterCareers.SkeletalWarrior:
                    case (int)MonsterCareers.Ghost:
                    case (int)MonsterCareers.Mummy:
                    case (int)MonsterCareers.Wraith:
                    case (int)MonsterCareers.Vampire:
                        if (weapon.NativeMaterialValue == (int)WeaponMaterialTypes.Silver) { damage *= 2; } break;
                    default: break;
                }
            }
            // TODO: Apply strength bonus from Mace of Molag Bal

            // Apply strength modifier
            if (ItemEquipTable.GetItemHands(weapon) == ItemHands.Both && weapon.TemplateIndex != (int)Weapons.Short_Bow && weapon.TemplateIndex != (int)Weapons.Long_Bow)
                damage += (DamageModifier(attacker.Stats.LiveStrength)) * 2; // Multiplying by 2, so that two-handed weapons gets double the damage mod from Strength, except bows.
            else
                damage += DamageModifier(attacker.Stats.LiveStrength);

            // Apply material modifier.
            // The in-game display in Daggerfall of weapon damages with material modifiers is incorrect. The material modifier is half of what the display suggests.
            damage += weapon.GetWeaponMaterialModifier();
            if (damage < 1)
                damage = 0;

            if (damage >= 1)
                damage += GetBonusOrPenaltyByEnemyType(attacker, target); // Added my own, non-overriden version of this method for modification.

            // Mod hook for adjusting final damage. (is a no-op in DFU)
            if (attacker == player && archeryModuleCheck)
                damage = AdjustWeaponAttackDamage(attacker, target, damage, weaponAnimTime, weapon);

            return damage;
        }

        /// <summary>Calculates whether an attack on a target is successful or not.</summary>
        private static bool CalculateSuccessfulHit(DaggerfallEntity attacker, DaggerfallEntity target, int chanceToHitMod, int struckBodyPart)
        {
            if (attacker == null || target == null)
                return false;

            int chanceToHit = chanceToHitMod;
            //Debug.LogFormat("Starting chanceToHitMod = {0}", chanceToHit);

            // Get armor value for struck body part
            chanceToHit += CalculateArmorToHit(target, struckBodyPart);

            // Apply adrenaline rush modifiers.
            chanceToHit += CalculateAdrenalineRushToHit(attacker, target);

            // Apply enchantment modifier. 
            chanceToHit += attacker.ChanceToHitModifier;
            //Debug.LogFormat("Attacker Chance To Hit Mod 'Enchantment' = {0}", attacker.ChanceToHitModifier); // No idea what this does, always seeing 0.

            // Apply stat differential modifiers. (default: luck and agility)
            chanceToHit += CalculateStatDiffsToHit(attacker, target);

            // Apply skill modifiers. (default: dodge and crit strike)
            chanceToHit += CalculateSkillsToHit(attacker, target);
            //Debug.LogFormat("After Dodge = {0}", chanceToHitMod);

            // Apply monster modifier and biography adjustments.
            chanceToHit += CalculateAdjustmentsToHit(attacker, target);
            //Debug.LogFormat("Final chanceToHitMod = {0}", chanceToHitMod);

            Mathf.Clamp(chanceToHit, 3, 97);

            return Dice100.SuccessRoll(chanceToHit);
        }

        /// <summary>This is where specific monsters will be given a true or false, depending on if said monster is clearly holding a type of weapon in their sprite.</summary>
        public static bool SpecialWeaponCheckForMonsters(DaggerfallEntity attacker)
        {
            EnemyEntity AIAttacker = attacker as EnemyEntity;
            switch (AIAttacker.CareerIndex)
            {
                case (int)MonsterCareers.Centaur:
                case (int)MonsterCareers.Giant:
                case (int)MonsterCareers.Gargoyle:
                case (int)MonsterCareers.Orc:
                case (int)MonsterCareers.OrcSergeant:
                case (int)MonsterCareers.OrcShaman:
                case (int)MonsterCareers.OrcWarlord:
                case (int)MonsterCareers.IronAtronach:
                case (int)MonsterCareers.IceAtronach:
                case (int)MonsterCareers.SkeletalWarrior:
                case (int)MonsterCareers.Wraith:
                case (int)MonsterCareers.Lich:
                case (int)MonsterCareers.AncientLich:
                case (int)MonsterCareers.FrostDaedra:
                case (int)MonsterCareers.FireDaedra:
                case (int)MonsterCareers.Daedroth:
                case (int)MonsterCareers.DaedraLord:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>This is where specific monsters will be given a pre-defined weapon object for purposes of the rest of the formula, based on their level and sprite weapon appearance.</summary>
        public static DummyDFUItem MonsterWeaponAssign(DaggerfallEntity attacker)
        {
            EnemyEntity AIAttacker = attacker as EnemyEntity;
            switch (AIAttacker.CareerIndex)
            {
                case (int)MonsterCareers.SkeletalWarrior:
                    return CreateDummyWeapon(Weapons.War_Axe, WeaponMaterialTypes.Steel);
                case (int)MonsterCareers.Orc:
                    return CreateDummyWeapon(Weapons.Saber, WeaponMaterialTypes.Steel);
                case (int)MonsterCareers.OrcSergeant:
                    return CreateDummyWeapon(Weapons.Battle_Axe, WeaponMaterialTypes.Dwarven);
                case (int)MonsterCareers.OrcWarlord:
                case (int)MonsterCareers.Daedroth:
                    return CreateDummyWeapon(Weapons.Battle_Axe, WeaponMaterialTypes.Orcish);
                case (int)MonsterCareers.OrcShaman:
                case (int)MonsterCareers.Lich:
                case (int)MonsterCareers.AncientLich:
                    return CreateDummyWeapon(Weapons.Staff, WeaponMaterialTypes.Adamantium);
                case (int)MonsterCareers.Centaur:
                    return CreateDummyWeapon(Weapons.Claymore, WeaponMaterialTypes.Elven);
                case (int)MonsterCareers.Giant:
                    return CreateDummyWeapon(Weapons.Warhammer, WeaponMaterialTypes.Steel);
                case (int)MonsterCareers.Gargoyle:
                    return CreateDummyWeapon(Weapons.Flail, WeaponMaterialTypes.Steel);
                case (int)MonsterCareers.IronAtronach:
                    return CreateDummyWeapon(Weapons.Mace, WeaponMaterialTypes.Steel);
                case (int)MonsterCareers.IceAtronach:
                    return CreateDummyWeapon(Weapons.Katana, WeaponMaterialTypes.Elven);
                case (int)MonsterCareers.Wraith:
                    return CreateDummyWeapon(Weapons.Saber, WeaponMaterialTypes.Mithril);
                case (int)MonsterCareers.FrostDaedra:
                    return CreateDummyWeapon(Weapons.Warhammer, WeaponMaterialTypes.Daedric);
                case (int)MonsterCareers.FireDaedra:
                case (int)MonsterCareers.DaedraLord:
                    return CreateDummyWeapon(Weapons.Broadsword, WeaponMaterialTypes.Daedric);
                default:
                    return null;
            }
        }

        public static int CalculateArmorToHit(DaggerfallEntity target, int struckBodyPart)
        {
            EnemyEntity AITarget = target as EnemyEntity;
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            int armorValue = 0;

            // Get armor value for struck body part. This value is multiplied by 5 times in the "EnemyEntity.cs" script, makes a big difference.
            if (struckBodyPart <= target.ArmorValues.Length)
            {
                armorValue = target.ArmorValues[struckBodyPart];
            }

            // Sets the armorValue so that armor does not have any effect on the hit chance, it just defaults to the "naked" amount for the player and humanoid enemies, other monsters still have their normal AC score factored in.
            if (target == player)
                armorValue = 100 - target.IncreasedArmorValueModifier - target.DecreasedArmorValueModifier;
            else if (AITarget.EntityType == EntityTypes.EnemyClass)
                armorValue = 60;

            return armorValue;
        }

        public static int CalculateAdrenalineRushToHit(DaggerfallEntity attacker, DaggerfallEntity target)
        {
            const int adrenalineRushModifier = 8; //Buffed base adrenalineRushModifier by 3
            const int improvedAdrenalineRushModifier = 12; //Buffed improvedAdrenalineRushModifier by 4

            int chanceToHitMod = 0;
            if (attacker.Career.AdrenalineRush && attacker.CurrentHealth < (attacker.MaxHealth / 4)) //Made adrenaline rush effect come into effect earlier, I.E. at higher health percent. From /8 to /4
            {
                chanceToHitMod += (attacker.ImprovedAdrenalineRush) ? improvedAdrenalineRushModifier : adrenalineRushModifier;
            }

            if (target.Career.AdrenalineRush && target.CurrentHealth < (target.MaxHealth / 4)) //Made adrenaline rush effect come into effect earlier, I.E. at higher health percent. From /8 to /4
            {
                chanceToHitMod -= (target.ImprovedAdrenalineRush) ? improvedAdrenalineRushModifier : adrenalineRushModifier;
            }
            return chanceToHitMod;
        }

        public static int CalculateStatDiffsToHit(DaggerfallEntity attacker, DaggerfallEntity target)
        {
            int chanceToHitMod = 0;

            // Apply luck modifier.
            chanceToHitMod += ((attacker.Stats.LiveLuck - target.Stats.LiveLuck) / 10);
            //Debug.LogFormat("After Luck = {0}", chanceToHitMod);

            // Apply agility modifier.
            chanceToHitMod += ((attacker.Stats.LiveAgility - target.Stats.LiveAgility) / 4); //Made Agility have twice as much effect on final hit chance.
                                                                                             //Debug.LogFormat("After Agility = {0}", chanceToHitMod);

            // Possibly make the Speed Stat a small factor as well, seems like it would make sense.
            chanceToHitMod += ((attacker.Stats.LiveSpeed - target.Stats.LiveSpeed) / 8);
            //Debug.LogFormat("After Speed = {0}", chanceToHitMod);

            // When I think about it, I might want to get some of the other stats into this formula as well, to help casters somewhat, as well as explain it like a more intelligent character notices patterns in enemy movement and uses to to get in more hits, maybe even strength, the character strikes with such force that they pierce through armor easier.

            // Apply flat Luck factor for the target's chance of being hit. Higher luck above 50 means enemies will miss you more, and below 50 will mean they hit you more often.
            chanceToHitMod -= (int)Mathf.Round((float)(target.Stats.LiveLuck - 50) / 10); // With this, at most Luck will effect chances by either -5 or +5.

            return chanceToHitMod;
        }

        public static int CalculateSkillsToHit(DaggerfallEntity attacker, DaggerfallEntity target)
        {
            int chanceToHitMod = 0;

            // Apply dodging modifier.
            // This modifier is bugged in classic and the attacker's dodging skill is used rather than the target's.
            // DF Chronicles says the dodging calculation is (dodging / 10), but it actually seems to be (dodging / 4).
            // Apply dodging modifier.
            chanceToHitMod -= (target.Skills.GetLiveSkillValue(DFCareer.Skills.Dodging) / 2); // Changing 4 to a 2, so 100 dodge will give -50 to hit chance, very powerful.

            return chanceToHitMod;
        }

        /// <summary>Checks for if a shield block was successful and returns true if so, false if not.</summary>
        public static bool ShieldBlockChanceCalculation(DaggerfallEntity target, DaggerfallEntity attacker, bool shieldStrongSpot, DaggerfallUnityItem shield, DaggerfallUnityItem weapon)
        {
            if (weapon == null)
            {
                // Just make sure to check if weapon is null or not.
                // Might have the weapon being used have some influence if an attack goes through a shield hard-point and hits the armor under instead?
                // May not though, will see.
            }

            int atkAgility = attacker.Stats.LiveAgility - 50;
            int atkSpeed = attacker.Stats.LiveSpeed - 50;
            int atkLuck = attacker.Stats.LiveLuck - 50;

            float hardBlockChance = 0f;
            float softBlockChance = 0f;
            int targetAgili = target.Stats.LiveAgility - 50;
            int targetSpeed = target.Stats.LiveSpeed - 50;
            int targetStren = target.Stats.LiveStrength - 50;
            int targetEndur = target.Stats.LiveEndurance - 50;
            int targetWillp = target.Stats.LiveWillpower - 50;
            int targetLuck = target.Stats.LiveLuck - 50;

            chanceToHitMod += ((attacker.Stats.LiveAgility - target.Stats.LiveAgility) / 4);

            switch (shield.TemplateIndex)
            {
                case (int)Armor.Buckler:
                    fullBlockChance = (targetAgili * 1f) - atkAgility;
                    hardBlockChance = 30f;
                    softBlockChance = 20f;
                    break;
                case (int)Armor.Round_Shield:
                    hardBlockChance = 35f;
                    softBlockChance = 10f;
                    break;
                case (int)Armor.Kite_Shield:
                    hardBlockChance = 45f;
                    softBlockChance = 5f;
                    break;
                case (int)Armor.Tower_Shield:
                    hardBlockChance = 55f;
                    softBlockChance = -5f;
                    break;
                default:
                    hardBlockChance = 40f;
                    softBlockChance = 0f;
                    break;
            }

            if (shieldStrongSpot)
            {
                hardBlockChance += (targetAgili * .3f);
                hardBlockChance += (targetSpeed * .3f);
                hardBlockChance += (targetStren * .3f);
                hardBlockChance += (targetEndur * .2f);
                hardBlockChance += (targetWillp * .1f);
                hardBlockChance += (targetLuck * .1f);

                Mathf.Clamp(hardBlockChance, 7f, 95f);
                int blockChanceInt = (int)Mathf.Round(hardBlockChance);

                if (Dice100.SuccessRoll(blockChanceInt))
                {
                    //Debug.LogFormat("$$$. Shield Blocked A Hard-Point, Chance Was {0}%", blockChanceInt);
                    return true;
                }
                else
                {
                    //Debug.LogFormat("!!!. Shield FAILED To Block A Hard-Point, Chance Was {0}%", blockChanceInt);
                    return false;
                }
            }
            else
            {
                softBlockChance += (targetAgili * .3f);
                softBlockChance += (targetSpeed * .2f);
                softBlockChance += (targetStren * .2f);
                softBlockChance += (targetEndur * .1f);
                softBlockChance += (targetWillp * .1f);
                softBlockChance += (targetLuck * .1f);

                Mathf.Clamp(softBlockChance, 0f, 50f);
                int blockChanceInt = (int)Mathf.Round(softBlockChance);

                if (Dice100.SuccessRoll(blockChanceInt))
                {
                    //Debug.LogFormat("$$$. Shield Blocked A Soft-Point, Chance Was {0}%", blockChanceInt);
                    return true;
                }
                else
                {
                    //Debug.LogFormat("!!!. Shield FAILED To Block A Soft-Point, Chance Was {0}%", blockChanceInt);
                    return false;
                }
            }
        }

        /// <summary>Compares the damage reduction of the struck shield, with the armor under the part that was struck, and returns true if the shield has the higher reduction value, or false if the armor under has a higher reduction value. This is to keep a full-suit of daedric armor from being worse while wearing a leather shield, which when a block is successful, would actually take more damage than if not wearing a shield.</summary>
        public static bool CompareShieldToUnderArmor(DaggerfallEntity target, DaggerfallUnityItem shield, int struckBodyPart, float naturalDamResist)
        {
            int redDamShield = 100;
            int redDamUnderArmor = 100;
            short[] armorProps = new short[] {-1, -1};
            bool shieldQuickCheck = true;

            ArmorMaterialIdentifier(shield, ref armorProps);

            redDamShield = PercentageReductionAverage(shield, armorMaterial, redDamShield, naturalDamResist, shieldQuickCheck);
            shieldQuickCheck = false;


            EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
            DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
            if (armor != null)
            {
                ArmorMaterialIdentifier(armor, ref armorProps);

                redDamUnderArmor = PercentageReductionAverage(armor, armorMaterial, redDamUnderArmor, naturalDamResist, shieldQuickCheck);
            }
            else // If the body part struck in 'naked' IE has no armor protecting it.
            {
                redDamUnderArmor = (int)Mathf.Round(redDamUnderArmor * (1f - naturalDamResist));
            }

            if (redDamShield <= redDamUnderArmor)
            {
                //Debug.Log("$$$: Shield Is Stronger Than Under Armor, Shield Being Used");
                return true;
            }
            else
            {
                //Debug.Log("!!!: Shield Is Weaker Than Under Armor, Armor Being Used Instead");
                return false;
            }
        }

        /// <summary>Finds the material that an armor item is made from, then returns the multiplier that will be used later based on this material check.</summary>
        public static void ArmorMaterialIdentifier(DaggerfallUnityItem armor, ref short[] armorProps)
        {
            if (armor == null)
            {
                armorProps[0] = -1;
                armorProps[1] = -1;
            }
            else
            {
                armorProps[0] = GetArmorMatType(armor);
                armorProps[1] = GetArmorMaterial(armor);
            }
        }

        public static short GetArmorMatType(DaggerfallUnityItem armor)
        {
            int tIdx = armor.TemplateIndex;
            int mat = armor.NativeMaterialValue;

            if (!armor.IsShield)
            {
                if (mat >= (int)ArmorMaterialTypes.Iron && mat <= (int)ArmorMaterialTypes.Daedric)
                    return 2;
                else if (mat == (int)ArmorMaterialTypes.Chain || mat == (int)ArmorMaterialTypes.Chain2)
                    return 1;
                else if (mat == (int)ArmorMaterialTypes.Leather)
                    return 0;
                else
                    return -1;
            }
            else
            {
                if (tIdx >= (int)HeavyArmor.Cuirass && tIdx <= (int)HeavyArmor.Boots)
                    return 2;
                else if (tIdx >= (int)MediumArmor.Hauberk && tIdx <= (int)MediumArmor.Sollerets)
                    return 1;
                else if (tIdx >= (int)LightArmor.Jerkin && tIdx <= (int)LightArmor.Right_Vambrace)
                    return 0;
                else
                    return -1;
            }
        }

        public static short GetArmorMaterial(DaggerfallUnityItem armor)
        {
            int mat = armor.NativeMaterialValue;

            switch (mat)
            {
                case (int)ArmorMaterialTypes.Leather:
                    return 0;
                case (int)ArmorMaterialTypes.Chain:
                case (int)ArmorMaterialTypes.Chain2:
                    return 1;
                case (int)ArmorMaterialTypes.Iron:
                    return 2;
                case (int)ArmorMaterialTypes.Steel:
                case (int)ArmorMaterialTypes.Silver:
                    return 3;
                case (int)ArmorMaterialTypes.Elven:
                    return 4;
                case (int)ArmorMaterialTypes.Dwarven:
                    return 5;
                case (int)ArmorMaterialTypes.Mithril:
                case (int)ArmorMaterialTypes.Adamantium:
                    return 6;
                case (int)ArmorMaterialTypes.Ebony:
                    return 7;
                case (int)ArmorMaterialTypes.Orcish:
                    return 8;
                case (int)ArmorMaterialTypes.Daedric:
                    return 9;
                default:
                    return -1;
            }
        }

        /// <summary>Currently being used to compare the damage reduction of a shield to the under armor it is covering. This is the average of all different types of damage reduction for simplification of this.</summary>
        public static int PercentageReductionAverage(DaggerfallUnityItem item, int armorMaterial, int damage, float naturalDamResist, bool shieldQuickCheck)
        {
            float condMulti = 1f;

            if (condBasedEffectModuleCheck) // Only runs if "Condition Based Effectiveness" module is active.
            {
                condMulti = AlterArmorReducBasedOnItemCondition(item);
                //Debug.LogFormat("Average Reduction Multiplier Due To Condition = {0}", condMulti);
            }

            if (shieldQuickCheck)
            {
                switch (armorMaterial)
                {
                    case 1: // leather
                        return (int)Mathf.Round(damage * (Mathf.Min((.68f * condMulti), .81f) - naturalDamResist));
                    case 2: // chains 1 and 2
                        return (int)Mathf.Round(damage * (Mathf.Min((.64f * condMulti), .77f) - naturalDamResist));
                    case 3: // iron
                        return (int)Mathf.Round(damage * (Mathf.Min((.58f * condMulti), .70f) - naturalDamResist));
                    case 4: // steel and silver
                        return (int)Mathf.Round(damage * (Mathf.Min((.52f * condMulti), .66f) - naturalDamResist));
                    case 5: // elven
                        return (int)Mathf.Round(damage * (Mathf.Min((.49f * condMulti), .62f) - naturalDamResist));
                    case 6: // dwarven
                        return (int)Mathf.Round(damage * (Mathf.Min((.45f * condMulti), .61f) - naturalDamResist));
                    case 7: // mithril and adamantium
                        return (int)Mathf.Round(damage * (Mathf.Min((.41f * condMulti), .54f) - naturalDamResist));
                    case 8: // ebony
                        return (int)Mathf.Round(damage * (Mathf.Min((.38f * condMulti), .47f) - naturalDamResist));
                    case 9: // orcish
                        return (int)Mathf.Round(damage * (Mathf.Min((.34f * condMulti), .41f) - naturalDamResist));
                    case 10: // daedric
                        return (int)Mathf.Round(damage * (Mathf.Min((.30f * condMulti), .35f) - naturalDamResist));
                    default:
                        return (int)Mathf.Round(damage * (Mathf.Min((1f * condMulti), 1f) - naturalDamResist));
                }
            }
            else
            {
                switch (armorMaterial)
                {
                    case 1: // leather
                        return (int)Mathf.Round(damage * (Mathf.Min((.83f * condMulti), .93f) - naturalDamResist));
                    case 2: // chains 1 and 2
                        return (int)Mathf.Round(damage * (Mathf.Min((.81f * condMulti), .93f) - naturalDamResist));
                    case 3: // iron
                        return (int)Mathf.Round(damage * (Mathf.Min((.86f * condMulti), .92f) - naturalDamResist));
                    case 4: // steel and silver
                        return (int)Mathf.Round(damage * (Mathf.Min((.78f * condMulti), .90f) - naturalDamResist));
                    case 5: // elven
                        return (int)Mathf.Round(damage * (Mathf.Min((.73f * condMulti), .87f) - naturalDamResist));
                    case 6: // dwarven
                        return (int)Mathf.Round(damage * (Mathf.Min((.65f * condMulti), .84f) - naturalDamResist));
                    case 7: // mithril and adamantium
                        return (int)Mathf.Round(damage * (Mathf.Min((.58f * condMulti), .81f) - naturalDamResist));
                    case 8: // ebony
                        return (int)Mathf.Round(damage * (Mathf.Min((.51f * condMulti), .76f) - naturalDamResist));
                    case 9: // orcish
                        return (int)Mathf.Round(damage * (Mathf.Min((.42f * condMulti), .65f) - naturalDamResist));
                    case 10: // daedric
                        return (int)Mathf.Round(damage * (Mathf.Min((.35f * condMulti), .56f) - naturalDamResist));
                    default:
                        return (int)Mathf.Round(damage * (Mathf.Min((1f * condMulti), 1f) - naturalDamResist));
                }
            }
        }

        /// <summary>
        /// Size categories for any creatures, used in some combat formula.
        /// </summary>
        public enum BodySize
        {
            None = -1,
            Tiny = 0,
            Small = 1,
            Average = 2,
            Large = 3,
            Huge = 4,
        }

        /// <summary>
        /// Includes the light/leather armor added by Roleplay Realism: Items.
        /// </summary>
        public enum LightArmor
        {
            Jerkin = 520,
            Cuisse = 521,
            Helmet = 522,
            Boots = 523,
            Gloves = 524,
            Left_Vambrace = 525,
            Right_Vambrace = 526,
        }

        /// <summary>
        /// Includes the medium/chain armor added by Roleplay Realism: Items.
        /// </summary>
        public enum MediumArmor
        {
            Hauberk = 515,
            Chausses = 516,
            Left_Spaulders = 517,
            Right_Spaulders = 518,
            Sollerets = 519,
        }

        /// <summary>
        /// Includes the vanilla item IDs for the body covering armor pieces for now atleast.
        /// </summary>
        public enum HeavyArmor
        {
            Cuirass = 102,
            Gauntlets = 103,
            Greaves = 104,
            Left_Pauldron = 105,
            Right_Pauldron = 106,
            Helm = 107,
            Boots = 108,
        }

        private static int CalculateAdjustmentsToHit(DaggerfallEntity attacker, DaggerfallEntity target)
        {
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            EnemyEntity AITarget = target as EnemyEntity;

            int chanceToHitMod = 0;

            // Apply hit mod from character biography. This gives -5 to player chances to not be hit if they say they have trouble "Fighting and Parrying"
            if (target == player)
            {
                chanceToHitMod -= player.BiographyAvoidHitMod;
            }

            // Apply monster modifier.
            if ((target != player) && (AITarget.EntityType == EntityTypes.EnemyMonster))
            {
                chanceToHitMod += 50; // Changed from 40 to 50, +10, in since i'm going to make dodging have double the effect, as well as nerf weapon material hit mod more.
            }

            // DF Chronicles says -60 is applied at the end, but it actually seems to be -50.
            chanceToHitMod -= 50;

            return chanceToHitMod;
        }

        /// <summary>Allocate any equipment damage from a strike, and reduce item condition.</summary>
        private static bool DamageEquipment(DaggerfallEntity attacker, DaggerfallEntity target, int damage, DaggerfallUnityItem weapon, int struckBodyPart)
        {
            int atkStrength = attacker.Stats.LiveStrength;
            int tarMatMod = 0;
            int matDifference = 0;
            bool bluntWep = false;
            bool shtbladeWep = false;
            bool missileWep = false;
            int wepEqualize = 1;
            int wepWeight = 1;
            float wepDamResist = 1f;
            float armorDamResist = 1f;
            int startItemCondPer = 0;

            if (!armorHitFormulaModuleCheck) // Uses the regular shield formula if the "armorHitFormula" Module is disabled in settings, but the equipment damage module is still active.
            {
                DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand); // Checks if character is using a shield or not.
                shieldBlockSuccess = false;
                if (shield != null)
                {
                    BodyParts[] protectedBodyParts = shield.GetShieldProtectedBodyParts();

                    for (int i = 0; (i < protectedBodyParts.Length) && !shieldBlockSuccess; i++)
                    {
                        if (protectedBodyParts[i] == (BodyParts)struckBodyPart)
                            shieldBlockSuccess = true;
                    }
                }
            }

            // If damage was done by a weapon, damage the weapon and armor of the hit body part.
            if (weapon != null && damage > 0)
            {
                int atkMatMod = weapon.GetWeaponMaterialModifier() + 2;
                int wepDam = damage;
                wepEqualize = EqualizeMaterialConditions(weapon);
                wepDam *= wepEqualize;

                if (weapon.GetWeaponSkillIDAsShort() == 32) // Checks if the weapon being used is in the Blunt Weapon category, then sets a bool value to true.
                {
                    wepDam += (atkStrength / 10);
                    wepDamResist = (wepEqualize * .20f) + 1;
                    wepDam = (int)Mathf.Ceil(wepDam / wepDamResist);
                    bluntWep = true;
                    wepWeight = (int)Mathf.Ceil(weapon.EffectiveUnitWeightInKg());

                    startItemCondPer = weapon.ConditionPercentage;
                    ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
                }
                else if (weapon.GetWeaponSkillIDAsShort() == 28) // Checks if the weapon being used is in the Short Blade category, then sets a bool value to true.
                {
                    if (weapon.TemplateIndex == (int)Weapons.Dagger || weapon.TemplateIndex == (int)Weapons.Tanto)
                    {
                        wepDam += (atkStrength / 30);
                        wepDamResist = (wepEqualize * .90f) + 1;
                        wepDam = (int)Mathf.Ceil(wepDam / wepDamResist);
                        shtbladeWep = true;
                    }
                    else
                    {
                        wepDam += (atkStrength / 30);
                        wepDamResist = (wepEqualize * .30f) + 1;
                        wepDam = (int)Mathf.Ceil(wepDam / wepDamResist);
                        shtbladeWep = true;
                    }

                    startItemCondPer = weapon.ConditionPercentage;
                    ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
                }
                else if (weapon.GetWeaponSkillIDAsShort() == 33) // Checks if the weapon being used is in the Missile Weapon category, then sets a bool value to true.
                {
                    missileWep = true;

                    startItemCondPer = weapon.ConditionPercentage;
                    ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
                }
                else // If all other weapons categories have not been found, it defaults to this, which currently includes long blades and axes.
                {
                    wepDam += (atkStrength / 10);
                    wepDamResist = (wepEqualize * .20f) + 1;
                    wepDam = (int)Mathf.Ceil(wepDam / wepDamResist);

                    startItemCondPer = weapon.ConditionPercentage;
                    ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
                }

                if (attacker == GameManager.Instance.PlayerEntity)
                    WarningMessagePlayerEquipmentCondition(weapon, startItemCondPer);

                if (shieldBlockSuccess)
                {
                    DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand);
                    int shieldEqualize = EqualizeMaterialConditions(shield);
                    damage *= shieldEqualize;
                    tarMatMod = ArmorMaterialModifierFinder(shield);
                    matDifference = tarMatMod - atkMatMod;
                    damage = MaterialDifferenceDamageCalculation(shield, matDifference, atkStrength, damage, bluntWep, wepWeight, shieldBlockSuccess);

                    startItemCondPer = shield.ConditionPercentage;
                    ApplyConditionDamageThroughWeaponDamage(shield, target, damage, bluntWep, shtbladeWep, missileWep, wepEqualize);

                    if (target == GameManager.Instance.PlayerEntity)
                        WarningMessagePlayerEquipmentCondition(shield, startItemCondPer);
                }
                else
                {
                    EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
                    DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
                    if (armor != null)
                    {
                        int armorEqualize = EqualizeMaterialConditions(armor);
                        damage *= armorEqualize;
                        tarMatMod = ArmorMaterialModifierFinder(armor);
                        matDifference = tarMatMod - atkMatMod;
                        damage = MaterialDifferenceDamageCalculation(armor, matDifference, atkStrength, damage, bluntWep, wepWeight, shieldBlockSuccess);

                        startItemCondPer = armor.ConditionPercentage;
                        ApplyConditionDamageThroughWeaponDamage(armor, target, damage, bluntWep, shtbladeWep, missileWep, wepEqualize);

                        if (target == GameManager.Instance.PlayerEntity)
                            WarningMessagePlayerEquipmentCondition(armor, startItemCondPer);
                    }
                }
                return false;
            }
            else if (weapon == null && damage > 0) // Handles Unarmed attacks.
            {
                if (shieldBlockSuccess)
                {
                    DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand);
                    int shieldEqualize = EqualizeMaterialConditions(shield);
                    damage *= shieldEqualize;
                    tarMatMod = ArmorMaterialModifierFinder(shield);
                    atkStrength /= 5;
                    armorDamResist = (tarMatMod * .40f) + 1;
                    damage = (int)Mathf.Ceil((damage + atkStrength) / armorDamResist);

                    startItemCondPer = shield.ConditionPercentage;
                    ApplyConditionDamageThroughUnarmedDamage(shield, target, damage);

                    if (target == GameManager.Instance.PlayerEntity)
                        WarningMessagePlayerEquipmentCondition(shield, startItemCondPer);
                }
                else
                {
                    EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
                    DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
                    if (armor != null)
                    {
                        int armorEqualize = EqualizeMaterialConditions(armor);
                        damage *= armorEqualize;
                        tarMatMod = ArmorMaterialModifierFinder(armor);
                        atkStrength /= 5;
                        armorDamResist = (tarMatMod * .20f) + 1;
                        damage = (int)Mathf.Ceil((damage + atkStrength) / armorDamResist);

                        startItemCondPer = armor.ConditionPercentage;
                        ApplyConditionDamageThroughUnarmedDamage(armor, target, damage);

                        if (target == GameManager.Instance.PlayerEntity)
                            WarningMessagePlayerEquipmentCondition(armor, startItemCondPer);
                    }
                }
                return false;
            }
            return false;
        }

        private static int AdjustWeaponHitChanceMod(DaggerfallEntity attacker, DaggerfallEntity target, int hitChanceMod, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            if (weaponAnimTime > 0 && (weapon.TemplateIndex == (int)Weapons.Short_Bow || weapon.TemplateIndex == (int)Weapons.Long_Bow))
            {
                int adjustedHitChanceMod = hitChanceMod;
                if (weaponAnimTime < 200)
                    adjustedHitChanceMod -= 40;
                else if (weaponAnimTime < 500)
                    adjustedHitChanceMod -= 10;
                else if (weaponAnimTime < 1000)
                    adjustedHitChanceMod = hitChanceMod;
                else if (weaponAnimTime < 2000)
                    adjustedHitChanceMod += 10;
                else if (weaponAnimTime > 5000)
                    adjustedHitChanceMod -= 10;
                else if (weaponAnimTime > 8000)
                    adjustedHitChanceMod -= 20;

                //Debug.LogFormat("Adjusted Weapon HitChanceMod for bow drawing from {0} to {1} (t={2}ms)", hitChanceMod, adjustedHitChanceMod, weaponAnimTime);
                return adjustedHitChanceMod;
            }

            return hitChanceMod;
        }

        private static int AdjustWeaponAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, int damage, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            if (weaponAnimTime > 0 && (weapon.TemplateIndex == (int)Weapons.Short_Bow || weapon.TemplateIndex == (int)Weapons.Long_Bow))
            {
                double adjustedDamage = damage;
                if (weaponAnimTime < 800)
                    adjustedDamage *= (double)weaponAnimTime / 800;
                else if (weaponAnimTime < 5000)
                    adjustedDamage = damage;
                else if (weaponAnimTime < 6000)
                    adjustedDamage *= 0.85;
                else if (weaponAnimTime < 8000)
                    adjustedDamage *= 0.75;
                else if (weaponAnimTime < 9000)
                    adjustedDamage *= 0.5;
                else if (weaponAnimTime >= 9000)
                    adjustedDamage *= 0.25;

                //Debug.LogFormat("Adjusted Weapon Damage for bow drawing from {0} to {1} (t={2}ms)", damage, (int)adjustedDamage, weaponAnimTime);
                return (int)adjustedDamage;
            }

            return damage;
        }

        #endregion
    }
}
