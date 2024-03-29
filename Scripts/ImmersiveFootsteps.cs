using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect;
using System.Collections.Generic;
using PCO = PhysicalCombatOverhaul.PhysicalCombatOverhaulMain;

namespace PhysicalCombatOverhaul
{
    public class ImmersiveFootsteps : MonoBehaviour
    {
        public static ImmersiveFootsteps Instance;

        #region Fields

        ulong loadID = 0;

        public float WalkStepInterval = 2.5f; // Matched to classic. Was 1.6f;
        public float RunStepInterval = 2.5f; // Matched to classic. Was 1.8f;
        public float FootstepVolumeScale = 0.7f;
        public float stepInterval = 0.5f;

        public float swimInterval = 1.75f;
        public Vector3 lastPosition;
        public float distance = 0f;

        public float footstepTimer = 0f;
        public int refreshSlotsTimer = 0;
        public bool isWalking = false;
        public bool altStep = false;

        public float volumeScale = 1f;

        GameObject playerAdvanced;
        PlayerGPS playerGPS;
        DaggerfallAudioSource dfAudioSource;
        PlayerMotor playerMotor;
        PlayerEnterExit playerEnterExit;
        TransportManager transportManager;
        AudioSource customAudioSource;

        AudioClip[] currentClimateFootsteps = PCO.PathFootstepsMain; // This for now until I do proper detection for non-exterior areas as well.

        DaggerfallDateTime.Seasons lastSeason = DaggerfallDateTime.Seasons.Summer;
        int lastClimateIndex = (int)MapsFile.Climates.Ocean;
        public int lastTileMapIndex = 0;

        DaggerfallDateTime.Seasons currentSeason = DaggerfallDateTime.Seasons.Summer;
        int currentClimateIndex = (int)MapsFile.Climates.Ocean;
        int currentTileMapIndex = 0;

        bool isInside = false;
        bool isInBuilding = false;
        bool onExteriorWater = false;
        bool onExteriorPath = false;
        bool oStaticGeometry = false;

        #endregion

        #region Properties

        public ulong LoadID
        {
            get { return loadID; }
            set { loadID = value; }
        }

        public AudioClip[] CurrentClimateFootsteps
        {
            get { return currentClimateFootsteps; }
            set { currentClimateFootsteps = value; }
        }

        #endregion

        private void Start()
        {
            Instance = this;

            playerAdvanced = GameManager.Instance.PlayerObject;
            playerGPS = GameManager.Instance.PlayerGPS;
            dfAudioSource = playerAdvanced.GetComponent<DaggerfallAudioSource>();
            playerMotor = GetComponent<PlayerMotor>();
            playerEnterExit = GetComponent<PlayerEnterExit>();
            transportManager = GetComponent<TransportManager>();

            // Set start position
            lastPosition = GetHorizontalPosition();
        }

        private void FixedUpdate()
        {
            if (GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress)
                return;

            bool playerSwimming = false;

            refreshSlotsTimer++;

            if (refreshSlotsTimer >= 250) // 50 FixedUpdates is approximately equal to 1 second since each FixedUpdate happens every 0.02 seconds, that's what Unity docs say at least.
            {
                refreshSlotsTimer = 0;
                PCO.RefreshEquipmentSlotReferences();
            }

            if (playerMotor == null)
            {
                footstepTimer = 0f;
                return;
            }

            if (playerEnterExit != null)
            {
                playerSwimming = playerEnterExit.IsPlayerSwimming;
            }

            if (playerMotor.IsGrounded == false && !playerSwimming)
            {
                return;
            }

            if (playerMotor.IsStandingStill)
            {
                // Reset footstepTimer if the character is not moving
                //footstepTimer = 0f;
                return;
            }

            if (playerMotor.IsRunning)
            {
                footstepTimer += 1.5f * Time.fixedDeltaTime;
                volumeScale = 1.25f;
            }
            else if (playerMotor.IsMovingLessThanHalfSpeed)
            {
                footstepTimer += 0.7f * Time.fixedDeltaTime;
                volumeScale = 0.6f;
            }
            else
            {
                footstepTimer += Time.fixedDeltaTime;
                volumeScale = 1f;
            }

            if (playerSwimming)
            {
                // Get distance player travelled horizontally
                Vector3 position = GetHorizontalPosition();
                distance += Vector3.Distance(position, lastPosition);
                lastPosition = position;

                if (distance > swimInterval)
                {
                    isInside = (playerEnterExit == null) ? true : playerEnterExit.IsPlayerInside;

                    if (isInside)
                    {
                        DetermineInteriorClimateFootstep();
                    }
                    else
                    {
                        DetermineExteriorClimateFootstep();
                    }

                    dfAudioSource.AudioSource.PlayOneShot(RollRandomAudioClip(currentClimateFootsteps), volumeScale * DaggerfallUnity.Settings.SoundVolume);

                    distance = 0f;
                }

                footstepTimer = 0f;
            }

            if (footstepTimer >= stepInterval)
            {
                isInside = (playerEnterExit == null) ? true : playerEnterExit.IsPlayerInside;

                if (isInside)
                {
                    DetermineInteriorClimateFootstep();
                }
                else
                {
                    DetermineExteriorClimateFootstep();
                }

                dfAudioSource.AudioSource.PlayOneShot(RollRandomAudioClip(currentClimateFootsteps), volumeScale * DaggerfallUnity.Settings.SoundVolume);

                //dfAudioSource.AudioSource.PlayOneShot(PCO.TestFootstepSound[0], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                //dfAudioSource.AudioSource.PlayOneShot(PCO.TestFootstepSound[0], UnityEngine.Random.Range(0.85f, 1.1f) * DaggerfallUnity.Settings.SoundVolume);
                //dfAudioSource.AudioSource.PlayOneShot(GetTestFootstepClip(), volumeScale * DaggerfallUnity.Settings.SoundVolume);
                //dfAudioSource.AudioSource.PlayOneShot(GetTestFootstepClip(), UnityEngine.Random.Range(0.85f, 1.1f) * DaggerfallUnity.Settings.SoundVolume);

                /*
                if (PCO.WornBoots == null)
                {
                    // Play footstep sound
                    if (!altStep)
                    {
                        dfAudioSource.AudioSource.PlayOneShot(PCO.FootstepSoundDungeon[0], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                        //dfAudioSource.AudioSource.PlayOneShot(PCO.FootstepSoundSnow[0], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                        altStep = true;
                    }
                    else
                    {
                        dfAudioSource.AudioSource.PlayOneShot(PCO.FootstepSoundDungeon[1], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                        //dfAudioSource.AudioSource.PlayOneShot(PCO.FootstepSoundSnow[1], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                        altStep = false;
                    }
                }
                else
                {
                    // Play footstep sound
                    if (!altStep)
                    {
                        dfAudioSource.AudioSource.PlayOneShot(PCO.FootstepSoundBuilding[0], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                        //dfAudioSource.AudioSource.PlayOneShot(PCO.FootstepSoundSnow[0], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                        altStep = true;
                    }
                    else
                    {
                        dfAudioSource.AudioSource.PlayOneShot(PCO.FootstepSoundBuilding[1], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                        //dfAudioSource.AudioSource.PlayOneShot(PCO.FootstepSoundSnow[1], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                        altStep = false;
                    }
                }
                */

                //dfAudioSource.PlayOneShot(SoundClips.ActivateLockUnlock, 1, 1 * DaggerfallUnity.Settings.SoundVolume);
                //customAudioSource.PlayOneShot(clip1, volumeScale * DaggerfallUnity.Settings.SoundVolume);

                // Reset the footstepTimer
                footstepTimer = 0f;
            }

            /*
            playerSeason = DaggerfallUnity.Instance.WorldTime.Now.SeasonValue;
            playerClimateIndex = GameManager.Instance.PlayerGPS.CurrentClimateIndex;
            playerInside = (playerEnterExit == null) ? true : playerEnterExit.IsPlayerInside;
            playerInBuilding = (playerEnterExit == null) ? false : playerEnterExit.IsPlayerInsideBuilding;

            // Play splash footsteps whether player is walking on or swimming in exterior water
            playerOnExteriorWater = (GameManager.Instance.PlayerMotor.OnExteriorWater == PlayerMotor.OnExteriorWaterMethod.Swimming || GameManager.Instance.PlayerMotor.OnExteriorWater == PlayerMotor.OnExteriorWaterMethod.WaterWalking);

            playerOnExteriorPath = GameManager.Instance.PlayerMotor.OnExteriorPath;
            playerOnStaticGeometry = GameManager.Instance.PlayerMotor.OnExteriorStaticGeometry;

            // Change footstep sounds between winter/summer variants, when player enters/exits an interior space, or changes between path, water, or other outdoor ground
            if (playerSeason != currentSeason || playerClimateIndex != currentClimateIndex || isInside != playerInside || playerOnExteriorWater != isInOutsideWater || playerOnExteriorPath != isInOutsidePath || playerOnStaticGeometry != isOnStaticGeometry)
            {
                currentSeason = playerSeason;
                currentClimateIndex = playerClimateIndex;
                isInside = playerInside;
                isInOutsideWater = playerOnExteriorWater;
                isInOutsidePath = playerOnExteriorPath;
                isOnStaticGeometry = playerOnStaticGeometry;
            }

            // Play sound if over distance threshold
            if (distance > threshold && customAudioSource && clip1 && clip2)
            {
                float volumeScale = FootstepVolumeScale;
                if (playerMotor.IsMovingLessThanHalfSpeed)
                    volumeScale *= 0.5f;

                if (!alternateStep)
                    customAudioSource.PlayOneShot(clip1, volumeScale * DaggerfallUnity.Settings.SoundVolume);
                else
                    customAudioSource.PlayOneShot(clip2, volumeScale * DaggerfallUnity.Settings.SoundVolume);

                alternateStep = (!alternateStep);
                distance = 0f;
            }
            */
        }

        public void DetermineInteriorClimateFootstep()
        {
            // Use water sounds if in dungeon water
            if (GameManager.Instance.PlayerEnterExit.IsPlayerInsideDungeon && playerEnterExit.blockWaterLevel != 10000)
            {
                // In water, deep depth
                if (playerEnterExit.IsPlayerSwimming)
                {
                    currentClimateFootsteps = altStep ? PCO.DeepWaterFootstepsAlt : PCO.DeepWaterFootstepsMain;
                }
                // In water, shallow depth
                else if (!playerEnterExit.IsPlayerSwimming && (playerMotor.transform.position.y - 0.57f) < (playerEnterExit.blockWaterLevel * -1 * MeshReader.GlobalScale))
                {
                    currentClimateFootsteps = altStep ? PCO.ShallowWaterFootstepsAlt : PCO.ShallowWaterFootstepsMain;
                }
                else
                {
                    CurrentClimateFootsteps = altStep ? PCO.TileFootstepsAlt : PCO.TileFootstepsMain;
                }
            }

            /*
             * Interior Tile Floors:
             * 16_3
             * 37_3
             * 40_3
             * 41_0, 41_1, 41_2, 41_3
             * 44_3
             * 63_3
             * 111_2, 111_3
             * 137_3
             * 141_0, 141_1, 141_2, 141_3
             * 144_3
             * 311_3
             * 337_3
             * 341_0, 341_1, 341_2, 341_3
             * 363_3
             * 411_3
             * 437_3
             * 440_3
             * 444_3
             * 463_3
             * 
             * Interior Stone Floors:
             * 60_3
             * 140_3
             * 160_3
             * 163_3
             * 340_3
             * 360_3
             * 366_3
             * 416_3
             * 460_3
             * 466_3
             * 
             * Interior Wood Floors:
             * 28_3
             * 66_3
             * 116_3
             * 128_3
             * 166_3
             * 171_3
             * 316_3
             * 328_3
             * 344_3
             * 428_3
             * 
            */
        }

        public void DetermineExteriorClimateFootstep()
        {
            currentSeason = DaggerfallUnity.Instance.WorldTime.Now.SeasonValue;
            currentClimateIndex = GameManager.Instance.PlayerGPS.CurrentClimateIndex;
            currentTileMapIndex = GameManager.Instance.StreamingWorld.PlayerTileMapIndex;

            if (lastTileMapIndex != currentTileMapIndex || lastClimateIndex != currentClimateIndex || lastSeason != currentSeason)
            {
                lastSeason = currentSeason;
                lastClimateIndex = currentClimateIndex;
                lastTileMapIndex = currentTileMapIndex;

                if (currentTileMapIndex == 0)
                {
                    // Minor bug here, if you are water-walking over water tiles, like the ocean, but water walking wears off, the "shallow water" sound will still play, due to not updating, oh well for now.
                    if (GameManager.Instance.PlayerMotor.OnExteriorWater == PlayerMotor.OnExteriorWaterMethod.WaterWalking) { currentClimateFootsteps = altStep ? PCO.ShallowWaterFootstepsAlt : PCO.ShallowWaterFootstepsMain; }
                    else { currentClimateFootsteps = altStep ? PCO.DeepWaterFootstepsAlt : PCO.DeepWaterFootstepsMain; }
                }
                else if (CheckClimateTileTables("Shallow_Water", (byte)currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.ShallowWaterFootstepsAlt : PCO.ShallowWaterFootstepsMain; }
                else if (CheckClimateTileTables("Path", (byte)currentTileMapIndex))
                {
                    CheckToUseArmorFootsteps();
                }
                else if (currentSeason == DaggerfallDateTime.Seasons.Winter && IsSnowyClimate(currentClimateIndex))
                {
                    if (currentClimateIndex == (int)MapsFile.Climates.Swamp && CheckClimateTileTables("Swamp_Snow_Alt", (byte)currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.MudFootstepsAlt : PCO.MudFootstepsMain; }
                    else { currentClimateFootsteps = altStep ? PCO.SnowFootstepsAlt : PCO.SnowFootstepsMain; }
                }
                else if (IsGrassyClimate(currentClimateIndex))
                {
                    if (CheckClimateTileTables("Temperate_Dirt", (byte)currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.GravelFootstepsAlt : PCO.GravelFootstepsMain; } // Gravel
                    else if (CheckClimateTileTables("Temperate_Stone", (byte)currentTileMapIndex)) { CheckToUseArmorFootsteps(); } // Stone
                    else { currentClimateFootsteps = altStep ? PCO.GrassFootstepsAlt : PCO.GrassFootstepsMain; } // Grass
                }
                else if (IsRockyClimate(currentClimateIndex))
                {
                    if (CheckClimateTileTables("Mountain_Dirt", (byte)currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.GravelFootstepsAlt : PCO.GravelFootstepsMain; } // Gravel
                    else if (CheckClimateTileTables("Mountain_Stone", (byte)currentTileMapIndex)) { CheckToUseArmorFootsteps(); } // Stone
                    else { currentClimateFootsteps = altStep ? PCO.GrassFootstepsAlt : PCO.GrassFootstepsMain; } // Grass
                }
                else if (IsSandyClimate(currentClimateIndex))
                {
                    if (CheckClimateTileTables("Desert_Gravel", (byte)currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.GravelFootstepsAlt : PCO.GravelFootstepsMain; } // Gravel
                    else if (CheckClimateTileTables("Desert_Stone", (byte)currentTileMapIndex)) { CheckToUseArmorFootsteps(); } // Stone
                    else { currentClimateFootsteps = altStep ? PCO.SandFootstepsAlt : PCO.SandFootstepsMain; } // Sand
                }
                else if (IsSwampyClimate(currentClimateIndex))
                {
                    if (CheckClimateTileTables("Swamp_Bog", (byte)currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.MudFootstepsAlt : PCO.MudFootstepsMain; } // Mud
                    else if (CheckClimateTileTables("Swamp_Grass", (byte)currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.GrassFootstepsAlt : PCO.GrassFootstepsMain; } // Grass
                    else { currentClimateFootsteps = altStep ? PCO.MudFootstepsAlt : PCO.MudFootstepsMain; } // Mud
                }
            }

            if (currentClimateFootsteps.Length <= 0)
            {
                currentClimateFootsteps = altStep ? PCO.PathFootstepsAlt : PCO.PathFootstepsMain;
            }

            // Work on this order of actions later today.
            // Subscribe To Events For Triggering Changes
            //      Method Calls
            //          Season
            //              Climate
            // (Could Eventually Have Weather In Here As Well, Will See.)
            //                  Tile Index
            //                      Sound To Play
            // Might be another step? But I'll just have to check/remember later.

            // Desert Terrain = Desert, Desert2, Subtropical
            // Mountain Terrain = Mountain, MountainWoods
            // Woodland Terrain = Woodlands, HauntedWoodlands
            // Swamp Terrain = Swamp, Rainforest
            // Other Terrain = Ocean
        }

        #region Climate Type Checks

        public static bool IsSnowyClimate(int climateIndex)
        {
            // These are all the existing climates that DO NOT get snow on the ground during the winter season, in the vanilla game atleast.
            switch (climateIndex)
            {
                case (int)MapsFile.Climates.Desert:
                case (int)MapsFile.Climates.Desert2:
                case (int)MapsFile.Climates.Rainforest:
                case (int)MapsFile.Climates.Subtropical:
                    return false;
                default:
                    return true;
            }
        }

        public static bool IsGrassyClimate(int climateIndex)
        {
            switch (climateIndex)
            {
                case (int)MapsFile.Climates.Woodlands:
                case (int)MapsFile.Climates.HauntedWoodlands:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsRockyClimate(int climateIndex)
        {
            switch (climateIndex)
            {
                case (int)MapsFile.Climates.Mountain:
                case (int)MapsFile.Climates.MountainWoods:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSandyClimate(int climateIndex)
        {
            switch (climateIndex)
            {
                case (int)MapsFile.Climates.Desert:
                case (int)MapsFile.Climates.Desert2:
                case (int)MapsFile.Climates.Subtropical:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSwampyClimate(int climateIndex)
        {
            switch (climateIndex)
            {
                case (int)MapsFile.Climates.Swamp:
                case (int)MapsFile.Climates.Rainforest:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region Climate Tile Checks

        static Dictionary<byte, bool> shallowWaterTileLookup = new Dictionary<byte, bool>
        {
            {5,true}, {6,true}, {8,true}, {20,true}, {21,true}, {23,true}, {30,true}, {31,true}, {33,true}, {34,true}, {35,true}, {36,true}, {49,true}
        };

        static Dictionary<byte, bool> pathTileLookup = new Dictionary<byte, bool>
        {
            {46,true}, {47,true}, {55,true}
        };

        static Dictionary<byte, bool> temperateDirtTileLookup = new Dictionary<byte, bool>
        {
            {1,true}, {4,true}, {7,true}, {10,true}, {13,true}, {25,true}, {26,true}, {28,true}, {37,true}, {38,true}, {39,true}, {51,true}, {52,true}, {54,true}
        };

        static Dictionary<byte, bool> temperateStoneTileLookup = new Dictionary<byte, bool>
        {
            {3,true}, {14,true}, {16,true}, {17,true}, {24,true}, {27,true}, {29,true}, {32,true}, {43,true}, {44,true}, {45,true}, {50,true}
        };

        static Dictionary<byte, bool> mountainDirtTileLookup = new Dictionary<byte, bool>
        {
            {1,true}, {4,true}, {7,true}, {10,true}, {13,true}, {25,true}, {26,true}, {28,true}, {37,true}, {38,true}, {39,true}, {51,true}, {52,true}, {54,true}
        };

        static Dictionary<byte, bool> mountainStoneTileLookup = new Dictionary<byte, bool>
        {
            {3,true}, {14,true}, {16,true}, {17,true}, {24,true}, {27,true}, {29,true}, {32,true}, {43,true}, {44,true}, {45,true}, {50,true}
        };

        static Dictionary<byte, bool> desertGravelTileLookup = new Dictionary<byte, bool>
        {
            {2,true}, {9,true}, {11,true}, {12,true}, {15,true}, {18,true}, {19,true}, {22,true}, {39,true}, {40,true}, {41,true}, {42,true}, {45,true}, {53,true}
        };

        static Dictionary<byte, bool> desertStoneTileLookup = new Dictionary<byte, bool>
        {
            {3,true}, {14,true}, {16,true}, {17,true}, {24,true}, {26,true}, {27,true}, {29,true}, {32,true}, {38,true}, {43,true}, {44,true}
        };

        static Dictionary<byte, bool> swampBogTileLookup = new Dictionary<byte, bool>
        {
            {1,true}, {4,true}, {7,true}, {10,true}, {13,true}, {25,true}, {26,true}, {28,true}, {37,true}, {38,true}, {39,true}, {50,true}, {51,true}, {52,true}, {54,true}
        };

        static Dictionary<byte, bool> swampGrassTileLookup = new Dictionary<byte, bool>
        {
            {3,true}, {14,true}, {16,true}, {17,true}, {24,true}, {27,true}, {29,true}, {32,true}, {43,true}, {44,true}, {45,true}
        };

        static Dictionary<byte, bool> swampAlternateTileLookup = new Dictionary<byte, bool>
        {
            {1,true}, {7,true}, {10,true}, {11,true}, {13,true}, {25,true}, {26,true}, {28,true}, {37,true}, {38,true}, {39,true}, {43,true}, {48,true}, {50,true}, {52,true}
        };

        static Dictionary<string, Dictionary<byte, bool>> climateFloorTilesLookup = new Dictionary<string, Dictionary<byte, bool>>
        {
            {"Shallow_Water",shallowWaterTileLookup}, {"Path",pathTileLookup}, {"Temperate_Dirt",temperateDirtTileLookup}, {"Temperate_Stone",temperateStoneTileLookup},
            {"Mountain_Dirt",mountainDirtTileLookup}, {"Mountain_Stone",mountainStoneTileLookup}, {"Desert_Gravel",desertGravelTileLookup}, {"Desert_Stone",desertStoneTileLookup},
            {"Swamp_Bog",swampBogTileLookup}, {"Swamp_Grass",swampGrassTileLookup}, {"Swamp_Snow_Alt",swampAlternateTileLookup}
        };

        public static bool CheckClimateTileTables(string climateKey, byte tileKey)
        {
            if (climateFloorTilesLookup.ContainsKey(climateKey))
            {
                if (climateFloorTilesLookup[climateKey].ContainsKey(tileKey))
                {
                    return climateFloorTilesLookup[climateKey][tileKey];
                }
                else { return false; }
            }
            else { return false; }
        }

        #endregion

        #region Building Interior Climate Floor Checks

        static Dictionary<string, bool> buildingTileFloorArchiveLookup = new Dictionary<string, bool>
        {
            {"16_3",true}, {"37_3",true}, {"40_3",true}, {"41_0",true}, {"41_1",true}, {"41_2",true}, {"41_3",true}, {"44_3",true}, {"63_3",true}, {"111_2",true}, {"111_3",true},
            {"137_3",true}, {"141_0",true}, {"141_1",true}, {"141_2",true}, {"141_3",true}, {"144_3",true}, {"311_3",true}, {"337_3",true}, {"341_0",true}, {"341_1",true},
            {"341_2",true}, {"341_3",true}, {"363_3",true}, {"411_3",true}, {"437_3",true}, {"440_3",true}, {"444_3",true}, {"463_3",true}
        };

        static Dictionary<string, bool> buildingStoneFloorArchiveLookup = new Dictionary<string, bool>
        {
            {"60_3",true}, {"140_3",true}, {"160_3",true}, {"163_3",true}, {"340_3",true}, {"360_3",true}, {"366_3",true}, {"416_3",true}, {"460_3",true}, {"466_3",true}
        };

        static Dictionary<string, bool> buildingWoodFloorArchiveLookup = new Dictionary<string, bool>
        {
            {"28_3",true}, {"66_3",true}, {"116_3",true}, {"128_3",true}, {"166_3",true}, {"171_3",true}, {"316_3",true}, {"328_3",true}, {"344_3",true}, {"428_3",true}
        };

        static Dictionary<string, Dictionary<string, bool>> buildingClimateFloorTypesLookup = new Dictionary<string, Dictionary<string, bool>>
        {
            {"Tile_Floor",buildingTileFloorArchiveLookup}, {"Stone_Floor",buildingStoneFloorArchiveLookup}, {"Wood_Floor",buildingWoodFloorArchiveLookup}
        };

        public static bool CheckBuildingClimateFloorTypeTables(string floorTypeKey, string archiveKey)
        {
            if (buildingClimateFloorTypesLookup.ContainsKey(floorTypeKey))
            {
                if (buildingClimateFloorTypesLookup[floorTypeKey].ContainsKey(archiveKey))
                {
                    return buildingClimateFloorTypesLookup[floorTypeKey][archiveKey];
                }
                else { return false; }
            }
            else { return false; }
        }

        #endregion

        public void CheckToUseArmorFootsteps()
        {
            DaggerfallUnityItem boots = PCO.WornBoots;
            if (boots != null)
            {
                if (boots.NativeMaterialValue >= (int)ArmorMaterialTypes.Iron)
                {
                    currentClimateFootsteps = altStep ? PCO.PlateFootstepsAlt : PCO.PlateFootstepsMain;
                }
                else if (boots.NativeMaterialValue >= (int)ArmorMaterialTypes.Chain)
                {
                    currentClimateFootsteps = altStep ? PCO.ChainmailFootstepsAlt : PCO.ChainmailFootstepsMain;
                }
                else
                {
                    currentClimateFootsteps = altStep ? PCO.LeatherFootstepsAlt : PCO.LeatherFootstepsMain;
                }
            }
            else
            {
                currentClimateFootsteps = altStep ? PCO.UnarmoredFootstepsAlt : PCO.UnarmoredFootstepsMain;
            }
        }

        private Vector3 GetHorizontalPosition()
        {
            return new Vector3(transform.position.x, transform.position.y, transform.position.z);
        }

        public static bool CoinFlip()
        {
            if (UnityEngine.Random.Range(0, 1 + 1) == 0)
                return false;
            else
                return true;
        }

        public static AudioClip RollRandomAudioClip(AudioClip[] clips)
        {
            int randChoice = UnityEngine.Random.Range(0, clips.Length);
            AudioClip clip = clips[randChoice];

            if (clip == PCO.LastSoundPlayed)
            {
                if (randChoice == 0)
                    randChoice++;
                else if (randChoice == clips.Length - 1)
                    randChoice--;
                else
                    randChoice = CoinFlip() ? randChoice + 1 : randChoice - 1;

                clip = clips[randChoice];
            }
            PCO.LastSoundPlayed = clip;
            return clip;
        }

        public static AudioClip GetTestFootstepClip()
        {
            AudioClip clip = null;

            clip = RollRandomAudioClip(PCO.TestFootstepSound);

            if (clip == null)
                clip = PCO.TestFootstepSound[0];

            return clip;
        }
    }
}