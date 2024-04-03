// Project:         PhysicalCombatAndArmorOverhaul mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2024 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    2/13/2024, 9:00 PM
// Last Edit:		4/2/2024, 11:30 PM
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
using DaggerfallConnect;
using System.Text.RegularExpressions;

namespace PhysicalCombatOverhaul
{
    public partial class PhysicalCombatOverhaulMain : MonoBehaviour
    {
        public static PhysicalCombatOverhaulMain Instance;

        static Mod mod;

        // Global Variables
        public static ImmersiveFootsteps footstepComponent { get; set; }
        public static AudioClip LastFootstepPlayed { get { return lastFootstepPlayed; } set { lastFootstepPlayed = value; } }
        public static AudioClip LastSwaySoundPlayed { get { return lastSwaySoundPlayed; } set { lastSwaySoundPlayed = value; } }
        public static IUserInterfaceWindow LastUIWindow { get; set; }
        public static byte CurrInteriorFloorType { get; set; }
        public static DaggerfallUnityItem WornHelmet { get; set; }
        public static DaggerfallUnityItem WornRightArm { get; set; }
        public static DaggerfallUnityItem WornLeftArm { get; set; }
        public static DaggerfallUnityItem WornChestArmor { get; set; }
        public static DaggerfallUnityItem WornGloves { get; set; }
        public static DaggerfallUnityItem WornLegArmor { get; set; }
        public static DaggerfallUnityItem WornBoots { get; set; }

        public static int plateWornSwayWeight = 0;
        public static int chainWornSwayWeight = 0;
        public static int leatherWornSwayWeight = 0;

        #region Mod Sound Variables

        // Mod Sounds
        private static AudioClip lastFootstepPlayed = null;
        private static AudioClip lastSwaySoundPlayed = null;

        public static AudioClip[] EmptyAudioList = Array.Empty<AudioClip>();

        public static AudioClip[] ChainmailFootstepsMain = { null, null, null };
        public static AudioClip[] ChainmailFootstepsAlt = { null, null, null };
        public static AudioClip[] LeatherFootstepsMain = { null, null, null };
        public static AudioClip[] LeatherFootstepsAlt = { null, null, null };
        public static AudioClip[] PlateFootstepsMain = { null, null, null };
        public static AudioClip[] PlateFootstepsAlt = { null, null, null };
        public static AudioClip[] UnarmoredFootstepsMain = { null, null, null };
        public static AudioClip[] UnarmoredFootstepsAlt = { null, null, null };

        public static AudioClip[] ChainmailSwaying = { null, null, null, null };
        public static AudioClip[] LeatherSwaying = { null, null, null, null };
        public static AudioClip[] PlateSwaying = { null, null, null, null };

        public static AudioClip[] DeepWaterFootstepsMain = { null, null, null };
        public static AudioClip[] DeepWaterFootstepsAlt = { null, null, null };
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

        public static AudioClip[] ChainmailHardLanding = { null, null };
        public static AudioClip[] LeatherHardLanding = { null, null };
        public static AudioClip[] PlateHardLanding = { null, null };
        public static AudioClip[] UnarmoredHardLanding = { null, null };
        public static AudioClip[] WaterLandingSound = { null };

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

            PlayerEnterExit.OnTransitionInterior += UpdateFootsteps_OnTransitionInterior;
            PlayerEnterExit.OnTransitionExterior += UpdateFootsteps_OnTransitionExterior;
            PlayerEnterExit.OnTransitionDungeonInterior += UpdateFootsteps_OnTransitionDungeonInterior;
            PlayerEnterExit.OnTransitionDungeonExterior += UpdateFootsteps_OnTransitionExterior;
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

        public void UpdateFootsteps_OnTransitionInterior(PlayerEnterExit.TransitionEventArgs args)
        {
            DFLocation.BuildingTypes buildingType = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingType;
            PlayerGPS.DiscoveredBuilding buildingData = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData;

            if (GameManager.Instance.PlayerEnterExit.IsPlayerInside)
            {
                if (buildingType != DFLocation.BuildingTypes.None)
                {
                    GameObject interiorGO = args.DaggerfallInterior.gameObject;

                    if (interiorGO != null)
                    {
                        GameObject modelsGO = interiorGO.transform.Find("Models").gameObject;

                        if (modelsGO != null)
                        {
                            GameObject combinedModelsGO = modelsGO.transform.Find("CombinedModels").gameObject;

                            if (combinedModelsGO != null)
                            {
                                MeshRenderer meshRender = combinedModelsGO.GetComponent<MeshRenderer>();

                                if (meshRender != null)
                                {
                                    Material[] mats = meshRender.materials;
                                    for (int i = 0; i < mats.Length; i++)
                                    {
                                        string matArchive = GetFormattedTextureArchiveFromMaterialName(mats[i].name);
                                        if (matArchive == string.Empty) { continue; }
                                        else
                                        {
                                            if (ImmersiveFootsteps.CheckBuildingClimateFloorTypeTables("Wood_Floor", matArchive))
                                            {
                                                CurrInteriorFloorType = 2;
                                                UpdateInteriorArmorFootstepSounds();
                                                return;
                                            }
                                            else if (ImmersiveFootsteps.CheckBuildingClimateFloorTypeTables("Stone_Floor", matArchive))
                                            {
                                                CurrInteriorFloorType = 1;
                                                UpdateInteriorArmorFootstepSounds();
                                                return;
                                            }
                                            else if (ImmersiveFootsteps.CheckBuildingClimateFloorTypeTables("Tile_Floor", matArchive))
                                            {
                                                CurrInteriorFloorType = 0;
                                                UpdateInteriorArmorFootstepSounds();
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // If the player is confirmed to be "inside", but another check fails after that, just default the footstep sound to the "Tile" one.
                CurrInteriorFloorType = 0;
                UpdateInteriorArmorFootstepSounds();
            }
        }

        public static string GetFormattedTextureArchiveFromMaterialName(string input)
        {
            // Assumed format example of input string: "TEXTURE.067 [Index=14] (Instance)"

            // Define a regular expression pattern to match the desired parts of the input string
            string pattern = @"TEXTURE\.(\d+) \[Index=(\d+)\] \(Instance\)";

            // Match the input string against the pattern
            Match match = Regex.Match(input, pattern);

            // Check if the input string matches the pattern
            if (match.Success)
            {
                // Extract the captured groups from the match
                string textureNumber = match.Groups[1].Value.TrimStart('0');
                string indexNumber = match.Groups[2].Value;

                // Format the extracted numbers as desired
                string formattedString = textureNumber + "_" + indexNumber;

                return formattedString;
            }
            else
            {
                // Return an empty string if the input string doesn't match the expected pattern
                return string.Empty;
            }
        }

        public void UpdateFootsteps_OnTransitionExterior(PlayerEnterExit.TransitionEventArgs args)
        {
            CurrInteriorFloorType = 0;
            ImmersiveFootsteps.Instance.lastTileMapIndex = 0;
            ImmersiveFootsteps.Instance.DetermineExteriorClimateFootstep();
        }

        public void UpdateFootsteps_OnTransitionDungeonInterior(PlayerEnterExit.TransitionEventArgs args)
        {
            CurrInteriorFloorType = 0;
            UpdateInteriorArmorFootstepSounds();
        }

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

        public static void UpdateInteriorArmorFootstepSounds()
        {
            DaggerfallUnityItem boots = WornBoots;

            if (CurrInteriorFloorType == 2)
            {
                if (boots != null)
                {
                    if (boots.NativeMaterialValue >= (int)ArmorMaterialTypes.Iron)
                        ImmersiveFootsteps.Instance.CurrentClimateFootsteps = ImmersiveFootsteps.Instance.altStep ? PlateFootstepsAlt : PlateFootstepsMain;
                    else if (boots.NativeMaterialValue >= (int)ArmorMaterialTypes.Chain)
                        ImmersiveFootsteps.Instance.CurrentClimateFootsteps = ImmersiveFootsteps.Instance.altStep ? ChainmailFootstepsAlt : ChainmailFootstepsMain;
                    else
                        ImmersiveFootsteps.Instance.CurrentClimateFootsteps = ImmersiveFootsteps.Instance.altStep ? WoodFootstepsAlt : WoodFootstepsMain;
                }
                else
                    ImmersiveFootsteps.Instance.CurrentClimateFootsteps = ImmersiveFootsteps.Instance.altStep ? WoodFootstepsAlt : WoodFootstepsMain;
            }
            else if (CurrInteriorFloorType == 1)
            {
                if (boots != null)
                {
                    if (boots.NativeMaterialValue >= (int)ArmorMaterialTypes.Iron)
                        ImmersiveFootsteps.Instance.CurrentClimateFootsteps = ImmersiveFootsteps.Instance.altStep ? PlateFootstepsAlt : PlateFootstepsMain;
                    else if (boots.NativeMaterialValue >= (int)ArmorMaterialTypes.Chain)
                        ImmersiveFootsteps.Instance.CurrentClimateFootsteps = ImmersiveFootsteps.Instance.altStep ? ChainmailFootstepsAlt : ChainmailFootstepsMain;
                    else
                        ImmersiveFootsteps.Instance.CurrentClimateFootsteps = ImmersiveFootsteps.Instance.altStep ? PathFootstepsAlt : PathFootstepsMain;
                }
                else
                    ImmersiveFootsteps.Instance.CurrentClimateFootsteps = ImmersiveFootsteps.Instance.altStep ? UnarmoredFootstepsAlt : UnarmoredFootstepsMain;
            }
            else
            {
                if (boots != null)
                {
                    if (boots.NativeMaterialValue >= (int)ArmorMaterialTypes.Iron)
                        ImmersiveFootsteps.Instance.CurrentClimateFootsteps = ImmersiveFootsteps.Instance.altStep ? PlateFootstepsAlt : PlateFootstepsMain;
                    else if (boots.NativeMaterialValue >= (int)ArmorMaterialTypes.Chain)
                        ImmersiveFootsteps.Instance.CurrentClimateFootsteps = ImmersiveFootsteps.Instance.altStep ? ChainmailFootstepsAlt : ChainmailFootstepsMain;
                    else
                        ImmersiveFootsteps.Instance.CurrentClimateFootsteps = ImmersiveFootsteps.Instance.altStep ? TileFootstepsAlt : TileFootstepsMain;
                }
                else
                    ImmersiveFootsteps.Instance.CurrentClimateFootsteps = ImmersiveFootsteps.Instance.altStep ? UnarmoredFootstepsAlt : UnarmoredFootstepsMain;
            }
        }

        public static void RefreshEquipmentSlotReferences()
        {
            PlayerEntity player = GameManager.Instance.PlayerEntity;

            if (player != null)
            {
                WornHelmet = player.ItemEquipTable.GetItem(EquipSlots.Head);
                WornRightArm = player.ItemEquipTable.GetItem(EquipSlots.RightArm);
                WornLeftArm = player.ItemEquipTable.GetItem(EquipSlots.LeftArm);
                WornChestArmor = player.ItemEquipTable.GetItem(EquipSlots.ChestArmor);
                WornGloves = player.ItemEquipTable.GetItem(EquipSlots.Gloves);
                WornLegArmor = player.ItemEquipTable.GetItem(EquipSlots.LegsArmor);
                WornBoots = player.ItemEquipTable.GetItem(EquipSlots.Feet);

                UpdateSwayMaterialWeights();

                if (GameManager.Instance.PlayerEnterExit.IsPlayerInside)
                {
                    UpdateInteriorArmorFootstepSounds();
                }
                else
                {
                    CurrInteriorFloorType = 0;
                    ImmersiveFootsteps.Instance.lastTileMapIndex = 0;
                    ImmersiveFootsteps.Instance.DetermineExteriorClimateFootstep();
                }
            }
        }

        public static void UpdateSwayMaterialWeights()
        {
            plateWornSwayWeight = 0;
            chainWornSwayWeight = 0;
            leatherWornSwayWeight = 0;

            // 0 = Nothing/Null, 1 = Leather, 2 = Chain, 3 = Plate.
            int[] materialValues = new int[]
            {
                GetMaterialValueOrDefault(WornHelmet),
                GetMaterialValueOrDefault(WornRightArm),
                GetMaterialValueOrDefault(WornLeftArm),
                GetMaterialValueOrDefault(WornChestArmor),
                GetMaterialValueOrDefault(WornGloves),
                GetMaterialValueOrDefault(WornLegArmor)
            };

            // 0 = Head, 1 = Right-Arm, 2 = Left-Arm, 3 = Chest, 4 = Gloves, 5 = Legs, 6 = Feet.
            for (int i = 0; i < materialValues.Length; i++)
            {
                int weight = 0;

                if (materialValues[i] <= -1) { continue; }

                switch (i)
                {
                    case 0: weight = 2; break;
                    case 1:
                    case 2:
                    case 4: weight = 1; break;
                    case 3: weight = 3; break;
                    case 5: weight = 4; break;
                }

                if (materialValues[i] >= (int)ArmorMaterialTypes.Iron) { plateWornSwayWeight += weight; }
                else if (materialValues[i] >= (int)ArmorMaterialTypes.Chain) { chainWornSwayWeight += weight; }
                else { leatherWornSwayWeight += weight; }
            }
        }

        // Helper method to get material value or return 0 if armor piece is null
        public static int GetMaterialValueOrDefault(DaggerfallUnityItem armor)
        {
            return armor != null ? armor.NativeMaterialValue : -1;
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

            success &= modManager.TryGetAsset("Chainmail_Footstep_1", false, out ChainmailFootstepsMain[0]);
            success &= modManager.TryGetAsset("Chainmail_Footstep_2", false, out ChainmailFootstepsMain[1]);
            success &= modManager.TryGetAsset("Chainmail_Footstep_3", false, out ChainmailFootstepsMain[2]);
            success &= modManager.TryGetAsset("Chainmail_Footstep_4", false, out ChainmailFootstepsAlt[0]);
            success &= modManager.TryGetAsset("Chainmail_Footstep_5", false, out ChainmailFootstepsAlt[1]);
            success &= modManager.TryGetAsset("Chainmail_Footstep_6", false, out ChainmailFootstepsAlt[2]);

            success &= modManager.TryGetAsset("Leather_Footstep_1", false, out LeatherFootstepsMain[0]);
            success &= modManager.TryGetAsset("Leather_Footstep_2", false, out LeatherFootstepsMain[1]);
            success &= modManager.TryGetAsset("Leather_Footstep_3", false, out LeatherFootstepsMain[2]);
            success &= modManager.TryGetAsset("Leather_Footstep_4", false, out LeatherFootstepsAlt[0]);
            success &= modManager.TryGetAsset("Leather_Footstep_5", false, out LeatherFootstepsAlt[1]);
            success &= modManager.TryGetAsset("Leather_Footstep_6", false, out LeatherFootstepsAlt[2]);

            success &= modManager.TryGetAsset("Plate_Footstep_1", false, out PlateFootstepsMain[0]);
            success &= modManager.TryGetAsset("Plate_Footstep_2", false, out PlateFootstepsMain[1]);
            success &= modManager.TryGetAsset("Plate_Footstep_3", false, out PlateFootstepsMain[2]);
            success &= modManager.TryGetAsset("Plate_Footstep_4", false, out PlateFootstepsAlt[0]);
            success &= modManager.TryGetAsset("Plate_Footstep_5", false, out PlateFootstepsAlt[1]);
            success &= modManager.TryGetAsset("Plate_Footstep_6", false, out PlateFootstepsAlt[2]);

            success &= modManager.TryGetAsset("Unarmored_Footstep_1", false, out UnarmoredFootstepsMain[0]);
            success &= modManager.TryGetAsset("Unarmored_Footstep_2", false, out UnarmoredFootstepsMain[1]);
            success &= modManager.TryGetAsset("Unarmored_Footstep_3", false, out UnarmoredFootstepsMain[2]);
            success &= modManager.TryGetAsset("Unarmored_Footstep_4", false, out UnarmoredFootstepsAlt[0]);
            success &= modManager.TryGetAsset("Unarmored_Footstep_5", false, out UnarmoredFootstepsAlt[1]);
            success &= modManager.TryGetAsset("Unarmored_Footstep_6", false, out UnarmoredFootstepsAlt[2]);

            success &= modManager.TryGetAsset("Chainmail_Swaying_1", false, out ChainmailSwaying[0]);
            success &= modManager.TryGetAsset("Chainmail_Swaying_2", false, out ChainmailSwaying[1]);
            success &= modManager.TryGetAsset("Chainmail_Swaying_3", false, out ChainmailSwaying[2]);
            success &= modManager.TryGetAsset("Chainmail_Swaying_4", false, out ChainmailSwaying[3]);

            success &= modManager.TryGetAsset("Leather_Swaying_1", false, out LeatherSwaying[0]);
            success &= modManager.TryGetAsset("Leather_Swaying_2", false, out LeatherSwaying[1]);
            success &= modManager.TryGetAsset("Leather_Swaying_3", false, out LeatherSwaying[2]);
            success &= modManager.TryGetAsset("Leather_Swaying_4", false, out LeatherSwaying[3]);

            success &= modManager.TryGetAsset("Plate_Swaying_1", false, out PlateSwaying[0]);
            success &= modManager.TryGetAsset("Plate_Swaying_2", false, out PlateSwaying[1]);
            success &= modManager.TryGetAsset("Plate_Swaying_3", false, out PlateSwaying[2]);
            success &= modManager.TryGetAsset("Plate_Swaying_4", false, out PlateSwaying[3]);

            success &= modManager.TryGetAsset("Deep_Water_Footstep_1", false, out DeepWaterFootstepsMain[0]);
            success &= modManager.TryGetAsset("Deep_Water_Footstep_2", false, out DeepWaterFootstepsMain[1]);
            success &= modManager.TryGetAsset("Deep_Water_Footstep_3", false, out DeepWaterFootstepsMain[2]);
            success &= modManager.TryGetAsset("Deep_Water_Footstep_4", false, out DeepWaterFootstepsAlt[0]);
            success &= modManager.TryGetAsset("Deep_Water_Footstep_5", false, out DeepWaterFootstepsAlt[1]);
            success &= modManager.TryGetAsset("Deep_Water_Footstep_6", false, out DeepWaterFootstepsAlt[2]);

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

            success &= modManager.TryGetAsset("Chainmail_Hard_Landing_1", false, out ChainmailHardLanding[0]);
            success &= modManager.TryGetAsset("Chainmail_Hard_Landing_2", false, out ChainmailHardLanding[1]);

            success &= modManager.TryGetAsset("Leather_Hard_Landing_1", false, out LeatherHardLanding[0]);
            success &= modManager.TryGetAsset("Leather_Hard_Landing_2", false, out LeatherHardLanding[1]);

            success &= modManager.TryGetAsset("Plate_Hard_Landing_1", false, out PlateHardLanding[0]);
            success &= modManager.TryGetAsset("Plate_Hard_Landing_2", false, out PlateHardLanding[1]);

            success &= modManager.TryGetAsset("Unarmored_Hard_Landing_1", false, out UnarmoredHardLanding[0]);
            success &= modManager.TryGetAsset("Unarmored_Hard_Landing_2", false, out UnarmoredHardLanding[1]);

            success &= modManager.TryGetAsset("Water_Landing_1", false, out WaterLandingSound[0]);

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
