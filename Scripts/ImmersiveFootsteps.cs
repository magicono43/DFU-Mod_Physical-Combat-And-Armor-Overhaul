using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect;
using PCO = PhysicalCombatOverhaul.PhysicalCombatOverhaulMain;

namespace PhysicalCombatOverhaul
{
    public class ImmersiveFootsteps : MonoBehaviour
    {
        #region Fields

        ulong loadID = 0;

        public float WalkStepInterval = 2.5f; // Matched to classic. Was 1.6f;
        public float RunStepInterval = 2.5f; // Matched to classic. Was 1.8f;
        public float FootstepVolumeScale = 0.7f;
        public float stepInterval = 0.5f;

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

        AudioClip[] currentClimateFootsteps;

        DaggerfallDateTime.Seasons lastSeason = DaggerfallDateTime.Seasons.Summer;
        int lastClimateIndex = (int)MapsFile.Climates.Ocean;
        int lastTileMapIndex = 0;

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

        #endregion

        private void Start()
        {
            playerAdvanced = GameManager.Instance.PlayerObject;
            playerGPS = GameManager.Instance.PlayerGPS;
            dfAudioSource = playerAdvanced.GetComponent<DaggerfallAudioSource>();
            playerMotor = GetComponent<PlayerMotor>();
            playerEnterExit = GetComponent<PlayerEnterExit>();
            transportManager = GetComponent<TransportManager>();
        }

        private void FixedUpdate()
        {
            if (GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress)
                return;

            refreshSlotsTimer++;

            if (refreshSlotsTimer >= 250) // 50 FixedUpdates is approximately equal to 1 second since each FixedUpdate happens every 0.02 seconds, that's what Unity docs say at least.
            {
                refreshSlotsTimer = 0;
                PhysicalCombatOverhaulMain.RefreshEquipmentSlotReferences();
            }

            if (playerMotor == null)
            {
                footstepTimer = 0f;
                return;
            }

            if (playerMotor.IsStandingStill)
            {
                // Reset footstepTimer if the character is not moving
                //footstepTimer = 0f;
                return;
            }

            // Tomorrow, perhaps see about making a "overlay" or delayed sound to happen before or after the armor-based footstep sound, which would act as the "environment specific" sound, will see.

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

            if (footstepTimer >= stepInterval)
            {
                dfAudioSource.AudioSource.PlayOneShot(PhysicalCombatOverhaulMain.TestFootstepSound[0], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                //dfAudioSource.AudioSource.PlayOneShot(PhysicalCombatOverhaulMain.TestFootstepSound[0], UnityEngine.Random.Range(0.85f, 1.1f) * DaggerfallUnity.Settings.SoundVolume);
                //dfAudioSource.AudioSource.PlayOneShot(GetTestFootstepClip(), volumeScale * DaggerfallUnity.Settings.SoundVolume);
                //dfAudioSource.AudioSource.PlayOneShot(GetTestFootstepClip(), UnityEngine.Random.Range(0.85f, 1.1f) * DaggerfallUnity.Settings.SoundVolume);

                /*
                if (PhysicalCombatOverhaulMain.WornBoots == null)
                {
                    // Play footstep sound
                    if (!altStep)
                    {
                        dfAudioSource.AudioSource.PlayOneShot(PhysicalCombatOverhaulMain.FootstepSoundDungeon[0], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                        //dfAudioSource.AudioSource.PlayOneShot(PhysicalCombatOverhaulMain.FootstepSoundSnow[0], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                        altStep = true;
                    }
                    else
                    {
                        dfAudioSource.AudioSource.PlayOneShot(PhysicalCombatOverhaulMain.FootstepSoundDungeon[1], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                        //dfAudioSource.AudioSource.PlayOneShot(PhysicalCombatOverhaulMain.FootstepSoundSnow[1], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                        altStep = false;
                    }
                }
                else
                {
                    // Play footstep sound
                    if (!altStep)
                    {
                        dfAudioSource.AudioSource.PlayOneShot(PhysicalCombatOverhaulMain.FootstepSoundBuilding[0], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                        //dfAudioSource.AudioSource.PlayOneShot(PhysicalCombatOverhaulMain.FootstepSoundSnow[0], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                        altStep = true;
                    }
                    else
                    {
                        dfAudioSource.AudioSource.PlayOneShot(PhysicalCombatOverhaulMain.FootstepSoundBuilding[1], volumeScale * DaggerfallUnity.Settings.SoundVolume);
                        //dfAudioSource.AudioSource.PlayOneShot(PhysicalCombatOverhaulMain.FootstepSoundSnow[1], volumeScale * DaggerfallUnity.Settings.SoundVolume);
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

                if (IsShallowWaterTile(currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.ShallowWaterFootstepsAlt : PCO.ShallowWaterFootstepsMain; }
                else if (IsPathTile(currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.PathFootstepsAlt : PCO.PathFootstepsMain; }
                else if (currentSeason == DaggerfallDateTime.Seasons.Winter && IsSnowyClimate(currentClimateIndex))
                {
                    if (IsAlternateSwampTile(currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.MudFootstepsAlt : PCO.MudFootstepsMain; }
                    else { currentClimateFootsteps = altStep ? PCO.SnowFootstepsAlt : PCO.SnowFootstepsMain; }
                }
                else if (IsGrassyClimate(currentClimateIndex))
                {
                    if (IsTemperateDirtTile(currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.GravelFootstepsAlt : PCO.GravelFootstepsMain; } // Gravel
                    else if (IsTemperateStoneTile(currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.PathFootstepsAlt : PCO.PathFootstepsMain; } // Stone
                    else { currentClimateFootsteps = altStep ? PCO.GrassFootstepsAlt : PCO.GrassFootstepsMain; } // Grass
                }
                else if (IsRockyClimate(currentClimateIndex))
                {
                    if (IsMountainDirtTile(currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.GravelFootstepsAlt : PCO.GravelFootstepsMain; } // Gravel
                    else if (IsMountainStoneTile(currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.PathFootstepsAlt : PCO.PathFootstepsMain; } // Stone
                    else { currentClimateFootsteps = altStep ? PCO.GrassFootstepsAlt : PCO.GrassFootstepsMain; } // Grass
                }
                else if (IsSandyClimate(currentClimateIndex))
                {
                    if (IsDesertGravelTile(currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.GravelFootstepsAlt : PCO.GravelFootstepsMain; } // Gravel
                    else if (IsDesertStoneTile(currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.PathFootstepsAlt : PCO.PathFootstepsMain; } // Stone
                    else { currentClimateFootsteps = altStep ? PCO.SandFootstepsAlt : PCO.SandFootstepsMain; } // Sand
                }
                else if (IsSwampyClimate(currentClimateIndex))
                {
                    if (IsSwampBogTile(currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.MudFootstepsAlt : PCO.MudFootstepsMain; } // Mud
                    else if (IsSwampGrassTile(currentTileMapIndex)) { currentClimateFootsteps = altStep ? PCO.GrassFootstepsAlt : PCO.GrassFootstepsMain; } // Grass
                    else { currentClimateFootsteps = altStep ? PCO.MudFootstepsAlt : PCO.MudFootstepsMain; } // Mud
                }
            }

            if (currentClimateFootsteps.Length <= 0)
            {
                currentClimateFootsteps = altStep ? PCO.PathFootstepsAlt : PCO.PathFootstepsMain;
            }

            // Perhaps tomorrow I'll try doing the subscribing to events for triggering when to recheck/update these values? Not sure, but will see.
            // Probably also try doing testing for the current changes tomorrow, now that I have the audio-files somewhat set-up in their respective arrays for the climates atleast.

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

        #region Climate Checks

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

        #region Tile Checks

        public static bool IsShallowWaterTile(int tileIndex)
        {
            // These are all the "Shallow Water" ground tiles, as determined by the "PlayerMotor.cs" script atleast.
            switch (tileIndex)
            {
                case 5:
                case 6:
                case 8:
                case 20:
                case 21:
                case 23:
                case 30:
                case 31:
                case 33:
                case 34:
                case 35:
                case 36:
                case 49:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsPathTile(int tileIndex)
        {
            // These are all the "Path" ground tiles, as determined by the "PlayerMotor.cs" script atleast.
            switch (tileIndex)
            {
                case 46:
                case 47:
                case 55:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsAlternateSwampTile(int tileIndex)
        {
            // These are all the ground tiles for the snowy swamp tileset that look more like mud than snow to me.
            switch (tileIndex)
            {
                case 1:
                case 7:
                case 10:
                case 11:
                case 13:
                case 25:
                case 26:
                case 28:
                case 37:
                case 38:
                case 39:
                case 43:
                case 48:
                case 50:
                case 52:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsTemperateDirtTile(int tileIndex)
        {
            // These are all the ground tiles for the temperate tileset that look more like dirt than grass or stone to me.
            switch (tileIndex)
            {
                case 1:
                case 4:
                case 7:
                case 10:
                case 13:
                case 25:
                case 26:
                case 28:
                case 37:
                case 38:
                case 39:
                case 51:
                case 52:
                case 54:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsTemperateStoneTile(int tileIndex)
        {
            // These are all the ground tiles for the temperate tileset that look more like stone than grass or dirt to me.
            switch (tileIndex)
            {
                case 3:
                case 14:
                case 16:
                case 17:
                case 24:
                case 27:
                case 29:
                case 32:
                case 43:
                case 44:
                case 45:
                case 50:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsMountainDirtTile(int tileIndex)
        {
            // These are all the ground tiles for the mountain tileset that look more like dirt than grass or stone to me.
            switch (tileIndex)
            {
                case 1:
                case 4:
                case 7:
                case 10:
                case 13:
                case 25:
                case 26:
                case 28:
                case 37:
                case 38:
                case 39:
                case 51:
                case 52:
                case 54:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsMountainStoneTile(int tileIndex)
        {
            // These are all the ground tiles for the mountain tileset that look more like stone than grass or dirt to me.
            switch (tileIndex)
            {
                case 3:
                case 14:
                case 16:
                case 17:
                case 24:
                case 27:
                case 29:
                case 32:
                case 43:
                case 44:
                case 45:
                case 50:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsDesertGravelTile(int tileIndex)
        {
            // These are all the ground tiles for the desert tileset that look more like gravel than sand or stone to me.
            switch (tileIndex)
            {
                case 2:
                case 9:
                case 11:
                case 12:
                case 15:
                case 18:
                case 19:
                case 22:
                case 39:
                case 40:
                case 41:
                case 42:
                case 45:
                case 53:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsDesertStoneTile(int tileIndex)
        {
            // These are all the ground tiles for the desert tileset that look more like stone than sand or gravel to me.
            switch (tileIndex)
            {
                case 3:
                case 14:
                case 16:
                case 17:
                case 24:
                case 26:
                case 27:
                case 29:
                case 32:
                case 38:
                case 43:
                case 44:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSwampBogTile(int tileIndex)
        {
            // These are all the ground tiles for the swamp tileset that look more like bog than grass or mud to me.
            switch (tileIndex)
            {
                case 1:
                case 4:
                case 7:
                case 10:
                case 13:
                case 25:
                case 26:
                case 28:
                case 37:
                case 38:
                case 39:
                case 50:
                case 51:
                case 52:
                case 54:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSwampGrassTile(int tileIndex)
        {
            // These are all the ground tiles for the swamp tileset that look more like grass than mud or bog to me.
            switch (tileIndex)
            {
                case 3:
                case 14:
                case 16:
                case 17:
                case 24:
                case 27:
                case 29:
                case 32:
                case 43:
                case 44:
                case 45:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

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

            if (clip == PhysicalCombatOverhaulMain.LastSoundPlayed)
            {
                if (randChoice == 0)
                    randChoice++;
                else if (randChoice == clips.Length - 1)
                    randChoice--;
                else
                    randChoice = CoinFlip() ? randChoice + 1 : randChoice - 1;

                clip = clips[randChoice];
            }
            PhysicalCombatOverhaulMain.LastSoundPlayed = clip;
            return clip;
        }

        public static AudioClip GetTestFootstepClip()
        {
            AudioClip clip = null;

            clip = RollRandomAudioClip(PhysicalCombatOverhaulMain.TestFootstepSound);

            if (clip == null)
                clip = PhysicalCombatOverhaulMain.TestFootstepSound[0];

            return clip;
        }
    }
}