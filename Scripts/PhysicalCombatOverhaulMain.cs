// Project:         PhysicalCombatAndArmorOverhaul mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2024 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    2/13/2024, 9:00 PM
// Last Edit:		2/14/2024, 8:50 PM
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

namespace PhysicalCombatOverhaul
{
    public partial class PhysicalCombatOverhaulMain : MonoBehaviour
    {
        public static PhysicalCombatOverhaulMain Instance;

        static Mod mod;

        // Global Variables
        public static ImmersiveFootsteps footstepComponent { get; set; }
        public static AudioClip LastSoundPlayed { get { return lastSoundPlayed; } set { lastSoundPlayed = value; } }

        #region Mod Sound Variables

        // Mod Sounds
        private static AudioClip lastSoundPlayed = null;
        public static AudioClip[] UnarmedHitWoodLightClips = { null, null, null, null };
        public static AudioClip[] UnarmedHitWoodHardClips = { null, null, null };

        public static AudioClip[] FootstepSoundDungeon = { null, null, null };
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

        #region Load Audio Clips

        
        private void LoadAudio() // Example taken from Penwick Papers Mod
        {
            ModManager modManager = ModManager.Instance;
            bool success = true;

            success &= modManager.TryGetAsset("Squidward_Walk_1", false, out FootstepSoundDungeon[0]);
            success &= modManager.TryGetAsset("Squidward_Walk_2", false, out FootstepSoundDungeon[1]);

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

            oldFootsteps.FootstepSoundBuilding1 = SoundClips.None;
            oldFootsteps.FootstepSoundBuilding2 = SoundClips.None;
            oldFootsteps.FootstepSoundDungeon1 = SoundClips.None;
            oldFootsteps.FootstepSoundDungeon1 = SoundClips.None;
            oldFootsteps.FootstepSoundOutside1 = SoundClips.None;
            oldFootsteps.FootstepSoundOutside1 = SoundClips.None;
            oldFootsteps.FootstepSoundShallow = SoundClips.None;
            oldFootsteps.FootstepSoundSnow1 = SoundClips.None;
            oldFootsteps.FootstepSoundSnow2 = SoundClips.None;
            oldFootsteps.FootstepSoundSubmerged = SoundClips.None;

        }
    }
}
