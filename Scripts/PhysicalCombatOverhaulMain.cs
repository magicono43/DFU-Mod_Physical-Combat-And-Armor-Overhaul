// Project:         PhysicalCombatOverhaul mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2024 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    2/13/2024, 9:00 PM
// Last Edit:		12/17/2024, 10:00 PM
// Version:			1.50
// Special Thanks:  Hazelnut, Ralzar, and Kab
// Modifier:		

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
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
using System.Collections.Generic;

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

        public static bool conditionBasedWeaponEffectiveness { get; set; }
        public static bool conditionBasedArmorEffectiveness { get; set; }

        // Mod Compatibility Check Values
        public static bool RolePlayRealismArcheryModuleCheck { get; set; }
        public static bool RalzarMeanerMonstersEditCheck { get; set; }

        // Global Variables
        public static readonly short[] weightMultipliersByMaterial = { 4, 5, 4, 4, 3, 4, 4, 2, 4, 5 };

        static Dictionary<int, Monster> monsterDict;
        public const int monsterCareerCount = 42;
        public static Dictionary<int, Monster> MonsterDict
        {
            get
            {
                if (monsterDict == null)
                    monsterDict = PCOEnemyBasics.BuildMonsterDict();

                //monsterCareerCount = monsterDict.Count - 1;

                return monsterDict;
            }
        }

        #region Mod Sound Variables

        // Mod Sounds
        private static AudioClip lastFootstepPlayed = null;
        private static AudioClip lastSwaySoundPlayed = null;

        public static AudioClip[] EmptyAudioList = Array.Empty<AudioClip>();

        public static AudioClip[] MissedAttackClips = { null, null, null };
        public static AudioClip[] DodgedAttackClips = { null, null, null };

        public static AudioClip[] FulNegMatResClips = { null };

        public static AudioClip[] FulNegActShieldClips = { null, null, null };
        public static AudioClip[] FulNegPasShieldClips = { null, null, null };

        public static AudioClip[] FulNegMetalArmClips = { null, null, null };
        public static AudioClip[] FulNegChainArmClips = { null, null, null };
        public static AudioClip[] FulNegLeatherArmClips = { null, null, null };

        public static AudioClip[] ParNegActShieldClips = { null, null, null };
        public static AudioClip[] ParNegPasShieldClips = { null, null, null };

        public static AudioClip[] ParNegMetalArmClips = { null, null, null };
        public static AudioClip[] ParNegChainArmClips = { null, null, null };
        public static AudioClip[] ParNegLeatherArmClips = { null, null, null };

        public static AudioClip[] FulNegNatArmFleshClips = { null, null, null };
        public static AudioClip[] FulNegNatArmFurClips = { null, null, null };
        public static AudioClip[] FulNegNatArmScaleClips = { null, null, null };
        public static AudioClip[] FulNegNatArmBoneClips = { null, null, null };
        public static AudioClip[] FulNegNatArmRockClips = { null, null, null };
        public static AudioClip[] FulNegNatArmMetalClips = { null, null, null };

        public static AudioClip[] BluntHitFleshClips = { null, null, null };
        public static AudioClip[] SlashHitFleshClips = { null, null, null };
        public static AudioClip[] PierceHitFleshClips = { null, null, null };

        public static AudioClip[] BluntHitFurClips = { null, null, null };
        public static AudioClip[] SlashHitFurClips = { null, null, null };
        public static AudioClip[] PierceHitFurClips = { null, null, null };

        public static AudioClip[] BluntHitScaleClips = { null, null, null };
        public static AudioClip[] SlashHitScaleClips = { null, null, null };
        public static AudioClip[] PierceHitScaleClips = { null, null, null };

        public static AudioClip[] BluntHitBoneClips = { null, null, null };
        public static AudioClip[] SlashHitBoneClips = { null, null, null };
        public static AudioClip[] PierceHitBoneClips = { null, null, null };

        public static AudioClip[] BluntHitRockClips = { null, null, null };
        public static AudioClip[] SlashHitRockClips = { null, null, null };
        public static AudioClip[] PierceHitRockClips = { null, null, null };

        public static AudioClip[] BluntHitMetalClips = { null, null, null };
        public static AudioClip[] SlashHitMetalClips = { null, null, null };
        public static AudioClip[] PierceHitMetalClips = { null, null, null };

        public static AudioClip[] AttackHitEtherealClips = { null, null, null };

        #endregion

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

            // Overridden Due To FormulaHelper.cs Private Access Modifiers, otherwise would not be included here.
            FormulaHelper.RegisterOverride(mod, "CalculateStruckBodyPart", (Func<int>)CalculateStruckBodyPart);
            FormulaHelper.RegisterOverride(mod, "CalculateBackstabChance", (Func<PlayerEntity, DaggerfallEntity, bool, int>)CalculateBackstabChance);
            FormulaHelper.RegisterOverride(mod, "CalculateBackstabDamage", (Func<int, int, int>)CalculateBackstabDamage);
            //FormulaHelper.RegisterOverride(mod, "GetBonusOrPenaltyByEnemyType", (Func<DaggerfallEntity, EnemyEntity, int>)GetBonusOrPenaltyByEnemyType);

            FormulaHelper.RegisterOverride(mod, "AdjustWeaponHitChanceMod", (Func<DaggerfallEntity, DaggerfallEntity, int, int, DaggerfallUnityItem, int>)AdjustWeaponHitChanceMod);
            FormulaHelper.RegisterOverride(mod, "AdjustWeaponAttackDamage", (Func<DaggerfallEntity, DaggerfallEntity, int, int, DaggerfallUnityItem, int>)AdjustWeaponAttackDamage);

            // Load Resources
            LoadAudio();

            if (monsterDict == null)
                monsterDict = PCOEnemyBasics.BuildMonsterDict();

            DaggerfallWorkshop.Game.UserInterfaceWindows.PCODebugHUD debugHUD = new DaggerfallWorkshop.Game.UserInterfaceWindows.PCODebugHUD();
            DaggerfallWorkshop.Game.UserInterfaceWindows.DaggerfallHUD dfuHud = DaggerfallUI.Instance.DaggerfallHUD;
            dfuHud.ParentPanel.Components.Add(debugHUD);

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
            /*
            if (GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress)
                return;

            // Handle key presses
            if (InputManager.Instance.GetKeyDown(KeyCode.P))
            {
                AudioClip clip = FulNegActShieldClips[0];

                DaggerfallAudioSource dfAudioSource = GameManager.Instance.PlayerObject.GetComponent<DaggerfallAudioSource>();

                if (dfAudioSource != null)
                {
                    dfAudioSource.AudioSource.PlayOneShot(clip, DaggerfallUnity.Settings.SoundVolume); // Get these hotkeys for testing implemented tomorrow and test which AudioSource sounds better to play from.
                }
                else
                    DaggerfallUI.AddHUDText("Player Object DF Audio Source Is Null.", 2.00f);
            }

            if (InputManager.Instance.GetKeyDown(KeyCode.O))
            {
                AudioClip clip = FulNegActShieldClips[0];

                DaggerfallAudioSource dfAudioSource = GameManager.Instance.WeaponManager.ScreenWeapon.gameObject.GetComponent<DaggerfallAudioSource>();

                if (dfAudioSource != null)
                {
                    dfAudioSource.AudioSource.PlayOneShot(clip, DaggerfallUnity.Settings.SoundVolume);
                }
                else
                    DaggerfallUI.AddHUDText("FPSWeapon DF Audio Source Is Null.", 2.00f);
            }
            */
        }

        // -- Newly Added Stuff 4-23-2024 --

        /// <summary>'CombatVariables' Struct for various combat related values and properties of this current attack.</summary>
        public struct CVARS
        {
            public short wepType;
            public float matReqDamMulti;
            public int chanceToHitMod;
            public bool critSuccess;
            public float critDamMulti;
            public int critHitAddi;
            public int damageModifiers;
            public int backstabChance;
            public int damage;
            public bool unarmedAttack;

            public bool shieldStrongSpot;
            public bool shieldBlockSuccess;
            public bool hitShield;
            public float shieldDTAmount;
            public float shieldDRAmount;
            public int shieldMaterial;

            public int struckBodyPart;
            public float armorDTAmount;
            public float armorDRAmount;
            public int armorMaterial;
            public int armorType;

            public float finalDTAmount;
            public float finalDRAmount;

            public float damBeforeDT;
            public float damAfterDT;

            public float tNatDT;
            public float tBluntMulti;
            public float tSlashMulti;
            public float tPierceMulti;

            public DaggerfallEntity aEntity;
            public Races aRace;
            public bool aIsVamp;
            public bool aIsLycan;
            public int aStrn;
            public int aWill;
            public int aAgil;
            public int aEndu;
            public int aSped;
            public int aLuck;
            public int[] attackList;
            public AttackType attackType;
            public AttackElementType attackElement;

            public DaggerfallEntity tEntity;
            public Races tRace;
            public bool tIsVamp;
            public bool tIsLycan;
            public int tStrn;
            public int tWill;
            public int tAgil;
            public int tEndu;
            public int tSped;
            public int tLuck;
            public NaturalArmorType tNatArm;
            public int tArmHardness;
            public int tAvoidContrib;
            public bool missWasDodge;

            public BodySize atkSize;
            public BodySize tarSize;
            public int atkCareer;
            public int tarCareer;
            public bool fullBlock;
            public bool partBlock;

            public DaggerfallUnityItem aMonsterWeapon;
            public bool aUseDummyWep;
            public DaggerfallUnityItem tMonsterWeapon;
            public bool tUseDummyWep;
            public bool modDamRange;
        }

        /// <summary>Fill in basic data for a new 'CombatVariables' struct variable.</summary>
        public static CVARS GetCombatVariables(DaggerfallEntity attacker, DaggerfallEntity target)
        {
            CVARS cvars = new CVARS();
            cvars.wepType = 30;
            cvars.matReqDamMulti = 1f;
            cvars.chanceToHitMod = 0;
            cvars.critSuccess = false;
            cvars.critDamMulti = 1f;
            cvars.critHitAddi = 0;
            cvars.damageModifiers = 0;
            cvars.backstabChance = 0;
            cvars.damage = 0;
            cvars.unarmedAttack = false;

            cvars.shieldStrongSpot = false;
            cvars.shieldBlockSuccess = false;
            cvars.hitShield = false;
            cvars.shieldDTAmount = 0;
            cvars.shieldDRAmount = 0;
            cvars.shieldMaterial = -1;

            cvars.struckBodyPart = -1;
            cvars.armorDTAmount = 0;
            cvars.armorDRAmount = 0;
            cvars.armorMaterial = -1;
            cvars.armorType = -1;

            cvars.finalDTAmount = 0;
            cvars.finalDRAmount = 0;

            cvars.damBeforeDT = 0;
            cvars.damAfterDT = 0;

            cvars.tNatDT = 0;
            cvars.tBluntMulti = 1f;
            cvars.tSlashMulti = 1f;
            cvars.tPierceMulti = 1f;

            cvars.aEntity = attacker;
            cvars.aRace = Races.None;
            cvars.aIsVamp = false;
            cvars.aIsLycan = false;
            cvars.aStrn = attacker.Stats.LiveStrength - 50;
            cvars.aWill = attacker.Stats.LiveWillpower - 50;
            cvars.aAgil = attacker.Stats.LiveAgility - 50;
            cvars.aEndu = attacker.Stats.LiveEndurance - 50;
            cvars.aSped = attacker.Stats.LiveSpeed - 50;
            cvars.aLuck = attacker.Stats.LiveLuck - 50;
            cvars.attackList = new int[12];
            cvars.attackType = AttackType.Bash;
            cvars.attackElement = AttackElementType.None;

            cvars.tEntity = target;
            cvars.tRace = Races.None;
            cvars.tIsVamp = false;
            cvars.tIsLycan = false;
            cvars.tStrn = target.Stats.LiveStrength - 50;
            cvars.tWill = target.Stats.LiveWillpower - 50;
            cvars.tAgil = target.Stats.LiveAgility - 50;
            cvars.tEndu = target.Stats.LiveEndurance - 50;
            cvars.tSped = target.Stats.LiveSpeed - 50;
            cvars.tLuck = target.Stats.LiveLuck - 50;
            cvars.tNatArm = NaturalArmorType.Flesh;
            cvars.tArmHardness = -1;
            cvars.tAvoidContrib = 0;
            cvars.missWasDodge = false;

            cvars.atkSize = BodySize.Average;
            cvars.tarSize = BodySize.Average;
            cvars.atkCareer = -1;
            cvars.tarCareer = -1;
            cvars.fullBlock = false;
            cvars.partBlock = false;

            cvars.aMonsterWeapon = null;
            cvars.aUseDummyWep = false;
            cvars.tMonsterWeapon = null;
            cvars.tUseDummyWep = false;
            cvars.modDamRange = false;

            return cvars;
        }

        /// <summary>'CombatData' Struct for various combat related values and properties of this current attack, to be used for debug info mostly.</summary>
        public struct CDATA
        {
            public MonsterCareers aCareer;
            public BodySize aSize;
            public string aWeapon;
            public AttackType attackType;
            public AttackElementType attackElement;

            public MonsterCareers tCareer;
            public BodySize tSize;
            public string tArmor;
            public string tShield;
            public NaturalArmorType tNatArmType;

            public int struckBodyPart;
            public bool critHit;

            public int initialDam;
            public float damAfterArmor;
            public float damAfterNatArmor;
        }

        /// <summary>Fill in basic data for a new 'CombatData' struct variable.</summary>
        public static CDATA GetCombatVarsData(ref CVARS cVars)
        {
            CDATA cdata = new CDATA();
            cdata.aCareer = (MonsterCareers)cVars.atkCareer;
            cdata.aSize = cVars.atkSize;
            cdata.aWeapon = "Unarmed";
            cdata.attackType = AttackType.Bash;
            cdata.attackElement = AttackElementType.None;

            cdata.tCareer = (MonsterCareers)cVars.tarCareer;
            cdata.tSize = cVars.tarSize;
            cdata.tArmor = "Unarmored";
            cdata.tShield = "No Shield";
            cdata.tNatArmType = cVars.tNatArm;

            cdata.struckBodyPart = -1;
            cdata.critHit = false;

            cdata.initialDam = 0;
            cdata.damAfterArmor = 0;
            cdata.damAfterNatArmor = 0;

            return cdata;
        }

        /// <summary>Return the natural armor type of the player.</summary>
        public static NaturalArmorType GetPlayerNaturalArmorType(PlayerEntity player)
        {
            return NaturalArmorType.Flesh;
        }

        /// <summary>Return the size category of the player.</summary>
        public static BodySize GetPlayerBodySize(PlayerEntity player)
        {
            return BodySize.Average;
        }

        /// <summary>Return the career of the player.</summary>
        public static int GetPlayerCareer(PlayerEntity player)
        {
            return -2;
        }

        /// <summary>Return the career of the provided creature.</summary>
        public static int GetCreatureCareer(EnemyEntity enemy)
        {
            if (enemy.EntityType == EntityTypes.EnemyClass)
            {
                return -1;
            }
            else
            {
                return enemy.CareerIndex;
            }
        }

        /// <summary>Return the attack type the player used.</summary>
        public static AttackType GetPlayerAttackType(ref CVARS cVars)
        {
            switch ((Skills)cVars.wepType)
            {
                default:
                case Skills.HandToHand:
                    if (cVars.aIsLycan && (GameManager.Instance.PlayerEffectManager.GetRacialOverrideEffect() as DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects.LycanthropyEffect).IsTransformed) // If player is a lycanthrope and currently transformed.
                        return AttackType.Claw;
                    else if (cVars.aIsVamp)
                        return AttackType.Scratch;
                    else if (cVars.aRace == Races.Khajiit)
                        return AttackType.Claw;
                    else
                        return AttackType.Bash;
                case Skills.ShortBlade:
                case Skills.Archery:
                    return AttackType.Stab;
                case Skills.LongBlade:
                case Skills.Axe:
                    return AttackType.Slash;
                case Skills.BluntWeapon:
                    return AttackType.Bludgeon;
            }
        }

        /// <summary>Return the element of the attack the player used.</summary>
        public static AttackElementType GetPlayerAttackElement(ref CVARS cVars)
        {
            return AttackElementType.None;
        }

        /// <summary>Populate common static combat variables stored for this particular creature.</summary>
        public static void GetMonsterSpecificCombatVariables(bool target, DaggerfallEntity creature, ref CVARS cVars)
        {
            if (target)
            {
                if (cVars.tarCareer >= 0 && cVars.tarCareer <= monsterCareerCount)
                {
                    Monster monster = MonsterDict[cVars.tarCareer];

                    cVars.tarSize = monster.Size;
                    cVars.tNatArm = monster.ArmorType;
                    cVars.tArmHardness = monster.ArmorHardness;
                    cVars.tMonsterWeapon = monster.MonsterWeapon;
                    cVars.tNatDT = monster.NaturalDT;
                    cVars.tBluntMulti = monster.BluntDR;
                    cVars.tSlashMulti = monster.SlashDR;
                    cVars.tPierceMulti = monster.PierceDR;
                }
            }
            else
            {
                if (cVars.atkCareer >= 0 && cVars.atkCareer <= monsterCareerCount)
                {
                    Monster monster = MonsterDict[cVars.atkCareer];

                    cVars.atkSize = monster.Size;
                    cVars.attackList = (int[])monster.AttacksList.Clone();
                    cVars.attackType = (AttackType)cVars.attackList[1];
                    cVars.attackElement = (AttackElementType)cVars.attackList[2];
                    cVars.aMonsterWeapon = monster.MonsterWeapon;
                }
            }
        }

        public static int CalcPlayerVsMonsterAttack(PlayerEntity attacker, EnemyEntity target, bool enemyAnimStateRecord, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            CVARS cVars = GetCombatVariables(attacker, target);
            cVars.aRace = (Races)attacker.BirthRaceTemplate.ID;
            cVars.aIsVamp = GameManager.Instance.PlayerEffectManager.HasVampirism();
            cVars.aIsLycan = GameManager.Instance.PlayerEffectManager.HasLycanthropy();
            cVars.atkCareer = GetPlayerCareer(attacker);
            cVars.atkSize = GetPlayerBodySize(attacker);
            cVars.tarCareer = GetCreatureCareer(target);
            GetMonsterSpecificCombatVariables(true, target, ref cVars);

            CDATA cData = GetCombatVarsData(ref cVars);

            if (weapon != null)
            {
                cVars.wepType = weapon.GetWeaponSkillIDAsShort();
                cData.aWeapon = weapon.LongName;

                if (softMatRequireModuleCheck)
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                    {
                        int targetMatRequire = (int)target.MinMetalToHit;
                        int weaponMatValue = weapon.NativeMaterialValue;
                        cVars.matReqDamMulti = targetMatRequire - weaponMatValue;

                        if (cVars.matReqDamMulti <= 0) // There is no "bonus" damage for meeting material requirements, nor for exceeding them, just normal unmodded damage.
                            cVars.matReqDamMulti = 1;
                        else // There is a damage penalty for attacking a target with below the minimum material requirements of that target, more as the difference between becomes greater.
                            cVars.matReqDamMulti = (Mathf.Min(cVars.matReqDamMulti * 0.2f, 0.9f) - 1) * -1; // Keeps the damage multiplier penalty from going above 90% reduced damage.
                    }
                }
                else
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                    {
                        DaggerfallUI.Instance.PopupMessage(TextManager.Instance.GetLocalizedText("materialIneffective"));
                        PlayRelevantCombatSound(CombatSoundTypes.Mat_Resist, attacker, target, ref cVars);
                        Instance.RaiseOnPlayerAttackedMonsterEvent(cData);
                        return 0;
                    }
                }
            }

            int playerWeaponSkill = attacker.Skills.GetLiveSkillValue(cVars.wepType);
            playerWeaponSkill = (int)Mathf.Ceil(playerWeaponSkill * 1.5f); // Makes it so player weapon skill has 150% of the effect it normally would on hit chance. So now instead of 50 weapon skill adding +50 to the end, 50 will now add +75.
            cVars.chanceToHitMod = playerWeaponSkill;

            cVars.attackType = GetPlayerAttackType(ref cVars);
            cData.attackType = cVars.attackType;
            cVars.attackElement = GetPlayerAttackElement(ref cVars);
            cData.attackElement = cVars.attackElement;

            cVars.critSuccess = CriticalStrikeHandler(attacker);
            cData.critHit = cVars.critSuccess;

            if (cVars.critSuccess)
            {
                cVars.critDamMulti = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 5);
                cVars.critHitAddi = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 4);

                cVars.critDamMulti = (cVars.critDamMulti * .05f) + 1;
                cVars.chanceToHitMod += cVars.critHitAddi;
            }

            // Apply swing modifiers
            ToHitAndDamageMods swingMods = CalculateSwingModifiers(GameManager.Instance.WeaponManager.ScreenWeapon);
            cVars.damageModifiers += swingMods.damageMod;
            cVars.chanceToHitMod += swingMods.toHitMod;

            // Apply proficiency modifiers
            ToHitAndDamageMods proficiencyMods = CalculateProficiencyModifiers(attacker, weapon);
            cVars.damageModifiers += proficiencyMods.damageMod;
            cVars.chanceToHitMod += proficiencyMods.toHitMod;

            // Apply racial bonuses
            ToHitAndDamageMods racialMods = CalculateRacialModifiers(attacker, weapon, attacker);
            cVars.damageModifiers += racialMods.damageMod;
            cVars.chanceToHitMod += racialMods.toHitMod;

            cVars.backstabChance = CalculateBackstabChance(attacker, null, enemyAnimStateRecord);
            cVars.chanceToHitMod += cVars.backstabChance;

            cVars.struckBodyPart = CalculateStruckBodyPart();
            cData.struckBodyPart = cVars.struckBodyPart;

            // Get damage for weaponless attacks
            if (cVars.wepType == (short)DFCareer.Skills.HandToHand)
            {
                cVars.unarmedAttack = true;

                if (CalculateHitSuccess(attacker, target, ref cVars))
                {
                    cVars.damage = CalculateHandToHandAttackDamage(attacker, target, cVars.damageModifiers, true); // Added my own, non-overriden version of this method for modification.

                    cVars.damage = CalculateBackstabDamage(cVars.damage, cVars.backstabChance);
                }
            }
            // Handle weapon attacks
            else if (weapon != null)
            {
                // Apply weapon material modifier.
                cVars.chanceToHitMod += CalculateWeaponToHit(weapon);

                // Mod hook for adjusting final hit chance mod. (is a no-op in DFU)
                if (archeryModuleCheck)
                    cVars.chanceToHitMod = AdjustWeaponHitChanceMod(attacker, target, cVars.chanceToHitMod, weaponAnimTime, weapon);

                if (CalculateHitSuccess(attacker, target, ref cVars))
                {
                    cVars.damage = CalculateWeaponAttackDamage(attacker, target, cVars.damageModifiers, weaponAnimTime, weapon);

                    cVars.damage = CalculateBackstabDamage(cVars.damage, cVars.backstabChance);
                }
            }

            cVars.damage = Mathf.Max(0, cVars.damage); // I think this is just here to keep damage from outputting a negative value.

            if (cVars.damage <= 0)
            {
                if (cVars.missWasDodge) { PlayRelevantCombatSound(CombatSoundTypes.Dodge, attacker, target, ref cVars); }
                else { PlayRelevantCombatSound(CombatSoundTypes.Miss, attacker, target, ref cVars); }
                Instance.RaiseOnPlayerAttackedMonsterEvent(cData);
                return 0;
            }

            if (cVars.critSuccess)
            {
                cVars.damage = (int)Mathf.Round(cVars.damage * cVars.critDamMulti); // Multiplies 'Final' damage values, before reductions, with the critical damage multiplier.
            }

            cData.initialDam = cVars.damage;

            DaggerfallUnityItem shield = null;
            DaggerfallUnityItem armor = null;

            EvaluateArmorAndShieldCoverage(target, ref cVars, out shield, out armor);

            if (shield != null) { cData.tShield = shield.LongName; }
            if (armor != null) { cData.tArmor = armor.LongName; }

            if (cVars.damage > 0)
            {
                if (FactorInArmor(attacker, target, weapon, shield, armor, ref cVars)) { Instance.RaiseOnPlayerAttackedMonsterEvent(cData); return 0; }
                cData.damAfterArmor = cVars.damAfterDT;
            }
            else
            {
                if (cVars.missWasDodge) { PlayRelevantCombatSound(CombatSoundTypes.Dodge, attacker, target, ref cVars); }
                else { PlayRelevantCombatSound(CombatSoundTypes.Miss, attacker, target, ref cVars); }
                Instance.RaiseOnPlayerAttackedMonsterEvent(cData);
                return 0;
            }

            if (cVars.damAfterDT > 0)
            {
                if (FactorInNaturalArmor(attacker, target, weapon, ref cVars)) { Instance.RaiseOnPlayerAttackedMonsterEvent(cData); return 0; }
                else { cData.damAfterNatArmor = cVars.damAfterDT; Instance.RaiseOnPlayerAttackedMonsterEvent(cData); return (int)cVars.damAfterDT; }
            }
            else
            {
                if (cVars.missWasDodge) { PlayRelevantCombatSound(CombatSoundTypes.Dodge, attacker, target, ref cVars); }
                else { PlayRelevantCombatSound(CombatSoundTypes.Miss, attacker, target, ref cVars); }
                Instance.RaiseOnPlayerAttackedMonsterEvent(cData);
                return 0;
            }
        }
        
        public static int CalcMonsterVsPlayerAttack(EnemyEntity attacker, PlayerEntity target, bool enemyAnimStateRecord, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            CVARS cVars = GetCombatVariables(attacker, target);
            cVars.tNatArm = GetPlayerNaturalArmorType(target);
            cVars.tarSize = GetPlayerBodySize(target);
            cVars.tarCareer = GetPlayerCareer(target);
            cVars.atkCareer = GetCreatureCareer(attacker);
            GetMonsterSpecificCombatVariables(false, attacker, ref cVars);

            CDATA cData = GetCombatVarsData(ref cVars);

            if (weapon == null)
            {
                cVars.aUseDummyWep = true;
            }
            else if (weapon != null && cVars.aMonsterWeapon != null)
            {
                if (cVars.aMonsterWeapon.nativeMaterialValue >= weapon.nativeMaterialValue && cVars.aMonsterWeapon.GetBaseDamageMax() >= weapon.GetBaseDamageMax())
                {
                    cVars.aUseDummyWep = true;
                    weapon = null;
                }
            }

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
                    // Use hand-to-hand damage ranges
                    cVars.modDamRange = true;
                }
            }

            if (cVars.aMonsterWeapon != null)
            {
                int weaponAverage = (cVars.aMonsterWeapon.GetBaseDamageMin() + cVars.aMonsterWeapon.GetBaseDamageMax()) / 2;
                int noWeaponAverage = (attacker.MobileEnemy.MinDamage + attacker.MobileEnemy.MaxDamage) / 2;
                if (noWeaponAverage > weaponAverage)
                {
                    // Use hand-to-hand damage ranges
                    cVars.modDamRange = true;
                }
            }

            if (weapon != null)
            {
                cVars.wepType = weapon.GetWeaponSkillIDAsShort();
                cData.aWeapon = weapon.LongName;

                if (softMatRequireModuleCheck)
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                    {
                        int targetMatRequire = (int)target.MinMetalToHit;
                        int weaponMatValue = weapon.NativeMaterialValue;
                        cVars.matReqDamMulti = targetMatRequire - weaponMatValue;

                        if (cVars.matReqDamMulti <= 0) // There is no "bonus" damage for meeting material requirements, nor for exceeding them, just normal unmodded damage.
                            cVars.matReqDamMulti = 1;
                        else // There is a damage penalty for attacking a target with below the minimum material requirements of that target, more as the difference between becomes greater.
                            cVars.matReqDamMulti = (Mathf.Min(cVars.matReqDamMulti * 0.2f, 0.9f) - 1) * -1; // Keeps the damage multiplier penalty from going above 90% reduced damage.
                    }
                }
                else
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                    {
                        PlayRelevantCombatSound(CombatSoundTypes.Mat_Resist, attacker, target, ref cVars);
                        Instance.RaiseOnMonsterAttackedPlayerEvent(cData);
                        return 0;
                    }
                }
            }
            else if (cVars.aMonsterWeapon != null)
            {
                cVars.wepType = cVars.aMonsterWeapon.GetWeaponSkillIDAsShort();
                cData.aWeapon = cVars.aMonsterWeapon.LongName;

                if (softMatRequireModuleCheck)
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)cVars.aMonsterWeapon.nativeMaterialValue)
                    {
                        int targetMatRequire = (int)target.MinMetalToHit;
                        int weaponMatValue = cVars.aMonsterWeapon.nativeMaterialValue;
                        cVars.matReqDamMulti = targetMatRequire - weaponMatValue;

                        if (cVars.matReqDamMulti <= 0) // There is no "bonus" damage for meeting material requirements, nor for exceeding them, just normal unmodded damage.
                            cVars.matReqDamMulti = 1;
                        else // There is a damage penalty for attacking a target with below the minimum material requirements of that target, more as the difference between becomes greater.
                            cVars.matReqDamMulti = (Mathf.Min(cVars.matReqDamMulti * 0.2f, 0.9f) - 1) * -1; // Keeps the damage multiplier penalty from going above 90% reduced damage.
                    }
                }
                else
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)cVars.aMonsterWeapon.nativeMaterialValue)
                    {
                        PlayRelevantCombatSound(CombatSoundTypes.Mat_Resist, attacker, target, ref cVars);
                        Instance.RaiseOnMonsterAttackedPlayerEvent(cData);
                        return 0;
                    }
                }
            }

            cVars.chanceToHitMod = attacker.Skills.GetLiveSkillValue(cVars.wepType);

            cVars.critSuccess = CriticalStrikeHandler(attacker);
            cData.critHit = cVars.critSuccess;

            if (cVars.critSuccess)
            {
                cVars.critDamMulti = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 5);
                cVars.critHitAddi = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 10);

                cVars.critDamMulti = (cVars.critDamMulti * .025f) + 1;
                cVars.chanceToHitMod += cVars.critHitAddi;
            }

            cVars.struckBodyPart = CalculateStruckBodyPart();
            cData.struckBodyPart = cVars.struckBodyPart;

            // Get damage for weaponless attacks
            if (cVars.wepType == (short)DFCareer.Skills.HandToHand)
            {
                cVars.unarmedAttack = true;

                if (attacker.EntityType == EntityTypes.EnemyClass)
                {
                    if (CalculateHitSuccess(attacker, target, ref cVars))
                    {
                        cVars.damage = CalculateHandToHandAttackDamage(attacker, target, cVars.damageModifiers, false); // Added my own, non-overriden version of this method for modification.
                    }
                }
                else // attacker is a monster
                {
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

                        if (DFRandom.rand() % 100 < reflexesChance && minBaseDamage > 0 && CalculateHitSuccess(attacker, target, ref cVars))
                        {
                            cVars.damage += UnityEngine.Random.Range(minBaseDamage, maxBaseDamage + 1);
                        }
                        ++attackNumber;
                    }
                    if (cVars.damage >= 1)
                    {
                        cVars.damage = CalculateHandToHandAttackDamage(attacker, target, cVars.damage, false); // Added my own, non-overriden version of this method for modification.
                        RollMonsterAttackType(ref cVars);
                        cData.attackType = cVars.attackType;
                        cData.attackElement = cVars.attackElement;
                    }
                }
            }
            // Handle weapon attacks
            else if (weapon != null)
            {
                // Apply weapon material modifier.
                cVars.chanceToHitMod += CalculateWeaponToHit(weapon);

                if (CalculateHitSuccess(attacker, target, ref cVars))
                {
                    if (cVars.modDamRange)
                    {
                        cVars.damage = CalculateModifiedWeaponAttackDamage(attacker, target, cVars.damageModifiers, weaponAnimTime, weapon, ref cVars);
                    }
                    else
                    {
                        cVars.damage = CalculateWeaponAttackDamage(attacker, target, cVars.damageModifiers, weaponAnimTime, weapon);
                    }

                    DetermineWeaponAttackType(ref cVars);
                    cData.attackType = cVars.attackType;
                    cData.attackElement = cVars.attackElement;
                }
            }
            else if (cVars.aMonsterWeapon != null)
            {
                // Apply weapon material modifier.
                cVars.chanceToHitMod += CalculateWeaponToHit(cVars.aMonsterWeapon);

                if (CalculateHitSuccess(attacker, target, ref cVars))
                {
                    if (cVars.modDamRange)
                    {
                        cVars.damage = CalculateModifiedWeaponAttackDamage(attacker, target, cVars.damageModifiers, weaponAnimTime, cVars.aMonsterWeapon, ref cVars);
                    }
                    else
                    {
                        cVars.damage = CalculateWeaponAttackDamage(attacker, target, cVars.damageModifiers, weaponAnimTime, cVars.aMonsterWeapon);
                    }

                    DetermineWeaponAttackType(ref cVars);
                    cData.attackType = cVars.attackType;
                    cData.attackElement = cVars.attackElement;
                }
            }

            cVars.damage = Mathf.Max(0, cVars.damage); // I think this is just here to keep damage from outputting a negative value.

            if (cVars.damage <= 0)
            {
                if (cVars.missWasDodge) { PlayRelevantCombatSound(CombatSoundTypes.Dodge, attacker, target, ref cVars); }
                else { PlayRelevantCombatSound(CombatSoundTypes.Miss, attacker, target, ref cVars); }
                Instance.RaiseOnMonsterAttackedPlayerEvent(cData);
                return 0;
            }

            if (cVars.critSuccess)
            {
                cVars.damage = (int)Mathf.Round(cVars.damage * cVars.critDamMulti); // Multiplies 'Final' damage values, before reductions, with the critical damage multiplier.
            }

            cData.initialDam = cVars.damage;

            DaggerfallUnityItem shield = null;
            DaggerfallUnityItem armor = null;

            EvaluateArmorAndShieldCoverage(target, ref cVars, out shield, out armor);

            if (shield != null) { cData.tShield = shield.LongName; }
            if (armor != null) { cData.tArmor = armor.LongName; }

            if (cVars.damage > 0)
            {
                if (weapon != null)
                {
                    if (FactorInArmor(attacker, target, weapon, shield, armor, ref cVars)) { Instance.RaiseOnMonsterAttackedPlayerEvent(cData); return 0; }
                    cData.damAfterArmor = cVars.damAfterDT;
                }
                else
                {
                    if (FactorInArmor(attacker, target, cVars.aMonsterWeapon, shield, armor, ref cVars)) { Instance.RaiseOnMonsterAttackedPlayerEvent(cData); return 0; }
                    cData.damAfterArmor = cVars.damAfterDT;
                }
            }
            else
            {
                if (cVars.missWasDodge) { PlayRelevantCombatSound(CombatSoundTypes.Dodge, attacker, target, ref cVars); }
                else { PlayRelevantCombatSound(CombatSoundTypes.Miss, attacker, target, ref cVars); }
                Instance.RaiseOnMonsterAttackedPlayerEvent(cData);
                return 0;
            }

            if (cVars.damAfterDT > 0)
            {
                if (weapon != null)
                {
                    if (FactorInNaturalArmor(attacker, target, weapon, ref cVars))
                    {
                        Instance.RaiseOnMonsterAttackedPlayerEvent(cData);
                        return 0;
                    }
                    else
                    {
                        cData.damAfterNatArmor = cVars.damAfterDT;
                        ApplyRingOfNamiraEffect(attacker, target, ref cVars);
                        Instance.RaiseOnMonsterAttackedPlayerEvent(cData);
                        return (int)cVars.damAfterDT;
                    }
                }
                else
                {
                    if (FactorInNaturalArmor(attacker, target, cVars.aMonsterWeapon, ref cVars))
                    {
                        Instance.RaiseOnMonsterAttackedPlayerEvent(cData);
                        return 0;
                    }
                    else
                    {
                        cData.damAfterNatArmor = cVars.damAfterDT;
                        ApplyRingOfNamiraEffect(attacker, target, ref cVars);
                        Instance.RaiseOnMonsterAttackedPlayerEvent(cData);
                        return (int)cVars.damAfterDT;
                    }
                }
            }
            else
            {
                if (cVars.missWasDodge) { PlayRelevantCombatSound(CombatSoundTypes.Dodge, attacker, target, ref cVars); }
                else { PlayRelevantCombatSound(CombatSoundTypes.Miss, attacker, target, ref cVars); }
                Instance.RaiseOnMonsterAttackedPlayerEvent(cData);
                return 0;
            }
        }

        public static int CalcMonsterVsMonsterAttack(EnemyEntity attacker, EnemyEntity target, bool enemyAnimStateRecord, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            CVARS cVars = GetCombatVariables(attacker, target);
            cVars.atkCareer = GetCreatureCareer(attacker);
            cVars.tarCareer = GetCreatureCareer(target);
            GetMonsterSpecificCombatVariables(false, attacker, ref cVars);
            GetMonsterSpecificCombatVariables(true, target, ref cVars);

            CDATA cData = GetCombatVarsData(ref cVars);

            if (weapon == null)
            {
                cVars.aUseDummyWep = true;
            }
            else if (weapon != null && cVars.aMonsterWeapon != null)
            {
                if (cVars.aMonsterWeapon.nativeMaterialValue >= weapon.nativeMaterialValue && cVars.aMonsterWeapon.GetBaseDamageMax() >= weapon.GetBaseDamageMax())
                {
                    cVars.aUseDummyWep = true;
                    weapon = null;
                }
            }

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
                    // Use hand-to-hand damage ranges
                    cVars.modDamRange = true;
                }
            }

            if (cVars.aMonsterWeapon != null)
            {
                int weaponAverage = (cVars.aMonsterWeapon.GetBaseDamageMin() + cVars.aMonsterWeapon.GetBaseDamageMax()) / 2;
                int noWeaponAverage = (attacker.MobileEnemy.MinDamage + attacker.MobileEnemy.MaxDamage) / 2;
                if (noWeaponAverage > weaponAverage)
                {
                    // Use hand-to-hand damage ranges
                    cVars.modDamRange = true;
                }
            }

            if (weapon != null)
            {
                cVars.wepType = weapon.GetWeaponSkillIDAsShort();
                cData.aWeapon = weapon.LongName;

                if (softMatRequireModuleCheck)
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                    {
                        int targetMatRequire = (int)target.MinMetalToHit;
                        int weaponMatValue = weapon.NativeMaterialValue;
                        cVars.matReqDamMulti = targetMatRequire - weaponMatValue;

                        if (cVars.matReqDamMulti <= 0) // There is no "bonus" damage for meeting material requirements, nor for exceeding them, just normal unmodded damage.
                            cVars.matReqDamMulti = 1;
                        else // There is a damage penalty for attacking a target with below the minimum material requirements of that target, more as the difference between becomes greater.
                            cVars.matReqDamMulti = (Mathf.Min(cVars.matReqDamMulti * 0.2f, 0.9f) - 1) * -1; // Keeps the damage multiplier penalty from going above 90% reduced damage.
                    }
                }
                else
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                    {
                        PlayRelevantCombatSound(CombatSoundTypes.Mat_Resist, attacker, target, ref cVars);
                        Instance.RaiseOnMonsterAttackedMonsterEvent(cData);
                        return 0;
                    }
                }
            }
            else if (cVars.aMonsterWeapon != null)
            {
                cVars.wepType = cVars.aMonsterWeapon.GetWeaponSkillIDAsShort();
                cData.aWeapon = cVars.aMonsterWeapon.LongName;

                if (softMatRequireModuleCheck)
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)cVars.aMonsterWeapon.nativeMaterialValue)
                    {
                        int targetMatRequire = (int)target.MinMetalToHit;
                        int weaponMatValue = cVars.aMonsterWeapon.nativeMaterialValue;
                        cVars.matReqDamMulti = targetMatRequire - weaponMatValue;

                        if (cVars.matReqDamMulti <= 0) // There is no "bonus" damage for meeting material requirements, nor for exceeding them, just normal unmodded damage.
                            cVars.matReqDamMulti = 1;
                        else // There is a damage penalty for attacking a target with below the minimum material requirements of that target, more as the difference between becomes greater.
                            cVars.matReqDamMulti = (Mathf.Min(cVars.matReqDamMulti * 0.2f, 0.9f) - 1) * -1; // Keeps the damage multiplier penalty from going above 90% reduced damage.
                    }
                }
                else
                {
                    if (target.MinMetalToHit > (WeaponMaterialTypes)cVars.aMonsterWeapon.nativeMaterialValue)
                    {
                        PlayRelevantCombatSound(CombatSoundTypes.Mat_Resist, attacker, target, ref cVars);
                        Instance.RaiseOnMonsterAttackedMonsterEvent(cData);
                        return 0;
                    }
                }
            }

            cVars.chanceToHitMod = attacker.Skills.GetLiveSkillValue(cVars.wepType);

            cVars.critSuccess = CriticalStrikeHandler(attacker);
            cData.critHit = cVars.critSuccess;

            if (cVars.critSuccess)
            {
                cVars.critDamMulti = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 5);
                cVars.critHitAddi = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 10);

                cVars.critDamMulti = (cVars.critDamMulti * .025f) + 1;
                cVars.chanceToHitMod += cVars.critHitAddi;
            }

            cVars.struckBodyPart = CalculateStruckBodyPart();
            cData.struckBodyPart = cVars.struckBodyPart;

            // Get damage for weaponless attacks
            if (cVars.wepType == (short)DFCareer.Skills.HandToHand)
            {
                cVars.unarmedAttack = true;

                if (attacker.EntityType == EntityTypes.EnemyClass)
                {
                    if (CalculateHitSuccess(attacker, target, ref cVars))
                    {
                        cVars.damage = CalculateHandToHandAttackDamage(attacker, target, cVars.damageModifiers, false); // Added my own, non-overriden version of this method for modification.
                    }
                }
                else // attacker is a monster
                {
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

                        if (DFRandom.rand() % 100 < reflexesChance && minBaseDamage > 0 && CalculateHitSuccess(attacker, target, ref cVars))
                        {
                            cVars.damage += UnityEngine.Random.Range(minBaseDamage, maxBaseDamage + 1);
                        }
                        ++attackNumber;
                    }
                    if (cVars.damage >= 1)
                    {
                        cVars.damage = CalculateHandToHandAttackDamage(attacker, target, cVars.damage, false); // Added my own, non-overriden version of this method for modification.
                        RollMonsterAttackType(ref cVars);
                        cData.attackType = cVars.attackType;
                        cData.attackElement = cVars.attackElement;
                    }
                }
            }
            // Handle weapon attacks
            else if (weapon != null)
            {
                // Apply weapon material modifier.
                cVars.chanceToHitMod += CalculateWeaponToHit(weapon);

                if (CalculateHitSuccess(attacker, target, ref cVars))
                {
                    if (cVars.modDamRange)
                    {
                        cVars.damage = CalculateModifiedWeaponAttackDamage(attacker, target, cVars.damageModifiers, weaponAnimTime, weapon, ref cVars);
                    }
                    else
                    {
                        cVars.damage = CalculateWeaponAttackDamage(attacker, target, cVars.damageModifiers, weaponAnimTime, weapon);
                    }

                    DetermineWeaponAttackType(ref cVars);
                    cData.attackType = cVars.attackType;
                    cData.attackElement = cVars.attackElement;
                }
            }
            else if (cVars.aMonsterWeapon != null)
            {
                // Apply weapon material modifier.
                cVars.chanceToHitMod += CalculateWeaponToHit(cVars.aMonsterWeapon);

                if (CalculateHitSuccess(attacker, target, ref cVars))
                {
                    if (cVars.modDamRange)
                    {
                        cVars.damage = CalculateModifiedWeaponAttackDamage(attacker, target, cVars.damageModifiers, weaponAnimTime, cVars.aMonsterWeapon, ref cVars);
                    }
                    else
                    {
                        cVars.damage = CalculateWeaponAttackDamage(attacker, target, cVars.damageModifiers, weaponAnimTime, cVars.aMonsterWeapon);
                    }

                    DetermineWeaponAttackType(ref cVars);
                    cData.attackType = cVars.attackType;
                    cData.attackElement = cVars.attackElement;
                }
            }

            cVars.damage = Mathf.Max(0, cVars.damage); // I think this is just here to keep damage from outputting a negative value.

            if (cVars.damage <= 0)
            {
                if (cVars.missWasDodge) { PlayRelevantCombatSound(CombatSoundTypes.Dodge, attacker, target, ref cVars); }
                else { PlayRelevantCombatSound(CombatSoundTypes.Miss, attacker, target, ref cVars); }
                Instance.RaiseOnMonsterAttackedMonsterEvent(cData);
                return 0;
            }

            if (cVars.critSuccess)
            {
                cVars.damage = (int)Mathf.Round(cVars.damage * cVars.critDamMulti); // Multiplies 'Final' damage values, before reductions, with the critical damage multiplier.
            }

            cData.initialDam = cVars.damage;

            DaggerfallUnityItem shield = null;
            DaggerfallUnityItem armor = null;

            EvaluateArmorAndShieldCoverage(target, ref cVars, out shield, out armor);

            if (shield != null) { cData.tShield = shield.LongName; }
            if (armor != null) { cData.tArmor = armor.LongName; }

            if (cVars.damage > 0)
            {
                if (weapon != null)
                {
                    if (FactorInArmor(attacker, target, weapon, shield, armor, ref cVars)) { Instance.RaiseOnMonsterAttackedMonsterEvent(cData); return 0; }
                    cData.damAfterArmor = cVars.damAfterDT;
                }
                else
                {
                    if (FactorInArmor(attacker, target, cVars.aMonsterWeapon, shield, armor, ref cVars)) { Instance.RaiseOnMonsterAttackedMonsterEvent(cData); return 0; }
                    cData.damAfterArmor = cVars.damAfterDT;
                }
            }
            else
            {
                if (cVars.missWasDodge) { PlayRelevantCombatSound(CombatSoundTypes.Dodge, attacker, target, ref cVars); }
                else { PlayRelevantCombatSound(CombatSoundTypes.Miss, attacker, target, ref cVars); }
                Instance.RaiseOnMonsterAttackedMonsterEvent(cData);
                return 0;
            }

            if (cVars.damAfterDT > 0)
            {
                if (weapon != null)
                {
                    if (FactorInNaturalArmor(attacker, target, weapon, ref cVars))
                    {
                        Instance.RaiseOnMonsterAttackedMonsterEvent(cData);
                        return 0;
                    }
                    else
                    {
                        cData.damAfterNatArmor = cVars.damAfterDT;
                        Instance.RaiseOnMonsterAttackedMonsterEvent(cData);
                        return (int)cVars.damAfterDT;
                    }
                }
                else
                {
                    if (FactorInNaturalArmor(attacker, target, cVars.aMonsterWeapon, ref cVars))
                    {
                        Instance.RaiseOnMonsterAttackedMonsterEvent(cData);
                        return 0;
                    }
                    else
                    {
                        cData.damAfterNatArmor = cVars.damAfterDT;
                        Instance.RaiseOnMonsterAttackedMonsterEvent(cData);
                        return (int)cVars.damAfterDT;
                    }
                }
            }
            else
            {
                if (cVars.missWasDodge) { PlayRelevantCombatSound(CombatSoundTypes.Dodge, attacker, target, ref cVars); }
                else { PlayRelevantCombatSound(CombatSoundTypes.Miss, attacker, target, ref cVars); }
                Instance.RaiseOnMonsterAttackedMonsterEvent(cData);
                return 0;
            }
        }

        /// <summary>Calculates whether an attack on a target is successful or not.</summary>
        public static bool CalculateHitSuccess(DaggerfallEntity attacker, DaggerfallEntity target, ref CVARS cVars)
        {
            if (attacker == null || target == null)
                return false;

            // Get armor value for struck body part
            cVars.tAvoidContrib += CalculateArmorToHit(target, cVars.struckBodyPart);

            // Apply adrenaline rush modifiers.
            cVars.tAvoidContrib += CalculateAdrenalineRushToHit(attacker, target);

            // Apply enchantment modifier. 
            cVars.chanceToHitMod += attacker.ChanceToHitModifier;
            //Debug.LogFormat("Attacker Chance To Hit Mod 'Enchantment' = {0}", attacker.ChanceToHitModifier); // Pretty sure this is from the "bad reactions from:" enchantment effect.

            // Apply stat differential modifiers. (default: luck and agility)
            cVars.tAvoidContrib += CalculateStatDiffsToHit(attacker, target);

            // Apply skill modifiers. (default: dodge and crit strike)
            cVars.tAvoidContrib += CalculateSkillsToHit(attacker, target);
            //Debug.LogFormat("After Dodge = {0}", chanceToHitMod);

            // Apply monster modifier and biography adjustments.
            cVars.tAvoidContrib += CalculateAdjustmentsToHit(attacker, target);
            //Debug.LogFormat("Final chanceToHitMod = {0}", chanceToHitMod);

            int chanceToHit = cVars.chanceToHitMod + cVars.tAvoidContrib;
            int diceRoll = Dice100.Roll();

            if (diceRoll <= cVars.chanceToHitMod && diceRoll > chanceToHit)
                cVars.missWasDodge = true; // If attack roll would have hit if target avoidance was not factored in, then this was a dodge, not just a miss due to attacker incompetence.

            Mathf.Clamp(chanceToHit, 3, 97);

            return Dice100.SuccessRoll(chanceToHit);
        }

        public static void DetermineWeaponAttackType(ref CVARS cVars)
        {
            if (cVars.attackList.Length >= 3 && cVars.aUseDummyWep && cVars.aMonsterWeapon != null)
            {
                if ((AttackType)cVars.attackList[1] != AttackType.Bash)
                {
                    cVars.attackType = (AttackType)cVars.attackList[1];
                    cVars.attackElement = (AttackElementType)cVars.attackList[2];
                    return;
                }
            }

            if (cVars.wepType == (short)DFCareer.Skills.BluntWeapon)
                cVars.attackType = AttackType.Bludgeon;
            else if (cVars.wepType == (short)DFCareer.Skills.LongBlade || cVars.wepType == (short)DFCareer.Skills.Axe)
                cVars.attackType = AttackType.Slash;
            else if (cVars.wepType == (short)DFCareer.Skills.ShortBlade || cVars.wepType == (short)DFCareer.Skills.Archery)
                cVars.attackType = AttackType.Stab;
        }

        public static void RollMonsterAttackType(ref CVARS cVars)
        {
            int[] list = (int[])cVars.attackList.Clone();

            if (list.Length >= 3)
            {
                if (list[0] >= 100)
                {
                    cVars.attackType = (AttackType)list[1];
                    cVars.attackElement = (AttackElementType)list[2];
                    return;
                }

                int roll = Dice100.Roll();
                int totalOdds = 0;
                int numAttackOptions = list.Length / 3;

                for (int i = 0; i < numAttackOptions; i++)
                {
                    totalOdds += list[i * 3];

                    if (roll <= totalOdds)
                    {
                        int attackType = list[i * 3 + 1];
                        int attackElement = list[i * 3 + 2];

                        cVars.attackType = (AttackType)attackType;
                        cVars.attackElement = (AttackElementType)attackElement;
                        return;
                    }
                }

                cVars.attackType = (AttackType)list[1];
                cVars.attackElement = (AttackElementType)list[2];
            }
            else
            {
                cVars.attackType = AttackType.Bash;
                cVars.attackElement = AttackElementType.None;
            }
        }

        public static void ApplyRingOfNamiraEffect(EnemyEntity attacker, PlayerEntity target, ref CVARS cVars)
        {
            if (target == GameManager.Instance.PlayerEntity)
            {
                DaggerfallUnityItem[] equippedItems = target.ItemEquipTable.EquipTable;
                DaggerfallUnityItem item = null;
                if (equippedItems.Length != 0)
                {
                    if (IsRingOfNamira(equippedItems[(int)EquipSlots.Ring0]) || IsRingOfNamira(equippedItems[(int)EquipSlots.Ring1]))
                    {
                        IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects.RingOfNamiraEffect.EffectKey);
                        effectTemplate.EnchantmentPayloadCallback(EnchantmentPayloadFlags.None,
                            targetEntity: attacker.EntityBehaviour,
                            sourceItem: item,
                            sourceDamage: (int)cVars.damAfterDT);
                    }
                }
            }
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

            if (conditionBasedWeaponEffectiveness)
            {
                if (attacker == player)
                    damage = AlterDamageBasedOnWepCondition(damage, true, weapon);
                else
                    damage = AlterDamageBasedOnWepCondition(damage, false, weapon);
            }

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

        public static int CalculateModifiedWeaponAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, int damageModifier, int weaponAnimTime, DaggerfallUnityItem weapon, ref CVARS cVars)
        {
            EnemyEntity AIAttacker = attacker as EnemyEntity;
            int damage = UnityEngine.Random.Range(AIAttacker.MobileEnemy.MinDamage, AIAttacker.MobileEnemy.MaxDamage + 1) + damageModifier;

            PlayerEntity player = GameManager.Instance.PlayerEntity;
            EnemyEntity AITarget = null;

            if (conditionBasedWeaponEffectiveness && !cVars.aUseDummyWep)
            {
                damage = AlterDamageBasedOnWepCondition(damage, false, weapon);
            }

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
                        if (weapon.NativeMaterialValue == (int)WeaponMaterialTypes.Silver) { damage *= 2; }
                        break;
                    default: break;
                }
            }

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

            return damage;
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

        public static void EvaluateArmorAndShieldCoverage(DaggerfallEntity target, ref CVARS cVars, out DaggerfallUnityItem shield, out DaggerfallUnityItem armor)
        {
            EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)cVars.struckBodyPart);
            armor = cVars.tEntity.ItemEquipTable.GetItem(hitSlot);
            if (armor != null)
            {
                cVars.armorDTAmount = GetBaseDTAmount(armor, target, ref cVars);
                cVars.armorDRAmount = GetBaseDRAmount(armor, target, ref cVars);

                cVars.finalDTAmount = cVars.armorDTAmount;
                cVars.finalDRAmount = cVars.armorDRAmount;
            }

            shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand); // Checks if character is using a shield or not.
            if (shield != null)
            {
                BodyParts[] protectedBodyParts = shield.GetShieldProtectedBodyParts();

                if (protectedBodyParts.Length > 0)
                {
                    for (int i = 0; (i < protectedBodyParts.Length) && !cVars.shieldStrongSpot; i++)
                    {
                        if (protectedBodyParts[i] == (BodyParts)cVars.struckBodyPart)
                            cVars.shieldStrongSpot = true;
                    }
                    ShieldBlockChanceCalculation(shield, target, ref cVars);
                }

                if (armor != null)
                {
                    CompareShieldToUnderArmor(shield, armor, ref cVars);
                }
            }
        }

        /// <summary>Checks for if a shield block was successful and returns true if so, false if not.</summary>
        public static void ShieldBlockChanceCalculation(DaggerfallUnityItem shield, DaggerfallEntity owner, ref CVARS cVars)
        {
            float fullBlockChance = 0;

            // Body size difference either gives a bonus or penalty to defender's chances of blocking.
            fullBlockChance -= (cVars.tarSize - cVars.atkSize) * 15f;

            switch (shield.TemplateIndex)
            {
                case (int)Armor.Buckler:
                    fullBlockChance = 35;
                    fullBlockChance += Mathf.Clamp(cVars.tAgil * 0.7f, 0, 35);
                    fullBlockChance += Mathf.Clamp(cVars.tSped * 0.3f, 0, 15);
                    fullBlockChance += Mathf.Clamp((cVars.tAgil - cVars.aAgil) * 0.5f, -50, 0);
                    fullBlockChance += Mathf.Clamp((cVars.tSped - cVars.aSped) * 0.15f, -50, 0); break;
                case (int)Armor.Round_Shield:
                    fullBlockChance = 25;
                    fullBlockChance += Mathf.Clamp(cVars.tStrn * 0.2f, 0, 10);
                    fullBlockChance += Mathf.Clamp(cVars.tAgil * 0.5f, 0, 25);
                    fullBlockChance += Mathf.Clamp(cVars.tSped * 0.2f, 0, 10);
                    fullBlockChance += Mathf.Clamp((cVars.tAgil - cVars.aAgil) * 0.35f, -50, 0);
                    fullBlockChance += Mathf.Clamp((cVars.tSped - cVars.aSped) * 0.2f, -50, 0); break;
                case (int)Armor.Kite_Shield:
                    fullBlockChance = 20;
                    fullBlockChance += Mathf.Clamp(cVars.tStrn * 0.2f, 0, 15);
                    fullBlockChance += Mathf.Clamp(cVars.tEndu * 0.1f, 0, 10);
                    fullBlockChance += Mathf.Clamp(cVars.tAgil * 0.5f, 0, 15);
                    fullBlockChance += Mathf.Clamp(cVars.tSped * 0.2f, 0, 5);
                    fullBlockChance += Mathf.Clamp((cVars.tAgil - cVars.aAgil) * 0.3f, -50, 0);
                    fullBlockChance += Mathf.Clamp((cVars.tSped - cVars.aSped) * 0.25f, -50, 0); break;
                case (int)Armor.Tower_Shield:
                    fullBlockChance = 10;
                    fullBlockChance += Mathf.Clamp(cVars.tStrn * 0.2f, 0, 25);
                    fullBlockChance += Mathf.Clamp(cVars.tEndu * 0.1f, 0, 15);
                    fullBlockChance += Mathf.Clamp(cVars.tAgil * 0.5f, 0, 5);
                    fullBlockChance += Mathf.Clamp((cVars.tAgil - cVars.aAgil) * 0.2f, -50, 0);
                    fullBlockChance += Mathf.Clamp((cVars.tSped - cVars.aSped) * 0.3f, -50, 0); break;
                default:
                    fullBlockChance = 15;
                    fullBlockChance += Mathf.Clamp(cVars.tStrn * 0.2f, 0, 10);
                    fullBlockChance += Mathf.Clamp(cVars.tEndu * 0.1f, 0, 5);
                    fullBlockChance += Mathf.Clamp(cVars.tAgil * 0.5f, 0, 20);
                    fullBlockChance += Mathf.Clamp(cVars.tSped * 0.2f, 0, 10);
                    fullBlockChance += Mathf.Clamp((cVars.tAgil - cVars.aAgil) * 0.2f, -50, 0);
                    fullBlockChance += Mathf.Clamp((cVars.tSped - cVars.aSped) * 0.25f, -50, 0); break;
            }

            fullBlockChance = Mathf.Clamp(fullBlockChance, 0, 90f);
            if (Dice100.SuccessRoll(Mathf.RoundToInt(fullBlockChance))) // Block Success
            {
                float dTMod = 1f;
                cVars.shieldBlockSuccess = true;
                cVars.shieldDTAmount = GetBaseDTAmount(shield, owner, ref cVars);
                cVars.shieldDRAmount = GetBaseDRAmount(shield, owner, ref cVars);
                cVars.shieldDTAmount *= 2;

                if (cVars.tarSize < cVars.atkSize)
                {
                    dTMod -= (cVars.atkSize - cVars.tarSize) * 0.2f;
                }
                dTMod -= (cVars.aStrn - cVars.tStrn) * 0.004f;

                cVars.shieldDTAmount *= dTMod;
            }
            else if (cVars.shieldStrongSpot) // Block Failed, but body-part hit is a hard-point for the shield. Roll for chances of shield taking most of hit rather than armor under it.
            {
                float hitShieldChance = 75 + Mathf.Clamp(cVars.tLuck * 0.5f, -25, 25);
                hitShieldChance -= (cVars.tarSize - cVars.atkSize) * 15f;
                hitShieldChance -= Mathf.Clamp(cVars.aAgil * 0.4f, 0, 20);
                hitShieldChance -= Mathf.Clamp(cVars.aLuck * 0.2f, -10, 10);
                hitShieldChance -= cVars.wepType == (short)DFCareer.Skills.ShortBlade ? 15 : 0;

                hitShieldChance = Mathf.Clamp(hitShieldChance, 10f, 95f);
                if (Dice100.SuccessRoll(Mathf.RoundToInt(hitShieldChance)))
                {
                    float dTMod = 1f;
                    cVars.shieldBlockSuccess = false;
                    cVars.hitShield = true;
                    cVars.shieldDTAmount = GetBaseDTAmount(shield, owner, ref cVars);
                    cVars.shieldDRAmount = GetBaseDRAmount(shield, owner, ref cVars);

                    if (cVars.tarSize < cVars.atkSize)
                    {
                        dTMod -= (cVars.atkSize - cVars.tarSize) * 0.1f;
                    }
                    dTMod -= (cVars.aStrn - cVars.tStrn) * 0.002f;

                    cVars.shieldDTAmount *= dTMod;
                }
                else
                {
                    cVars.shieldBlockSuccess = false;
                    cVars.hitShield = false;
                }
            }
            else // Block Failed and shield completely avoided, hit armor under it.
            {
                cVars.shieldBlockSuccess = false;
                cVars.hitShield = false;
            }
        }

        public static float GetBaseDTAmount(DaggerfallUnityItem armor, DaggerfallEntity owner, ref CVARS cVars)
        {
            float dT = 0f;

            PlayerEntity player = GameManager.Instance.PlayerEntity;

            if (armor.IsShield)
            {
                cVars.shieldMaterial = GetArmorMaterial(armor);

                switch (cVars.shieldMaterial)
                {
                    case (int)ArmorMats.Leather: dT = 3.5f; break;
                    case (int)ArmorMats.Chain: dT = 4.5f; break;
                    case (int)ArmorMats.Iron: dT = 6f; break;
                    case (int)ArmorMats.Steel_Silver: dT = 6.5f; break;
                    case (int)ArmorMats.Elven: dT = 7f; break;
                    case (int)ArmorMats.Dwarven: dT = 7.5f; break;
                    case (int)ArmorMats.Mithril_Adam: dT = 8f; break;
                    case (int)ArmorMats.Ebony: dT = 8.5f; break;
                    case (int)ArmorMats.Orcish: dT = 9f; break;
                    case (int)ArmorMats.Daedric: dT = 9.5f; break;
                    default: dT = 0f; break;
                }

                // Possibly modify somewhat based on the shield-type? Not sure, will see.
                switch (armor.TemplateIndex)
                {
                    case (int)Armor.Buckler:
                        break;
                    case (int)Armor.Round_Shield:
                        break;
                    case (int)Armor.Kite_Shield:
                        break;
                    case (int)Armor.Tower_Shield:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                cVars.armorMaterial = GetArmorMaterial(armor);
                cVars.armorType = GetArmorMatType(armor);

                if (cVars.armorType == 0)
                {
                    switch (cVars.armorMaterial)
                    {
                        case (int)ArmorMats.Leather: dT = 2f; break;
                        case (int)ArmorMats.Chain: dT = 2f; break;
                        case (int)ArmorMats.Iron: dT = 2.25f; break;
                        case (int)ArmorMats.Steel_Silver: dT = 2.5f; break;
                        case (int)ArmorMats.Elven: dT = 2.75f; break;
                        case (int)ArmorMats.Dwarven: dT = 3f; break;
                        case (int)ArmorMats.Mithril_Adam: dT = 3.25f; break;
                        case (int)ArmorMats.Ebony: dT = 3.5f; break;
                        case (int)ArmorMats.Orcish: dT = 3.75f; break;
                        case (int)ArmorMats.Daedric: dT = 4f; break;
                        default: dT = 0f; break;
                    }
                }
                else if (cVars.armorType == 1)
                {
                    switch (cVars.armorMaterial)
                    {
                        case (int)ArmorMats.Leather: dT = 2.5f; break;
                        case (int)ArmorMats.Chain: dT = 2.5f; break;
                        case (int)ArmorMats.Iron: dT = 3f; break;
                        case (int)ArmorMats.Steel_Silver: dT = 3.5f; break;
                        case (int)ArmorMats.Elven: dT = 4f; break;
                        case (int)ArmorMats.Dwarven: dT = 4.5f; break;
                        case (int)ArmorMats.Mithril_Adam: dT = 5f; break;
                        case (int)ArmorMats.Ebony: dT = 5.5f; break;
                        case (int)ArmorMats.Orcish: dT = 6f; break;
                        case (int)ArmorMats.Daedric: dT = 6.5f; break;
                        default: dT = 0f; break;
                    }
                }
                else if (cVars.armorType == 2)
                {
                    switch (cVars.armorMaterial)
                    {
                        case (int)ArmorMats.Leather: dT = 3.5f; break;
                        case (int)ArmorMats.Chain: dT = 3.5f; break;
                        case (int)ArmorMats.Iron: dT = 3.5f; break;
                        case (int)ArmorMats.Steel_Silver: dT = 4.25f; break;
                        case (int)ArmorMats.Elven: dT = 5f; break;
                        case (int)ArmorMats.Dwarven: dT = 5.75f; break;
                        case (int)ArmorMats.Mithril_Adam: dT = 6.5f; break;
                        case (int)ArmorMats.Ebony: dT = 7.25f; break;
                        case (int)ArmorMats.Orcish: dT = 8f; break;
                        case (int)ArmorMats.Daedric: dT = 8.75f; break;
                        default: dT = 0f; break;
                    }
                }
            }

            if (conditionBasedArmorEffectiveness)
            {
                if (owner == player)
                    dT = AlterArmorReducBasedOnItemCondition(dT, true, armor);
                else
                    dT = AlterArmorReducBasedOnItemCondition(dT, false, armor);
            }

            return dT;
        }

        public static float GetBaseDRAmount(DaggerfallUnityItem armor, DaggerfallEntity owner, ref CVARS cVars)
        {
            float dR = 0f;

            PlayerEntity player = GameManager.Instance.PlayerEntity;

            if (armor.IsShield)
            {
                cVars.shieldMaterial = GetArmorMaterial(armor);

                switch (cVars.shieldMaterial)
                {
                    case (int)ArmorMats.Leather: dR = 0.15f; break;
                    case (int)ArmorMats.Chain: dR = 0.2f; break;
                    case (int)ArmorMats.Iron: dR = 0.25f; break;
                    case (int)ArmorMats.Steel_Silver: dR = 0.275f; break;
                    case (int)ArmorMats.Elven: dR = 0.3f; break;
                    case (int)ArmorMats.Dwarven: dR = 0.325f; break;
                    case (int)ArmorMats.Mithril_Adam: dR = 0.35f; break;
                    case (int)ArmorMats.Ebony: dR = 0.375f; break;
                    case (int)ArmorMats.Orcish: dR = 0.4f; break;
                    case (int)ArmorMats.Daedric: dR = 0.425f; break;
                    default: dR = 0f; break;
                }

                // Possibly modify somewhat based on the shield-type? Not sure, will see.
                switch (armor.TemplateIndex)
                {
                    case (int)Armor.Buckler:
                        break;
                    case (int)Armor.Round_Shield:
                        break;
                    case (int)Armor.Kite_Shield:
                        break;
                    case (int)Armor.Tower_Shield:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                cVars.armorMaterial = GetArmorMaterial(armor);
                cVars.armorType = GetArmorMatType(armor);

                if (cVars.armorType == 0)
                {
                    switch (cVars.armorMaterial)
                    {
                        case (int)ArmorMats.Leather: dR = 0f; break;
                        case (int)ArmorMats.Chain: dR = 0f; break;
                        case (int)ArmorMats.Iron: dR = 0.05f; break;
                        case (int)ArmorMats.Steel_Silver: dR = 0.075f; break;
                        case (int)ArmorMats.Elven: dR = 0.1f; break;
                        case (int)ArmorMats.Dwarven: dR = 0.125f; break;
                        case (int)ArmorMats.Mithril_Adam: dR = 0.15f; break;
                        case (int)ArmorMats.Ebony: dR = 0.175f; break;
                        case (int)ArmorMats.Orcish: dR = 0.2f; break;
                        case (int)ArmorMats.Daedric: dR = 0.225f; break;
                        default: dR = 0f; break;
                    }
                }
                else if (cVars.armorType == 1)
                {
                    switch (cVars.armorMaterial)
                    {
                        case (int)ArmorMats.Leather: dR = 0.075f; break;
                        case (int)ArmorMats.Chain: dR = 0.075f; break;
                        case (int)ArmorMats.Iron: dR = 0.1f; break;
                        case (int)ArmorMats.Steel_Silver: dR = 0.125f; break;
                        case (int)ArmorMats.Elven: dR = 0.15f; break;
                        case (int)ArmorMats.Dwarven: dR = 0.175f; break;
                        case (int)ArmorMats.Mithril_Adam: dR = 0.2f; break;
                        case (int)ArmorMats.Ebony: dR = 0.225f; break;
                        case (int)ArmorMats.Orcish: dR = 0.25f; break;
                        case (int)ArmorMats.Daedric: dR = 0.275f; break;
                        default: dR = 0f; break;
                    }
                }
                else if (cVars.armorType == 2)
                {
                    switch (cVars.armorMaterial)
                    {
                        case (int)ArmorMats.Leather: dR = 0.175f; break;
                        case (int)ArmorMats.Chain: dR = 0.175f; break;
                        case (int)ArmorMats.Iron: dR = 0.175f; break;
                        case (int)ArmorMats.Steel_Silver: dR = 0.2f; break;
                        case (int)ArmorMats.Elven: dR = 0.225f; break;
                        case (int)ArmorMats.Dwarven: dR = 0.25f; break;
                        case (int)ArmorMats.Mithril_Adam: dR = 0.275f; break;
                        case (int)ArmorMats.Ebony: dR = 0.3f; break;
                        case (int)ArmorMats.Orcish: dR = 0.325f; break;
                        case (int)ArmorMats.Daedric: dR = 0.35f; break;
                        default: dR = 0f; break;
                    }
                }
            }

            if (conditionBasedArmorEffectiveness)
            {
                if (owner == player)
                    dR = AlterArmorReducBasedOnItemCondition(dR, true, armor);
                else
                    dR = AlterArmorReducBasedOnItemCondition(dR, false, armor);
            }

            return dR;
        }

        // Multiplies the damage of an attack with a weapon, based on the current condition of said weapon, blunt less effected, but also does not benefit as much from higher condition.
        public static int AlterDamageBasedOnWepCondition(int damage, bool isPlayer, DaggerfallUnityItem weapon)
        {
            if (weapon == null) // To attempt to keep object reference compile error from occuring when weapon breaks from an attack.
                return damage;

            int condPerc = weapon.ConditionPercentage;
            float condFactor = 1.0f;

            switch (weapon.GetWeaponSkillUsed())
            {
                default:
                case (int)DFCareer.ProficiencyFlags.HandToHand:
                case (int)DFCareer.ProficiencyFlags.BluntWeapons:
                case (int)DFCareer.ProficiencyFlags.MissileWeapons:
                    if (condPerc >= 92)                         // New
                        condFactor = 1.1f;
                    else if (condPerc <= 91 && condPerc >= 76)  // Almost New
                        condFactor = 1f;
                    else if (condPerc <= 75 && condPerc >= 61)  // Slightly Used
                        condFactor = 1f;
                    else if (condPerc <= 60 && condPerc >= 41)  // Used
                        condFactor = 0.90f;
                    else if (condPerc <= 40 && condPerc >= 16)  // Worn
                        condFactor = 0.80f;
                    else if (condPerc <= 15 && condPerc >= 6)   // Battered
                        condFactor = 0.65f;
                    else if (condPerc <= 5)                     // Useless, Broken
                        condFactor = 0.50f;
                    else                                        // Other
                        condFactor = 1f;
                    break;
                case (int)DFCareer.ProficiencyFlags.ShortBlades:
                case (int)DFCareer.ProficiencyFlags.LongBlades:
                case (int)DFCareer.ProficiencyFlags.Axes:
                    if (condPerc >= 92)                         // New
                        condFactor = 1.3f;
                    else if (condPerc <= 91 && condPerc >= 76)  // Almost New
                        condFactor = 1.1f;
                    else if (condPerc <= 75 && condPerc >= 61)  // Slightly Used
                        condFactor = 1f;
                    else if (condPerc <= 60 && condPerc >= 41)  // Used
                        condFactor = 0.85f;
                    else if (condPerc <= 40 && condPerc >= 16)  // Worn
                        condFactor = 0.70f;
                    else if (condPerc <= 15 && condPerc >= 6)   // Battered
                        condFactor = 0.45f;
                    else if (condPerc <= 5)                     // Useless, Broken
                        condFactor = 0.25f;
                    else                                        // Other
                        condFactor = 1f;
                    break;
            }

            float finalDamage = damage * condFactor;

            // Non-player's equipment effectiveness is only effected half as much as the player, since they can't actively maintain it and such, unlike the player, etc.
            if (!isPlayer)
                finalDamage += (damage - finalDamage) * 0.5f;

            return (int)Mathf.Round(finalDamage);
        }

        // Multiplies the DT or DR of a piece of armor, based on the current condition of said armor.
        public static float AlterArmorReducBasedOnItemCondition(float value, bool isPlayer, DaggerfallUnityItem armor)
        {
            if (armor == null) // To attempt to keep object reference compile error from occuring when worn shield breaks from an attack.
                return 1f;

            int condPerc = armor.ConditionPercentage;
            float condFactor = 1.0f;

            if (condPerc >= 92)                         // New
                condFactor = 1.15f;
            else if (condPerc <= 91 && condPerc >= 76)  // Almost New
                condFactor = 1.05f;
            else if (condPerc <= 75 && condPerc >= 61)  // Slightly Used
                condFactor = 1f;
            else if (condPerc <= 60 && condPerc >= 41)  // Used
                condFactor = 0.90f;
            else if (condPerc <= 40 && condPerc >= 16)  // Worn
                condFactor = 0.80f;
            else if (condPerc <= 15 && condPerc >= 6)   // Battered
                condFactor = 0.65f;
            else if (condPerc <= 5)                     // Useless, Broken
                condFactor = 0.50f;
            else                                        // Other
                condFactor = 1f;

            float finalRed = value * condFactor;

            // Non-player's equipment effectiveness is only effected half as much as the player, since they can't actively maintain it and such, unlike the player, etc.
            if (!isPlayer)
                finalRed += (value - finalRed) * 0.5f;

            return finalRed;
        }

        /// <summary>Compares the damage reduction of the struck shield, with the armor under the part that was struck. This is to keep a full-suit of daedric armor from being worse while wearing a leather shield, which when a block is successful, would actually take more damage than if not wearing a shield.</summary>
        public static void CompareShieldToUnderArmor(DaggerfallUnityItem shield, DaggerfallUnityItem armor, ref CVARS cVars)
        {
            // Will potentially change this later, so that the shield always gets hit if it does, but add the under-armor amount to it or something to compensate? Will see.
            if (cVars.armorDTAmount >= cVars.shieldDTAmount)
            {
                cVars.finalDTAmount = cVars.armorDTAmount;
                cVars.shieldBlockSuccess = false;
                cVars.hitShield = false;
            }
            else
            {
                cVars.finalDTAmount = cVars.shieldDTAmount;
            }

            if (cVars.armorDRAmount >= cVars.shieldDRAmount)
            {
                cVars.finalDRAmount = cVars.armorDRAmount;
                cVars.shieldBlockSuccess = false;
                cVars.hitShield = false;
            }
            else
            {
                cVars.finalDRAmount = cVars.shieldDRAmount;
            }
        }

        /// <summary>Finds the material that an armor item is made from, then returns the multiplier that will be used later based on this material check.</summary>
        public static void ArmorMaterialIdentifier(DaggerfallUnityItem armor, ref int[] armorProps)
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

        public static int GetArmorMatType(DaggerfallUnityItem armor)
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

        public static int GetArmorMaterial(DaggerfallUnityItem armor)
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

        public static int GetWeaponMaterial(DaggerfallUnityItem weapon)
        {
            if (weapon != null)
            {
                int mat = weapon.NativeMaterialValue;

                switch (mat)
                {
                    case (int)WeaponMaterialTypes.Iron:
                        return 0;
                    case (int)WeaponMaterialTypes.Steel:
                    case (int)WeaponMaterialTypes.Silver:
                        return 1;
                    case (int)WeaponMaterialTypes.Elven:
                        return 2;
                    case (int)WeaponMaterialTypes.Dwarven:
                        return 3;
                    case (int)WeaponMaterialTypes.Mithril:
                    case (int)WeaponMaterialTypes.Adamantium:
                        return 4;
                    case (int)WeaponMaterialTypes.Ebony:
                        return 5;
                    case (int)WeaponMaterialTypes.Orcish:
                        return 6;
                    case (int)WeaponMaterialTypes.Daedric:
                        return 7;
                    default:
                        return -1;
                }
            }
            return -1;
        }

        /// <summary>Retrieves the multiplier based on the condition modifier of a material, the idea being that items will take around the same amount of damage as other items in that category.</summary>
        public static int EqualizeMaterialConditions(DaggerfallUnityItem item)
        {
            int itemMat = item.NativeMaterialValue;

            if (itemMat <= 9 && itemMat >= 0) // Checks if the item material is for weapons, and leather armor.
            {
                switch (itemMat)
                {
                    case (int)WeaponMaterialTypes.Iron:
                    case (int)WeaponMaterialTypes.Steel:
                    case (int)WeaponMaterialTypes.Silver:
                        return 1;
                    case (int)WeaponMaterialTypes.Elven:
                        return 2;
                    case (int)WeaponMaterialTypes.Dwarven:
                        return 3;
                    case (int)WeaponMaterialTypes.Mithril:
                        return 4;
                    case (int)WeaponMaterialTypes.Adamantium:
                        return 5;
                    case (int)WeaponMaterialTypes.Ebony:
                        return 6;
                    case (int)WeaponMaterialTypes.Orcish:
                        return 7;
                    case (int)WeaponMaterialTypes.Daedric:
                        return 8;
                    default:
                        return 1; // Leather should default to this.
                }
            }
            else if (itemMat <= 521 && itemMat >= 256) // Checks if the item material is for armors.
            {
                switch (itemMat)
                {
                    case (int)ArmorMaterialTypes.Chain:
                    case (int)ArmorMaterialTypes.Chain2:
                    case (int)ArmorMaterialTypes.Iron:
                    case (int)ArmorMaterialTypes.Steel:
                    case (int)ArmorMaterialTypes.Silver:
                        return 1;
                    case (int)ArmorMaterialTypes.Elven:
                        return 2;
                    case (int)ArmorMaterialTypes.Dwarven:
                        return 3;
                    case (int)ArmorMaterialTypes.Mithril:
                        return 4;
                    case (int)ArmorMaterialTypes.Adamantium:
                        return 5;
                    case (int)ArmorMaterialTypes.Ebony:
                        return 6;
                    case (int)ArmorMaterialTypes.Orcish:
                        return 7;
                    case (int)ArmorMaterialTypes.Daedric:
                        return 8;
                    default:
                        return 1;
                }
            }
            else
                return 1;
        }

        /// <summary>
        /// Types of combat sound categories.
        /// </summary>
        public enum CombatSoundTypes
        {
            None = -1,
            Miss = 0,
            Dodge = 1,
            Mat_Resist = 2,
            Full_Act_Block = 3,
            Full_Pas_Block = 4,
            Full_Neg_Metal_Armor = 5,
            Full_Neg_Chain_Armor = 6,
            Full_Neg_Leather_Armor = 7,
            Part_Act_Block = 8,
            Part_Pas_Block = 9,
            Part_Neg_Metal_Armor = 10,
            Part_Neg_Chain_Armor = 11,
            Part_Neg_Leather_Armor = 12,
            Full_Neg_Flesh = 13,
            Full_Neg_Fur = 14,
            Full_Neg_Scale = 15,
            Full_Neg_Bone = 16,
            Full_Neg_Rock = 17,
            Full_Neg_Metal = 18,
            Attack_Hit_Flesh = 19,
            Attack_Hit_Fur = 20,
            Attack_Hit_Scale = 21,
            Attack_Hit_Bone = 22,
            Attack_Hit_Rock = 23,
            Attack_Hit_Metal = 24,
            Attack_Hit_Ethereal = 25,
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

        /// <summary>
        /// The vanilla armor materials, but in an index format more useful for this mod's logic.
        /// </summary>
        public enum ArmorMats
        {
            None = -1,
            Leather = 0,
            Chain = 1,
            Iron = 2,
            Steel_Silver = 3,
            Elven = 4,
            Dwarven = 5,
            Mithril_Adam = 6,
            Ebony = 7,
            Orcish = 8,
            Daedric = 9,
        }

        /// <summary>
        /// The modded armor materials 'types', added by Roleplay Realism: Items.
        /// </summary>
        public enum ArmorType
        {
            None = -1,
            Leather = 0,
            Chain = 1,
            Plate = 2,
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

        public static bool FactorInArmor(DaggerfallEntity attacker, DaggerfallEntity target, DaggerfallUnityItem weapon, DaggerfallUnityItem shield, DaggerfallUnityItem armor, ref CVARS cVars)
        {
            if (cVars.finalDTAmount > 0 || cVars.finalDRAmount > 0)
            {
                float damAfterDR = cVars.damage * Mathf.Abs(cVars.finalDRAmount - 1);
                float dTAfterRound = cVars.finalDTAmount;
                float damRemainder = damAfterDR % 1;
                float dTRemainder = cVars.finalDTAmount % 1;

                damAfterDR = (float)Math.Truncate(damAfterDR);
                if (Dice100.SuccessRoll((int)Mathf.Clamp(Mathf.Floor(damRemainder * 100 * ((cVars.aLuck * .02f) + 1)), 0, 100)))
                    ++damAfterDR;

                dTAfterRound = (float)Math.Truncate(dTAfterRound);
                if (Dice100.SuccessRoll((int)Mathf.Clamp(Mathf.Floor(dTRemainder * 100 * ((cVars.tLuck * .02f) + 1)), 0, 100)))
                    ++dTAfterRound;

                cVars.damBeforeDT = damAfterDR;
                cVars.damAfterDT = Mathf.Max(damAfterDR - dTAfterRound, 0);

                if (dTAfterRound >= damAfterDR) // Attack was completely negated by shield or armor.
                {
                    if (cVars.shieldBlockSuccess)
                    {
                        if (shield != null)
                        {
                            DamageEquipment(weapon, shield, attacker, target, ref cVars);
                            PlayRelevantCombatSound(CombatSoundTypes.Full_Act_Block, attacker, target, ref cVars);
                            // Also maybe animation when that is a thing?
                            return true;
                        }
                    }
                    else if (cVars.hitShield)
                    {
                        if (shield != null)
                        {
                            DamageEquipment(weapon, shield, attacker, target, ref cVars);
                            PlayRelevantCombatSound(CombatSoundTypes.Full_Pas_Block, attacker, target, ref cVars);
                            return true;
                        }
                    }
                    else
                    {
                        if (armor != null)
                        {
                            DamageEquipment(weapon, armor, attacker, target, ref cVars);
                            if (cVars.armorType == 2)
                                PlayRelevantCombatSound(CombatSoundTypes.Full_Neg_Metal_Armor, attacker, target, ref cVars);
                            else if (cVars.armorType == 1)
                                PlayRelevantCombatSound(CombatSoundTypes.Full_Neg_Chain_Armor, attacker, target, ref cVars);
                            else
                                PlayRelevantCombatSound(CombatSoundTypes.Full_Neg_Leather_Armor, attacker, target, ref cVars);
                            return true;
                        }
                    }
                }
                else // Attack was only partially reduced by shield or armor, so the DT value was overcome.
                {
                    if (cVars.shieldBlockSuccess)
                    {
                        if (shield != null)
                        {
                            DamageEquipment(weapon, shield, attacker, target, ref cVars);
                            PlayRelevantCombatSound(CombatSoundTypes.Part_Act_Block, attacker, target, ref cVars);
                            // Also maybe animation when that is a thing?
                            return false;
                        }
                    }
                    else if (cVars.hitShield)
                    {
                        if (shield != null)
                        {
                            DamageEquipment(weapon, shield, attacker, target, ref cVars);
                            PlayRelevantCombatSound(CombatSoundTypes.Part_Pas_Block, attacker, target, ref cVars);
                            return false;
                        }
                    }
                    else
                    {
                        if (armor != null)
                        {
                            DamageEquipment(weapon, armor, attacker, target, ref cVars);
                            if (cVars.armorType == 2)
                                PlayRelevantCombatSound(CombatSoundTypes.Part_Neg_Metal_Armor, attacker, target, ref cVars);
                            else if (cVars.armorType == 1)
                                PlayRelevantCombatSound(CombatSoundTypes.Part_Neg_Chain_Armor, attacker, target, ref cVars);
                            else
                                PlayRelevantCombatSound(CombatSoundTypes.Part_Neg_Leather_Armor, attacker, target, ref cVars);
                            return false;
                        }
                    }
                }
            }
            else
            {
                // I think here if damage was dealt, but there was no armor or shield to reduce any of it.
                cVars.damBeforeDT = cVars.damage;
                cVars.damAfterDT = cVars.damage;

                DamageEquipment(weapon, armor, attacker, target, ref cVars);
                return false;
            }
            return false;
        }

        public static bool FactorInNaturalArmor(DaggerfallEntity attacker, DaggerfallEntity target, DaggerfallUnityItem weapon, ref CVARS cVars)
        {
            float damAfterDR = cVars.damAfterDT;

            if (softMatRequireModuleCheck)
            {
                if (cVars.tNatArm == NaturalArmorType.Ethereal)
                {
                    if (weapon != null)
                    {
                        if (weapon.NativeMaterialValue == (int)WeaponMaterialTypes.Iron || weapon.NativeMaterialValue == (int)WeaponMaterialTypes.Steel)
                        {
                            if (cVars.matReqDamMulti > 0.4f)
                            {
                                cVars.matReqDamMulti = 0.4f; // Simple materials do much less damage to incorporeal creatures.
                            }
                        }
                    }
                }

                float damCheckBeforeMatMod = damAfterDR;

                damAfterDR *= cVars.matReqDamMulti;

                float damCheckAfterMatMod = damAfterDR;

                if (damAfterDR < 1 && damAfterDR > 0)
                    damAfterDR = damAfterDR >= 0.5f ? 1 : 0;

                if (damCheckBeforeMatMod > 0 && (damCheckAfterMatMod / damCheckBeforeMatMod) <= 0.45f)
                    DaggerfallUI.AddHUDText("This Weapon Is Not Very Effective Against This Creature.", 2.00f);
            }

            if (damAfterDR > 0)
            {
                if (cVars.tNatDT > 0 || cVars.tBluntMulti > 0 || cVars.tSlashMulti > 0 || cVars.tPierceMulti > 0)
                {
                    if (cVars.unarmedAttack || cVars.wepType == (short)DFCareer.Skills.HandToHand || cVars.wepType == (short)DFCareer.Skills.BluntWeapon)
                        damAfterDR *= cVars.tBluntMulti;
                    else if (cVars.wepType == (short)DFCareer.Skills.LongBlade || cVars.wepType == (short)DFCareer.Skills.Axe)
                        damAfterDR *= cVars.tSlashMulti;
                    else if (cVars.wepType == (short)DFCareer.Skills.ShortBlade || cVars.wepType == (short)DFCareer.Skills.Archery)
                        damAfterDR *= cVars.tPierceMulti;

                    float dTAfterRound = cVars.tNatDT;
                    float damRemainder = damAfterDR % 1;
                    float dTRemainder = cVars.tNatDT % 1;

                    damAfterDR = (float)Math.Truncate(damAfterDR);
                    if (Dice100.SuccessRoll((int)Mathf.Clamp(Mathf.Floor(damRemainder * 100 * ((cVars.aLuck * .02f) + 1)), 0, 100)))
                        ++damAfterDR;

                    dTAfterRound = (float)Math.Truncate(dTAfterRound);
                    if (Dice100.SuccessRoll((int)Mathf.Clamp(Mathf.Floor(dTRemainder * 100 * ((cVars.tLuck * .02f) + 1)), 0, 100)))
                        ++dTAfterRound;

                    cVars.damBeforeDT = damAfterDR;
                    cVars.damAfterDT = Mathf.Max(damAfterDR - dTAfterRound, 0);

                    if (dTAfterRound >= damAfterDR) // Attack was completely negated by natural armor.
                    {
                        DamageEquipment(weapon, null, attacker, target, ref cVars);

                        switch (cVars.tNatArm)
                        {
                            default:
                            case NaturalArmorType.Flesh: PlayRelevantCombatSound(CombatSoundTypes.Full_Neg_Flesh, attacker, target, ref cVars); break;
                            case NaturalArmorType.Fur: PlayRelevantCombatSound(CombatSoundTypes.Full_Neg_Fur, attacker, target, ref cVars); break;
                            case NaturalArmorType.Scale: PlayRelevantCombatSound(CombatSoundTypes.Full_Neg_Scale, attacker, target, ref cVars); break;
                            case NaturalArmorType.Bone: PlayRelevantCombatSound(CombatSoundTypes.Full_Neg_Bone, attacker, target, ref cVars); break;
                            case NaturalArmorType.Rock: PlayRelevantCombatSound(CombatSoundTypes.Full_Neg_Rock, attacker, target, ref cVars); break;
                            case NaturalArmorType.Metal: PlayRelevantCombatSound(CombatSoundTypes.Full_Neg_Metal, attacker, target, ref cVars); break;
                        }

                        return true;
                    }
                    else // Attack was only partially reduced by natural armor, so the DT value was overcome.
                    {
                        DamageEquipment(weapon, null, attacker, target, ref cVars);

                        switch (cVars.tNatArm)
                        {
                            default:
                            case NaturalArmorType.Flesh: PlayRelevantCombatSound(CombatSoundTypes.Attack_Hit_Flesh, attacker, target, ref cVars); break;
                            case NaturalArmorType.Fur: PlayRelevantCombatSound(CombatSoundTypes.Attack_Hit_Fur, attacker, target, ref cVars); break;
                            case NaturalArmorType.Scale: PlayRelevantCombatSound(CombatSoundTypes.Attack_Hit_Scale, attacker, target, ref cVars); break;
                            case NaturalArmorType.Bone: PlayRelevantCombatSound(CombatSoundTypes.Attack_Hit_Bone, attacker, target, ref cVars); break;
                            case NaturalArmorType.Rock: PlayRelevantCombatSound(CombatSoundTypes.Attack_Hit_Rock, attacker, target, ref cVars); break;
                            case NaturalArmorType.Metal: PlayRelevantCombatSound(CombatSoundTypes.Attack_Hit_Metal, attacker, target, ref cVars); break;
                            case NaturalArmorType.Ethereal: PlayRelevantCombatSound(CombatSoundTypes.Attack_Hit_Ethereal, attacker, target, ref cVars); break;
                        }

                        // Handle poisoned weapons
                        if (weapon != null && weapon.poisonType != Poisons.None)
                        {
                            FormulaHelper.InflictPoison(attacker, target, weapon.poisonType, false);
                            weapon.poisonType = Poisons.None;
                        }

                        cVars.damAfterDT = Mathf.Max(1, Mathf.RoundToInt(cVars.damAfterDT));

                        if (attacker != GameManager.Instance.PlayerEntity && attacker.EntityBehaviour.EntityType != EntityTypes.EnemyClass)
                            FormulaHelper.OnMonsterHit((EnemyEntity)attacker, target, (int)cVars.damAfterDT);

                        return false;
                    }
                }
                else
                {
                    // I think here if damage was dealt, but no natural armor or resistances to reduce it.
                    cVars.damBeforeDT = cVars.damage;
                    cVars.damAfterDT = cVars.damage;

                    DamageEquipment(weapon, null, attacker, target, ref cVars);

                    switch (cVars.tNatArm)
                    {
                        default:
                        case NaturalArmorType.Flesh: PlayRelevantCombatSound(CombatSoundTypes.Attack_Hit_Flesh, attacker, target, ref cVars); break;
                        case NaturalArmorType.Fur: PlayRelevantCombatSound(CombatSoundTypes.Attack_Hit_Fur, attacker, target, ref cVars); break;
                        case NaturalArmorType.Scale: PlayRelevantCombatSound(CombatSoundTypes.Attack_Hit_Scale, attacker, target, ref cVars); break;
                        case NaturalArmorType.Bone: PlayRelevantCombatSound(CombatSoundTypes.Attack_Hit_Bone, attacker, target, ref cVars); break;
                        case NaturalArmorType.Rock: PlayRelevantCombatSound(CombatSoundTypes.Attack_Hit_Rock, attacker, target, ref cVars); break;
                        case NaturalArmorType.Metal: PlayRelevantCombatSound(CombatSoundTypes.Attack_Hit_Metal, attacker, target, ref cVars); break;
                        case NaturalArmorType.Ethereal: PlayRelevantCombatSound(CombatSoundTypes.Attack_Hit_Ethereal, attacker, target, ref cVars); break;
                    }

                    // Handle poisoned weapons
                    if (weapon != null && weapon.poisonType != Poisons.None)
                    {
                        FormulaHelper.InflictPoison(attacker, target, weapon.poisonType, false);
                        weapon.poisonType = Poisons.None;
                    }

                    cVars.damAfterDT = Mathf.Max(1, Mathf.RoundToInt(cVars.damAfterDT));

                    if (attacker != GameManager.Instance.PlayerEntity && attacker.EntityBehaviour.EntityType != EntityTypes.EnemyClass)
                        FormulaHelper.OnMonsterHit((EnemyEntity)attacker, target, (int)cVars.damAfterDT);

                    return false;
                }
            }
            else
            {
                PlayRelevantCombatSound(CombatSoundTypes.Mat_Resist, attacker, target, ref cVars);
                return true;
            }
        }

        /// <summary>Allocate any equipment damage from a strike, and reduce item condition.</summary>
        private static void DamageEquipment(DaggerfallUnityItem weapon, DaggerfallUnityItem armor, DaggerfallEntity attacker, DaggerfallEntity target, ref CVARS cVars)
        {
            // Will obviously need to work on this more later, for stuff like different armor types, weapon types, and even how much using a shield and blocking completely changes the condition damage caused, etc.

            ItemCollection attackerItems = attacker.Items;
            ItemCollection targetItems = target.Items;

            float damRedByDT = Mathf.Abs(cVars.damBeforeDT - cVars.damAfterDT);
            float damToBody = Mathf.Abs(cVars.damAfterDT);
            float matDiffArmor = 0;
            float matDiffBody = 0;

            if (cVars.aUseDummyWep && cVars.aMonsterWeapon != null)
            {
                if (cVars.wepType == (short)Skills.Archery)
                {
                    // Likely won't do any damage to the bow, that should happen when fired, hit or miss.
                }
                else if (cVars.wepType == (short)Skills.BluntWeapon)
                {
                    matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(cVars.aMonsterWeapon) + ((cVars.aStrn + 50) * 0.04f) - 2f) : 0;
                }
                else if (cVars.wepType == (short)Skills.Axe)
                {
                    matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(cVars.aMonsterWeapon) + ((cVars.aStrn + 50) * 0.03f)) : 0;
                }
                else if (cVars.wepType == (short)Skills.LongBlade)
                {
                    matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(cVars.aMonsterWeapon) + ((cVars.aStrn + 50) * 0.02f)) : 0;
                }
                else if (cVars.wepType == (short)Skills.ShortBlade)
                {
                    matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(cVars.aMonsterWeapon) + ((cVars.aStrn + 50) * 0.013f)) : 0;
                }
                else
                {
                    matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(cVars.aMonsterWeapon) + ((cVars.aStrn + 50) * 0.02f)) : 0;
                }
            }
            else if (weapon != null)
            {
                if (cVars.wepType == (short)Skills.Archery)
                {
                    // Likely won't do any damage to the bow, that should happen when fired, hit or miss.
                }
                else if (cVars.wepType == (short)Skills.BluntWeapon)
                {
                    matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(weapon) + ((cVars.aStrn + 50) * 0.04f) - 2f) : 0;
                    float conDamModArmor = Mathf.Clamp(1f + (matDiffArmor * 0.2f), 0.3f, 2.4f);
                    int damByDT = Mathf.Max(Mathf.RoundToInt(damRedByDT * conDamModArmor), 0);

                    matDiffBody = cVars.tArmHardness - (GetWeaponMaterial(weapon) + ((cVars.aStrn + 50) * 0.02f));
                    float conDamModBody = matDiffBody > 0 ? Mathf.Min(0.7f + (matDiffArmor * 0.35f), 2.7f) : Mathf.Max(0.7f + (matDiffArmor * 0.15f), 0.1f);
                    int damByBody = Mathf.Max(Mathf.RoundToInt(damToBody * conDamModBody), 0);

                    int totalConditionDam = damByDT + damByBody;

                    HandleItemConditionDamage(weapon, attacker, attackerItems, totalConditionDam);
                    // Likely will mostly do damage to armor based on weight of the weapon, also probably do less damage to blunt weapon condition compared to blades.
                }
                else if (cVars.wepType == (short)Skills.Axe)
                {
                    matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(weapon) + ((cVars.aStrn + 50) * 0.03f)) : 0;
                    float conDamModArmor = Mathf.Clamp(1f + (matDiffArmor * 0.2f), 0.3f, 2.7f);
                    int damByDT = Mathf.Max(Mathf.RoundToInt(damRedByDT * conDamModArmor), 0);

                    matDiffBody = cVars.tArmHardness - (GetWeaponMaterial(weapon) + ((cVars.aStrn + 50) * 0.015f));
                    float conDamModBody = matDiffBody > 0 ? Mathf.Min(0.5f + (matDiffArmor * 0.25f), 3.5f) : Mathf.Max(0.5f + (matDiffArmor * 0.1f), 0.1f);
                    int damByBody = Mathf.Max(Mathf.RoundToInt(damToBody * conDamModBody), 0);

                    int totalConditionDam = damByDT + damByBody;

                    HandleItemConditionDamage(weapon, attacker, attackerItems, totalConditionDam);
                    // Likely an in-between of blunt and longblades in terms of condition damage characteristics.
                }
                else if (cVars.wepType == (short)Skills.LongBlade)
                {
                    matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(weapon) + ((cVars.aStrn + 50) * 0.02f)) : 0;
                    float conDamModArmor = Mathf.Clamp(1f + (matDiffArmor * 0.2f), 0.3f, 3f);
                    int damByDT = Mathf.Max(Mathf.RoundToInt(damRedByDT * conDamModArmor), 0);

                    matDiffBody = cVars.tArmHardness - (GetWeaponMaterial(weapon) + ((cVars.aStrn + 50) * 0.01f));
                    float conDamModBody = matDiffBody > 0 ? Mathf.Min(0.4f + (matDiffArmor * 0.2f), 5f) : Mathf.Max(0.4f + (matDiffArmor * 0.075f), 0.1f);
                    int damByBody = Mathf.Max(Mathf.RoundToInt(damToBody * conDamModBody), 0);

                    int totalConditionDam = damByDT + damByBody;

                    HandleItemConditionDamage(weapon, attacker, attackerItems, totalConditionDam);
                }
                else if (cVars.wepType == (short)Skills.ShortBlade)
                {
                    matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(weapon) + ((cVars.aStrn + 50) * 0.013f)) : 0;
                    float conDamModArmor = Mathf.Clamp(1f + (matDiffArmor * 0.2f), 0.3f, 2.4f);
                    int damByDT = Mathf.Max(Mathf.RoundToInt(damRedByDT * conDamModArmor), 0);

                    matDiffBody = cVars.tArmHardness - (GetWeaponMaterial(weapon) + ((cVars.aStrn + 50) * 0.0065f));
                    float conDamModBody = matDiffBody > 0 ? Mathf.Min(0.3f + (matDiffArmor * 0.15f), 4f) : Mathf.Max(0.3f + (matDiffArmor * 0.05f), 0.1f);
                    int damByBody = Mathf.Max(Mathf.RoundToInt(damToBody * conDamModBody), 0);

                    int totalConditionDam = damByDT + damByBody;

                    HandleItemConditionDamage(weapon, attacker, attackerItems, totalConditionDam);
                }
                else
                {
                    matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(weapon) + ((cVars.aStrn + 50) * 0.02f)) : 0;
                    float conDamModArmor = Mathf.Clamp(1f + (matDiffArmor * 0.2f), 0.3f, 3f);
                    int damByDT = Mathf.Max(Mathf.RoundToInt(damRedByDT * conDamModArmor), 0);

                    matDiffBody = cVars.tArmHardness - (GetWeaponMaterial(weapon) + ((cVars.aStrn + 50) * 0.01f));
                    float conDamModBody = matDiffBody > 0 ? Mathf.Min(0.4f + (matDiffArmor * 0.2f), 5f) : Mathf.Max(0.4f + (matDiffArmor * 0.075f), 0.1f);
                    int damByBody = Mathf.Max(Mathf.RoundToInt(damToBody * conDamModBody), 0);

                    int totalConditionDam = damByDT + damByBody;

                    HandleItemConditionDamage(weapon, attacker, attackerItems, totalConditionDam);
                    //
                    // I'm thinking for the damage caused due to damaging the actual target, whatever is under the armor. Maybe have some mostly arbitrary case-switch method for all enemies,
                    // and just define what "tier" of material their skin/body is made from and use that as a way to determine how much condition damage the weapon takes, similar to armor, etc.
                    // Suppose could also modify it slightly depending on their strength and endurance stats or something also.
                    // Now with the vanilla enemies "body materials" defined, I guess I can continue on whatever this part was tomorrow, probably.
                }
            }
            else // Handles Unarmed attacks.
            {
                switch (cVars.attackType)
                {
                    case AttackType.Bash:
                    case AttackType.Bludgeon:
                    case AttackType.Elemental_Bludgeon:
                    case AttackType.Maul:
                    case AttackType.Kick:
                        matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(cVars.aMonsterWeapon) + ((cVars.aStrn + 50) * 0.06f) - 4f) : 0; break;
                    case AttackType.Slash:
                    case AttackType.Elemental_Slash:
                    case AttackType.Claw:
                    case AttackType.Scratch:
                        matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(cVars.aMonsterWeapon) + ((cVars.aStrn + 50) * 0.03f) - 2f) : 0; break;
                    case AttackType.Stab:
                    case AttackType.Elemental_Stab:
                    case AttackType.Sting:
                        matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(cVars.aMonsterWeapon) + ((cVars.aStrn + 50) * 0.018f) - 1.5f) : 0; break;
                    case AttackType.Bite:
                        matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(cVars.aMonsterWeapon) + ((cVars.aStrn + 50) * 0.0525f) - 3.25f) : 0; break;
                    case AttackType.Pinch:
                        matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(cVars.aMonsterWeapon) + ((cVars.aStrn + 50) * 0.045f) - 2f) : 0; break;
                    default:
                        matDiffArmor = armor != null ? GetArmorMaterial(armor) - (GetWeaponMaterial(cVars.aMonsterWeapon) + ((cVars.aStrn + 50) * 0.03f) - 2f) : 0; break;
                }
            }

            if (armor != null)
            {
                if (cVars.attackType == AttackType.Ethereal) // I'm thinking now atleast, that Ethereal attacks will possibly go through armor, but also not deal any condition damage to said armor, etc.
                    return;

                // When I get to the mod compatibility stuff, I'll want to take Roleplay Realism: Items armor types into account, most likely.
                float conDamMod = Mathf.Clamp(1f + (-1f * matDiffArmor * 0.3f), 0.3f, 4f);
                int armorDam = Mathf.Max(Mathf.RoundToInt(damRedByDT * conDamMod), 0);
                HandleItemConditionDamage(armor, target, targetItems, armorDam);
                //
            }
        }

        /// <summary>Handles the actual work of damaging the condition of an item.</summary>
        private static void HandleItemConditionDamage(DaggerfallUnityItem item, DaggerfallEntity owner, ItemCollection ownerItems, int damValue)
        {
            if (item != null && owner != null)
            {
                if (damValue > 0)
                {
                    if (item.IsEnchanted && ownerItems != null) // If the Weapon or Armor piece is enchanted, when broken it will be Destroyed from the owner's inventory.
                        item.LowerCondition(damValue, owner, ownerItems);
                    else
                        item.LowerCondition(damValue, owner);
                }
            }
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

        private static bool IsRingOfNamira(DaggerfallUnityItem item)
        {
            return item != null && item.ContainsEnchantment(EnchantmentTypes.SpecialArtifactEffect, (int)ArtifactsSubTypes.Ring_of_Namira);
        }

        #endregion

        public static bool CoinFlip()
        {
            if (UnityEngine.Random.Range(0, 1 + 1) == 0)
                return false;
            else
                return true;
        }

        public static void PlayRelevantCombatSound(CombatSoundTypes soundType, DaggerfallEntity attacker, DaggerfallEntity target, ref CVARS cVars)
        {
            AudioClip[] clips = GetSoundClipList(soundType, ref cVars);

            if (clips.Length == 0)
                return;

            AudioClip clip = RollRandomCombatAudioClip(clips);

            DaggerfallAudioSource dfAudioSource = null;

            if (target != null)
            {
                if (target == GameManager.Instance.PlayerEntity)
                    dfAudioSource = GameManager.Instance.WeaponManager.ScreenWeapon.gameObject.GetComponent<DaggerfallAudioSource>();
                else
                {
                    EnemySounds enemySounds = target.EntityBehaviour.gameObject.GetComponent<EnemySounds>(); // Tomorrow, guess I can try using the "OnEnemyDeath" event and applying this, will see.

                    if (enemySounds != null)
                        dfAudioSource = enemySounds.GetComponent<DaggerfallAudioSource>();
                }
            }

            if (dfAudioSource != null)
            {
                dfAudioSource.AudioSource.PlayOneShot(clip, DaggerfallUnity.Settings.SoundVolume);
            }
        }

        public static AudioClip[] GetSoundClipList(CombatSoundTypes soundType, ref CVARS cVars)
        {
            switch (soundType)
            {
                default:
                case CombatSoundTypes.None: return EmptyAudioList;
                case CombatSoundTypes.Miss: return MissedAttackClips;
                case CombatSoundTypes.Dodge: return DodgedAttackClips;
                case CombatSoundTypes.Mat_Resist: return FulNegMatResClips;
                case CombatSoundTypes.Full_Act_Block: return FulNegActShieldClips;
                case CombatSoundTypes.Full_Pas_Block: return FulNegPasShieldClips;
                case CombatSoundTypes.Full_Neg_Metal_Armor: return FulNegMetalArmClips;
                case CombatSoundTypes.Full_Neg_Chain_Armor: return FulNegChainArmClips;
                case CombatSoundTypes.Full_Neg_Leather_Armor: return FulNegLeatherArmClips;
                case CombatSoundTypes.Part_Act_Block: return ParNegActShieldClips;
                case CombatSoundTypes.Part_Pas_Block: return ParNegPasShieldClips;
                case CombatSoundTypes.Part_Neg_Metal_Armor: return ParNegMetalArmClips;
                case CombatSoundTypes.Part_Neg_Chain_Armor: return ParNegChainArmClips;
                case CombatSoundTypes.Part_Neg_Leather_Armor: return ParNegLeatherArmClips;
                case CombatSoundTypes.Full_Neg_Flesh: return FulNegNatArmFleshClips;
                case CombatSoundTypes.Full_Neg_Fur: return FulNegNatArmFurClips;
                case CombatSoundTypes.Full_Neg_Scale: return FulNegNatArmScaleClips;
                case CombatSoundTypes.Full_Neg_Bone: return FulNegNatArmBoneClips;
                case CombatSoundTypes.Full_Neg_Rock: return FulNegNatArmRockClips;
                case CombatSoundTypes.Full_Neg_Metal: return FulNegNatArmMetalClips;
                case CombatSoundTypes.Attack_Hit_Flesh:
                    if (cVars.wepType == (short)DFCareer.Skills.LongBlade || cVars.wepType == (short)DFCareer.Skills.Axe)
                        return SlashHitFleshClips;
                    else if (cVars.wepType == (short)DFCareer.Skills.ShortBlade || cVars.wepType == (short)DFCareer.Skills.Archery)
                        return PierceHitFleshClips;
                    else
                        return BluntHitFleshClips;
                case CombatSoundTypes.Attack_Hit_Fur:
                    if (cVars.wepType == (short)DFCareer.Skills.LongBlade || cVars.wepType == (short)DFCareer.Skills.Axe)
                        return SlashHitFurClips;
                    else if (cVars.wepType == (short)DFCareer.Skills.ShortBlade || cVars.wepType == (short)DFCareer.Skills.Archery)
                        return PierceHitFurClips;
                    else
                        return BluntHitFurClips;
                case CombatSoundTypes.Attack_Hit_Scale:
                    if (cVars.wepType == (short)DFCareer.Skills.LongBlade || cVars.wepType == (short)DFCareer.Skills.Axe)
                        return SlashHitScaleClips;
                    else if (cVars.wepType == (short)DFCareer.Skills.ShortBlade || cVars.wepType == (short)DFCareer.Skills.Archery)
                        return PierceHitScaleClips;
                    else
                        return BluntHitScaleClips;
                case CombatSoundTypes.Attack_Hit_Bone:
                    if (cVars.wepType == (short)DFCareer.Skills.LongBlade || cVars.wepType == (short)DFCareer.Skills.Axe)
                        return SlashHitBoneClips;
                    else if (cVars.wepType == (short)DFCareer.Skills.ShortBlade || cVars.wepType == (short)DFCareer.Skills.Archery)
                        return PierceHitBoneClips;
                    else
                        return BluntHitBoneClips;
                case CombatSoundTypes.Attack_Hit_Rock:
                    if (cVars.wepType == (short)DFCareer.Skills.LongBlade || cVars.wepType == (short)DFCareer.Skills.Axe)
                        return SlashHitRockClips;
                    else if (cVars.wepType == (short)DFCareer.Skills.ShortBlade || cVars.wepType == (short)DFCareer.Skills.Archery)
                        return PierceHitRockClips;
                    else
                        return BluntHitRockClips;
                case CombatSoundTypes.Attack_Hit_Metal:
                    if (cVars.wepType == (short)DFCareer.Skills.LongBlade || cVars.wepType == (short)DFCareer.Skills.Axe)
                        return SlashHitMetalClips;
                    else if (cVars.wepType == (short)DFCareer.Skills.ShortBlade || cVars.wepType == (short)DFCareer.Skills.Archery)
                        return PierceHitMetalClips;
                    else
                        return BluntHitMetalClips;
                case CombatSoundTypes.Attack_Hit_Ethereal:
                    return AttackHitEtherealClips;
            }
        }

        // Made these two different methods because I didn't feel like figuring out a "clean" way to use the same one tracking both "LastAudioClipPlayed" values, oh well for now.
        public static AudioClip RollRandomCombatAudioClip(AudioClip[] clips)
        {
            int randChoice = UnityEngine.Random.Range(0, clips.Length);
            AudioClip clip = clips[randChoice];
            return clip;
        }

        #region Load Audio Clips


        private void LoadAudio()
        {
            ModManager modManager = ModManager.Instance;
            bool success = true;

            success &= modManager.TryGetAsset("HQ_Missed_Attack_1", false, out MissedAttackClips[0]);
            success &= modManager.TryGetAsset("HQ_Missed_Attack_2", false, out MissedAttackClips[1]);
            success &= modManager.TryGetAsset("HQ_Missed_Attack_3", false, out MissedAttackClips[2]);

            success &= modManager.TryGetAsset("HQ_Dodged_Attack_1", false, out DodgedAttackClips[0]);
            success &= modManager.TryGetAsset("HQ_Dodged_Attack_2", false, out DodgedAttackClips[1]);
            success &= modManager.TryGetAsset("HQ_Dodged_Attack_3", false, out DodgedAttackClips[2]);

            success &= modManager.TryGetAsset("HQ_Full_Negate_Mat_Resist_1", false, out FulNegMatResClips[0]);
            //success &= modManager.TryGetAsset("HQ_Full_Negate_Mat_Resist_2", false, out FulNegMatResClips[1]);
            //success &= modManager.TryGetAsset("HQ_Full_Negate_Mat_Resist_3", false, out FulNegMatResClips[2]);

            success &= modManager.TryGetAsset("HQ_Full_Negate_Active_Shield_Block_1", false, out FulNegActShieldClips[0]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Active_Shield_Block_2", false, out FulNegActShieldClips[1]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Active_Shield_Block_3", false, out FulNegActShieldClips[2]);

            success &= modManager.TryGetAsset("HQ_Full_Negate_Passive_Shield_Block_1", false, out FulNegPasShieldClips[0]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Passive_Shield_Block_2", false, out FulNegPasShieldClips[1]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Passive_Shield_Block_3", false, out FulNegPasShieldClips[2]);

            success &= modManager.TryGetAsset("HQ_Full_Negate_Metal_Armor_1", false, out FulNegMetalArmClips[0]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Metal_Armor_2", false, out FulNegMetalArmClips[1]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Metal_Armor_3", false, out FulNegMetalArmClips[2]);

            success &= modManager.TryGetAsset("HQ_Full_Negate_Chain_Armor_1", false, out FulNegChainArmClips[0]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Chain_Armor_2", false, out FulNegChainArmClips[1]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Chain_Armor_3", false, out FulNegChainArmClips[2]);

            success &= modManager.TryGetAsset("HQ_Full_Negate_Leather_Armor_1", false, out FulNegLeatherArmClips[0]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Leather_Armor_2", false, out FulNegLeatherArmClips[1]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Leather_Armor_3", false, out FulNegLeatherArmClips[2]);

            success &= modManager.TryGetAsset("HQ_Part_Negate_Active_Shield_Block_1", false, out ParNegActShieldClips[0]);
            success &= modManager.TryGetAsset("HQ_Part_Negate_Active_Shield_Block_2", false, out ParNegActShieldClips[1]);
            success &= modManager.TryGetAsset("HQ_Part_Negate_Active_Shield_Block_3", false, out ParNegActShieldClips[2]);

            success &= modManager.TryGetAsset("HQ_Part_Negate_Passive_Shield_Block_1", false, out ParNegPasShieldClips[0]);
            success &= modManager.TryGetAsset("HQ_Part_Negate_Passive_Shield_Block_2", false, out ParNegPasShieldClips[1]);
            success &= modManager.TryGetAsset("HQ_Part_Negate_Passive_Shield_Block_3", false, out ParNegPasShieldClips[2]);

            success &= modManager.TryGetAsset("HQ_Part_Negate_Metal_Armor_1", false, out ParNegMetalArmClips[0]);
            success &= modManager.TryGetAsset("HQ_Part_Negate_Metal_Armor_2", false, out ParNegMetalArmClips[1]);
            success &= modManager.TryGetAsset("HQ_Part_Negate_Metal_Armor_3", false, out ParNegMetalArmClips[2]);

            success &= modManager.TryGetAsset("HQ_Part_Negate_Chain_Armor_1", false, out ParNegChainArmClips[0]);
            success &= modManager.TryGetAsset("HQ_Part_Negate_Chain_Armor_2", false, out ParNegChainArmClips[1]);
            success &= modManager.TryGetAsset("HQ_Part_Negate_Chain_Armor_3", false, out ParNegChainArmClips[2]);

            success &= modManager.TryGetAsset("HQ_Part_Negate_Leather_Armor_1", false, out ParNegLeatherArmClips[0]);
            success &= modManager.TryGetAsset("HQ_Part_Negate_Leather_Armor_2", false, out ParNegLeatherArmClips[1]);
            success &= modManager.TryGetAsset("HQ_Part_Negate_Leather_Armor_3", false, out ParNegLeatherArmClips[2]);

            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Flesh_1", false, out FulNegNatArmFleshClips[0]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Flesh_2", false, out FulNegNatArmFleshClips[1]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Flesh_3", false, out FulNegNatArmFleshClips[2]);

            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Fur_1", false, out FulNegNatArmFurClips[0]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Fur_2", false, out FulNegNatArmFurClips[1]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Fur_3", false, out FulNegNatArmFurClips[2]);

            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Scale_1", false, out FulNegNatArmScaleClips[0]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Scale_2", false, out FulNegNatArmScaleClips[1]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Scale_3", false, out FulNegNatArmScaleClips[2]);

            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Bone_1", false, out FulNegNatArmBoneClips[0]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Bone_2", false, out FulNegNatArmBoneClips[1]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Bone_3", false, out FulNegNatArmBoneClips[2]);

            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Stone_1", false, out FulNegNatArmRockClips[0]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Stone_2", false, out FulNegNatArmRockClips[1]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Stone_3", false, out FulNegNatArmRockClips[2]);

            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Metal_1", false, out FulNegNatArmMetalClips[0]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Metal_2", false, out FulNegNatArmMetalClips[1]);
            success &= modManager.TryGetAsset("HQ_Full_Negate_Nat_Armor_Metal_3", false, out FulNegNatArmMetalClips[2]);

            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Flesh_1", false, out BluntHitFleshClips[0]);
            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Flesh_2", false, out BluntHitFleshClips[1]);
            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Flesh_3", false, out BluntHitFleshClips[2]);

            success &= modManager.TryGetAsset("HQ_Slash_Hit_Flesh_1", false, out SlashHitFleshClips[0]);
            success &= modManager.TryGetAsset("HQ_Slash_Hit_Flesh_2", false, out SlashHitFleshClips[1]);
            success &= modManager.TryGetAsset("HQ_Slash_Hit_Flesh_3", false, out SlashHitFleshClips[2]);

            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Flesh_1", false, out PierceHitFleshClips[0]);
            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Flesh_2", false, out PierceHitFleshClips[1]);
            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Flesh_3", false, out PierceHitFleshClips[2]);

            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Fur_1", false, out BluntHitFurClips[0]);
            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Fur_2", false, out BluntHitFurClips[1]);
            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Fur_3", false, out BluntHitFurClips[2]);

            success &= modManager.TryGetAsset("HQ_Slash_Hit_Fur_1", false, out SlashHitFurClips[0]);
            success &= modManager.TryGetAsset("HQ_Slash_Hit_Fur_2", false, out SlashHitFurClips[1]);
            success &= modManager.TryGetAsset("HQ_Slash_Hit_Fur_3", false, out SlashHitFurClips[2]);

            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Fur_1", false, out PierceHitFurClips[0]);
            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Fur_2", false, out PierceHitFurClips[1]);
            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Fur_3", false, out PierceHitFurClips[2]);

            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Scale_1", false, out BluntHitScaleClips[0]);
            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Scale_2", false, out BluntHitScaleClips[1]);
            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Scale_3", false, out BluntHitScaleClips[2]);

            success &= modManager.TryGetAsset("HQ_Slash_Hit_Scale_1", false, out SlashHitScaleClips[0]);
            success &= modManager.TryGetAsset("HQ_Slash_Hit_Scale_2", false, out SlashHitScaleClips[1]);
            success &= modManager.TryGetAsset("HQ_Slash_Hit_Scale_3", false, out SlashHitScaleClips[2]);

            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Scale_1", false, out PierceHitScaleClips[0]);
            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Scale_2", false, out PierceHitScaleClips[1]);
            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Scale_3", false, out PierceHitScaleClips[2]);

            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Bone_1", false, out BluntHitBoneClips[0]);
            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Bone_2", false, out BluntHitBoneClips[1]);
            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Bone_3", false, out BluntHitBoneClips[2]);

            success &= modManager.TryGetAsset("HQ_Slash_Hit_Bone_1", false, out SlashHitBoneClips[0]);
            success &= modManager.TryGetAsset("HQ_Slash_Hit_Bone_2", false, out SlashHitBoneClips[1]);
            success &= modManager.TryGetAsset("HQ_Slash_Hit_Bone_3", false, out SlashHitBoneClips[2]);

            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Bone_1", false, out PierceHitBoneClips[0]);
            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Bone_2", false, out PierceHitBoneClips[1]);
            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Bone_3", false, out PierceHitBoneClips[2]);

            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Rock_1", false, out BluntHitRockClips[0]);
            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Rock_2", false, out BluntHitRockClips[1]);
            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Rock_3", false, out BluntHitRockClips[2]);

            success &= modManager.TryGetAsset("HQ_Slash_Hit_Rock_1", false, out SlashHitRockClips[0]);
            success &= modManager.TryGetAsset("HQ_Slash_Hit_Rock_2", false, out SlashHitRockClips[1]);
            success &= modManager.TryGetAsset("HQ_Slash_Hit_Rock_3", false, out SlashHitRockClips[2]);

            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Rock_1", false, out PierceHitRockClips[0]);
            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Rock_2", false, out PierceHitRockClips[1]);
            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Rock_3", false, out PierceHitRockClips[2]);

            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Metal_1", false, out BluntHitMetalClips[0]);
            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Metal_2", false, out BluntHitMetalClips[1]);
            success &= modManager.TryGetAsset("HQ_Blunt_Hit_Metal_3", false, out BluntHitMetalClips[2]);

            success &= modManager.TryGetAsset("HQ_Slash_Hit_Metal_1", false, out SlashHitMetalClips[0]);
            success &= modManager.TryGetAsset("HQ_Slash_Hit_Metal_2", false, out SlashHitMetalClips[1]);
            success &= modManager.TryGetAsset("HQ_Slash_Hit_Metal_3", false, out SlashHitMetalClips[2]);

            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Metal_1", false, out PierceHitMetalClips[0]);
            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Metal_2", false, out PierceHitMetalClips[1]);
            success &= modManager.TryGetAsset("HQ_Pierce_Hit_Metal_3", false, out PierceHitMetalClips[2]);

            success &= modManager.TryGetAsset("HQ_Attack_Hit_Ethereal_1", false, out AttackHitEtherealClips[0]);
            success &= modManager.TryGetAsset("HQ_Attack_Hit_Ethereal_2", false, out AttackHitEtherealClips[1]);
            success &= modManager.TryGetAsset("HQ_Attack_Hit_Ethereal_3", false, out AttackHitEtherealClips[2]);

            if (!success)
                throw new Exception("[Warning] PhysicalCombatOverhaul: Missing sound asset");
        }


        #endregion

        #region Events

        // OnPlayerAttackedMonster
        public delegate void OnPlayerVSMonsterAttackEventHandler (CDATA args);
        public static event OnPlayerVSMonsterAttackEventHandler OnPlayerAttackedMonster;
        protected virtual void RaiseOnPlayerAttackedMonsterEvent(CDATA cData)
        {
            CDATA args = cData;
            if (OnPlayerAttackedMonster != null)
                OnPlayerAttackedMonster(args);
        }

        // OnMonsterAttackedPlayer
        public delegate void OnMonsterVSPlayerAttackEventHandler(CDATA args);
        public static event OnMonsterVSPlayerAttackEventHandler OnMonsterAttackedPlayer;
        protected virtual void RaiseOnMonsterAttackedPlayerEvent(CDATA cData)
        {
            CDATA args = cData;
            if (OnMonsterAttackedPlayer != null)
                OnMonsterAttackedPlayer(args);
        }

        // OnMonsterAttackedMonster
        public delegate void OnMonsterVSMonsterAttackEventHandler(CDATA args);
        public static event OnMonsterVSMonsterAttackEventHandler OnMonsterAttackedMonster;
        protected virtual void RaiseOnMonsterAttackedMonsterEvent(CDATA cData)
        {
            CDATA args = cData;
            if (OnMonsterAttackedMonster != null)
                OnMonsterAttackedMonster(args);
        }

        #endregion
    }
}
