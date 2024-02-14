// Project:         PhysicalCombatAndArmorOverhaul mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2024 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    2/13/2024, 9:00 PM
// Last Edit:		2/13/2024, 9:00 PM
// Version:			2.00
// Special Thanks:  Hazelnut, Ralzar, and Kab
// Modifier:		

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace PhysicalCombatOverhaul
{
    public partial class PhysicalCombatOverhaulMain : MonoBehaviour
    {
        public static PhysicalCombatOverhaulMain Instance;

        static Mod mod;

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

            Debug.Log("Finished mod init: Physical Combat And Armor Overhaul");
        }

        private static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            //ReverseCycleDirection = mod.GetSettings().GetValue<bool>("GeneralSettings", "ReverseCycleDirections");
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
    }
}
