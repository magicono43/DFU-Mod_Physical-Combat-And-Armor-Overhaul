// Project:         PhysicalCombatAndArmorOverhaul mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2024 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    2/13/2024, 9:00 PM
// Last Edit:		3/9/2024, 11:20 PM
// Version:			2.00
// Special Thanks:  Hazelnut, Ralzar, and Kab
// Modifier:		

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Formulas;
using System;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.UserInterface;

namespace PhysicalCombatOverhaul
{
    public partial class PhysicalCombatOverhaulMain : MonoBehaviour
    {
        public static PhysicalCombatOverhaulMain Instance;

        static Mod mod;

        // Global Variables
        public static ImmersiveFootsteps footstepComponent { get; set; }
        public static AudioClip LastSoundPlayed { get { return lastSoundPlayed; } set { lastSoundPlayed = value; } }
        public static IUserInterfaceWindow LastUIWindow { get; set; }
        public static DaggerfallUnityItem WornHelmet { get; set; }
        public static DaggerfallUnityItem WornChestArmor { get; set; }
        public static DaggerfallUnityItem WornLegArmor { get; set; }
        public static DaggerfallUnityItem WornBoots { get; set; }

        #region Mod Sound Variables

        // Mod Sounds
        private static AudioClip lastSoundPlayed = null;

        public static AudioClip[] GrassFootstepsMain = { null, null, null };
        public static AudioClip[] GrassFootstepsAlt = { null, null, null };
        public static AudioClip[] GravelFootstepsMain = { null, null, null };
        public static AudioClip[] GravelFootstepsAlt = { null, null, null };
        public static AudioClip[] MudFootstepsMain = { null, null, null };
        public static AudioClip[] MudFootstepsAlt = { null, null, null };
        public static AudioClip[] PathFootstepsMain = { null, null, null };
        public static AudioClip[] PathFootstepsAlt = { null, null, null };
        public static AudioClip[] SandFootstepsMain = { null, null, null };
        public static AudioClip[] SandFootstepsAlt = { null, null, null };
        public static AudioClip[] ShallowWaterFootstepsMain = { null, null, null };
        public static AudioClip[] ShallowWaterFootstepsAlt = { null, null, null };
        public static AudioClip[] SnowFootstepsMain = { null, null, null };
        public static AudioClip[] SnowFootstepsAlt = { null, null, null };
        public static AudioClip[] TileFootstepsMain = { null, null, null };
        public static AudioClip[] TileFootstepsAlt = { null, null, null };
        public static AudioClip[] WoodFootstepsMain = { null, null, null };
        public static AudioClip[] WoodFootstepsAlt = { null, null, null };

        public static AudioClip[] TestFootstepSound = { null, null, null, null, null, null };
        public static AudioClip[] FootstepSoundDungeon = { null, null, null, null, null };
        public static AudioClip[] FootstepSoundOutside = { null, null, null };
        public static AudioClip[] FootstepSoundSnow = { null, null, null };
        public static AudioClip[] FootstepSoundBuilding = { null, null, null };
        public static AudioClip[] FootstepSoundShallow = { null, null, null };
        public static AudioClip[] FootstepSoundSubmerged = { null, null, null };
        public static AudioClip[] FootstepsArmor = { null, null, null };

        #endregion

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<PhysicalCombatOverhaulMain>(); // Add script to the scene.

            go.AddComponent<ImmersiveFootsteps>();

            mod.LoadSettingsCallback = LoadSettings; // To enable use of the "live settings changes" feature in-game.

            mod.IsReady = true;
        }

        private void Start()
        {
            Debug.Log("Begin mod init: Physical Combat And Armor Overhaul");

            Instance = this;

            mod.LoadSettings();

            //FormulaHelper.RegisterOverride(mod, "CalculateAttackDamage", (Func<DaggerfallEntity, DaggerfallEntity, bool, int, DaggerfallUnityItem, int>)CalculateAttackDamage);

            DaggerfallUI.UIManager.OnWindowChange += UIManager_RefreshEquipSlotReferencesOnInventoryClose;

            // Load Resources
            LoadAudio();

            DisableVanillaFootsteps();

            GameObject playerAdvanced = GameManager.Instance.PlayerObject;
            playerAdvanced.AddComponent<ImmersiveFootsteps>();

            Debug.Log("Finished mod init: Physical Combat And Armor Overhaul");
        }

        private static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            //ReverseCycleDirection = mod.GetSettings().GetValue<bool>("GeneralSettings", "ReverseCycleDirections");
        }

        /*
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
        */

        public static void UIManager_RefreshEquipSlotReferencesOnInventoryClose(object sender, EventArgs e)
        {
            if (!GameManager.Instance.StateManager.GameInProgress)
                return;

            if (GameManager.Instance.StateManager.LastState == StateManager.StateTypes.Game || GameManager.Instance.StateManager.LastState == StateManager.StateTypes.UI)
            {
                if (DaggerfallUI.UIManager.WindowCount > 0)
                    LastUIWindow = DaggerfallUI.Instance.UserInterfaceManager.TopWindow;

                if (DaggerfallUI.UIManager.WindowCount == 0 && LastUIWindow is DaggerfallInventoryWindow)
                {
                    RefreshEquipmentSlotReferences();
                }
            }
        }

        public static void RefreshEquipmentSlotReferences()
        {
            PlayerEntity player = GameManager.Instance.PlayerEntity;

            if (player != null)
            {
                WornHelmet = player.ItemEquipTable.GetItem(EquipSlots.Head);
                WornChestArmor = player.ItemEquipTable.GetItem(EquipSlots.ChestArmor);
                WornLegArmor = player.ItemEquipTable.GetItem(EquipSlots.LegsArmor);
                WornBoots = player.ItemEquipTable.GetItem(EquipSlots.Feet);
                // Eventual method call to refresh what sounds should be used based on the equipped items, etc.
            }
        }

        #region Load Audio Clips

        
        private void LoadAudio() // Example taken from Penwick Papers Mod
        {
            ModManager modManager = ModManager.Instance;
            bool success = true;

            success &= modManager.TryGetAsset("Testing_1", false, out TestFootstepSound[0]);
            success &= modManager.TryGetAsset("Testing_2", false, out TestFootstepSound[1]);
            success &= modManager.TryGetAsset("Testing_3", false, out TestFootstepSound[2]);
            success &= modManager.TryGetAsset("Testing_4", false, out TestFootstepSound[3]);
            success &= modManager.TryGetAsset("Testing_5", false, out TestFootstepSound[4]);
            success &= modManager.TryGetAsset("Testing_6", false, out TestFootstepSound[5]);
            //success &= modManager.TryGetAsset("Squidward_Walk_2", false, out FootstepSoundDungeon[1]);

            success &= modManager.TryGetAsset("Grass_Footstep_1", false, out GrassFootstepsMain[0]);
            success &= modManager.TryGetAsset("Grass_Footstep_2", false, out GrassFootstepsMain[1]);
            success &= modManager.TryGetAsset("Grass_Footstep_3", false, out GrassFootstepsMain[2]);
            success &= modManager.TryGetAsset("Grass_Footstep_4", false, out GrassFootstepsAlt[0]);
            success &= modManager.TryGetAsset("Grass_Footstep_5", false, out GrassFootstepsAlt[1]);
            success &= modManager.TryGetAsset("Grass_Footstep_6", false, out GrassFootstepsAlt[2]);

            success &= modManager.TryGetAsset("Gravel_Footstep_1", false, out GravelFootstepsMain[0]);
            success &= modManager.TryGetAsset("Gravel_Footstep_2", false, out GravelFootstepsMain[1]);
            success &= modManager.TryGetAsset("Gravel_Footstep_3", false, out GravelFootstepsMain[2]);
            success &= modManager.TryGetAsset("Gravel_Footstep_4", false, out GravelFootstepsAlt[0]);
            success &= modManager.TryGetAsset("Gravel_Footstep_5", false, out GravelFootstepsAlt[1]);
            success &= modManager.TryGetAsset("Gravel_Footstep_6", false, out GravelFootstepsAlt[2]);

            success &= modManager.TryGetAsset("Mud_Footstep_1", false, out MudFootstepsMain[0]);
            success &= modManager.TryGetAsset("Mud_Footstep_2", false, out MudFootstepsMain[1]);
            success &= modManager.TryGetAsset("Mud_Footstep_3", false, out MudFootstepsMain[2]);
            success &= modManager.TryGetAsset("Mud_Footstep_4", false, out MudFootstepsAlt[0]);
            success &= modManager.TryGetAsset("Mud_Footstep_5", false, out MudFootstepsAlt[1]);
            success &= modManager.TryGetAsset("Mud_Footstep_6", false, out MudFootstepsAlt[2]);

            success &= modManager.TryGetAsset("Path_Footstep_1", false, out PathFootstepsMain[0]);
            success &= modManager.TryGetAsset("Path_Footstep_2", false, out PathFootstepsMain[1]);
            success &= modManager.TryGetAsset("Path_Footstep_3", false, out PathFootstepsMain[2]);
            success &= modManager.TryGetAsset("Path_Footstep_4", false, out PathFootstepsAlt[0]);
            success &= modManager.TryGetAsset("Path_Footstep_5", false, out PathFootstepsAlt[1]);
            success &= modManager.TryGetAsset("Path_Footstep_6", false, out PathFootstepsAlt[2]);

            success &= modManager.TryGetAsset("Sand_Footstep_1", false, out SandFootstepsMain[0]);
            success &= modManager.TryGetAsset("Sand_Footstep_2", false, out SandFootstepsMain[1]);
            success &= modManager.TryGetAsset("Sand_Footstep_3", false, out SandFootstepsMain[2]);
            success &= modManager.TryGetAsset("Sand_Footstep_4", false, out SandFootstepsAlt[0]);
            success &= modManager.TryGetAsset("Sand_Footstep_5", false, out SandFootstepsAlt[1]);
            success &= modManager.TryGetAsset("Sand_Footstep_6", false, out SandFootstepsAlt[2]);

            success &= modManager.TryGetAsset("Shallow_Water_Footstep_1", false, out ShallowWaterFootstepsMain[0]);
            success &= modManager.TryGetAsset("Shallow_Water_Footstep_2", false, out ShallowWaterFootstepsMain[1]);
            success &= modManager.TryGetAsset("Shallow_Water_Footstep_3", false, out ShallowWaterFootstepsMain[2]);
            success &= modManager.TryGetAsset("Shallow_Water_Footstep_4", false, out ShallowWaterFootstepsAlt[0]);
            success &= modManager.TryGetAsset("Shallow_Water_Footstep_5", false, out ShallowWaterFootstepsAlt[1]);
            success &= modManager.TryGetAsset("Shallow_Water_Footstep_6", false, out ShallowWaterFootstepsAlt[2]);

            success &= modManager.TryGetAsset("Snow_Footstep_1", false, out SnowFootstepsMain[0]);
            success &= modManager.TryGetAsset("Snow_Footstep_2", false, out SnowFootstepsMain[1]);
            success &= modManager.TryGetAsset("Snow_Footstep_3", false, out SnowFootstepsMain[2]);
            success &= modManager.TryGetAsset("Snow_Footstep_4", false, out SnowFootstepsAlt[0]);
            success &= modManager.TryGetAsset("Snow_Footstep_5", false, out SnowFootstepsAlt[1]);
            success &= modManager.TryGetAsset("Snow_Footstep_6", false, out SnowFootstepsAlt[2]);

            success &= modManager.TryGetAsset("Tile_Footstep_1", false, out TileFootstepsMain[0]);
            success &= modManager.TryGetAsset("Tile_Footstep_2", false, out TileFootstepsMain[1]);
            success &= modManager.TryGetAsset("Tile_Footstep_3", false, out TileFootstepsMain[2]);
            success &= modManager.TryGetAsset("Tile_Footstep_4", false, out TileFootstepsAlt[0]);
            success &= modManager.TryGetAsset("Tile_Footstep_5", false, out TileFootstepsAlt[1]);
            success &= modManager.TryGetAsset("Tile_Footstep_6", false, out TileFootstepsAlt[2]);

            success &= modManager.TryGetAsset("Wood_Footstep_1", false, out WoodFootstepsMain[0]);
            success &= modManager.TryGetAsset("Wood_Footstep_2", false, out WoodFootstepsMain[1]);
            success &= modManager.TryGetAsset("Wood_Footstep_3", false, out WoodFootstepsMain[2]);
            success &= modManager.TryGetAsset("Wood_Footstep_4", false, out WoodFootstepsAlt[0]);
            success &= modManager.TryGetAsset("Wood_Footstep_5", false, out WoodFootstepsAlt[1]);
            success &= modManager.TryGetAsset("Wood_Footstep_6", false, out WoodFootstepsAlt[2]);

            if (!success)
                throw new Exception("LockedLootContainers: Missing sound asset");
        }
        

        #endregion

        private static int CalculateAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, bool enemyAnimStateRecord, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            return 0;
        }

        private void DisableVanillaFootsteps()
        {
            var oldFootsteps = GameManager.Instance.PlayerObject.GetComponent<PlayerFootsteps>();

            oldFootsteps.FootstepSoundDungeon1 = SoundClips.None;
            oldFootsteps.FootstepSoundDungeon2 = SoundClips.None;
            oldFootsteps.FootstepSoundOutside1 = SoundClips.None;
            oldFootsteps.FootstepSoundOutside2 = SoundClips.None;
            oldFootsteps.FootstepSoundSnow1 = SoundClips.None;
            oldFootsteps.FootstepSoundSnow2 = SoundClips.None;
            oldFootsteps.FootstepSoundBuilding1 = SoundClips.None;
            oldFootsteps.FootstepSoundBuilding2 = SoundClips.None;
            oldFootsteps.FootstepSoundShallow = SoundClips.None;
            oldFootsteps.FootstepSoundSubmerged = SoundClips.None;

            oldFootsteps.FallHardSound = SoundClips.None;
            oldFootsteps.FallDamageSound = SoundClips.None;
            oldFootsteps.SplashLargeSound = SoundClips.None;

            oldFootsteps.enabled = false;
        }
    }
}
