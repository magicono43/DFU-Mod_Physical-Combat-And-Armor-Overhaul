using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect;

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

        DaggerfallDateTime.Seasons currentSeason = DaggerfallDateTime.Seasons.Summer;
        int currentClimateIndex = (int)MapsFile.Climates.Ocean;
        bool isInside = false;
        bool isInOutsideWater = false;
        bool isInOutsidePath = false;
        bool isOnStaticGeometry = false;

        DaggerfallDateTime.Seasons playerSeason;
        int playerClimateIndex;
        bool playerInside;
        bool playerInBuilding;
        bool playerOnExteriorWater;
        bool playerOnExteriorPath;
        bool playerOnStaticGeometry;

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

        public void DetermineGroundClimateFootstep()
        {
            int currentTileMapIndex = GameManager.Instance.StreamingWorld.PlayerTileMapIndex;
            currentSeason = DaggerfallUnity.Instance.WorldTime.Now.SeasonValue;
            currentClimateIndex = GameManager.Instance.PlayerGPS.CurrentClimateIndex;

            if (IsShallowWaterTile(currentTileMapIndex)) { } // Just establishing logic for now. Later will actually set this to play the shallow water footstep sound when on these tiles.

            if (IsPathTile(currentTileMapIndex)) { } // Just establishing logic for now. Later will actually set this to play the path footstep sound when on these tiles.

            if (currentSeason == DaggerfallDateTime.Seasons.Winter && IsSnowyClimate(currentClimateIndex))
            {
                // Just like the above if-statements, I think this small section will end with a returned sound-clip directly, but just placeholder logic for now as well.
                if (IsAlternateSwampTile(currentTileMapIndex)) { } // Return specific sound-clip here. (Likely the mud footstep ones.)
                else { } // Otherwise use the snow footstep sound-clip.
            }

            if (IsSandyClimate(currentClimateIndex))
            {
                // Tomorrow, do similar as above but for the "sandy" climates to use either sand or stone/gravel sound depending on the tile type, will see.
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

            switch (currentSeason)
            {
                case DaggerfallDateTime.Seasons.Fall:
                    switch (currentClimateIndex)
                    {
                        case (int)MapsFile.Climates.Ocean:
                        case (int)MapsFile.Climates.Desert:
                        case (int)MapsFile.Climates.Desert2:
                        case (int)MapsFile.Climates.Mountain:
                        case (int)MapsFile.Climates.Rainforest:
                        case (int)MapsFile.Climates.Swamp:
                        case (int)MapsFile.Climates.Subtropical:
                        case (int)MapsFile.Climates.MountainWoods:
                        case (int)MapsFile.Climates.Woodlands:
                        case (int)MapsFile.Climates.HauntedWoodlands:
                        default:
                            break;
                    } break;
                case DaggerfallDateTime.Seasons.Spring:
                    switch (currentClimateIndex)
                    {
                        case (int)MapsFile.Climates.Ocean:
                        case (int)MapsFile.Climates.Desert:
                        case (int)MapsFile.Climates.Desert2:
                        case (int)MapsFile.Climates.Mountain:
                        case (int)MapsFile.Climates.Rainforest:
                        case (int)MapsFile.Climates.Swamp:
                        case (int)MapsFile.Climates.Subtropical:
                        case (int)MapsFile.Climates.MountainWoods:
                        case (int)MapsFile.Climates.Woodlands:
                        case (int)MapsFile.Climates.HauntedWoodlands:
                        default:
                            break;
                    }
                    break;
                case DaggerfallDateTime.Seasons.Summer:
                    switch (currentClimateIndex)
                    {
                        case (int)MapsFile.Climates.Ocean:
                        case (int)MapsFile.Climates.Desert:
                        case (int)MapsFile.Climates.Desert2:
                        case (int)MapsFile.Climates.Mountain:
                        case (int)MapsFile.Climates.Rainforest:
                        case (int)MapsFile.Climates.Swamp:
                        case (int)MapsFile.Climates.Subtropical:
                        case (int)MapsFile.Climates.MountainWoods:
                        case (int)MapsFile.Climates.Woodlands:
                        case (int)MapsFile.Climates.HauntedWoodlands:
                        default:
                            break;
                    }
                    break;
                case DaggerfallDateTime.Seasons.Winter:
                    switch (currentClimateIndex)
                    {
                        case (int)MapsFile.Climates.Ocean:
                        case (int)MapsFile.Climates.Desert:
                        case (int)MapsFile.Climates.Desert2:
                        case (int)MapsFile.Climates.Mountain:
                        case (int)MapsFile.Climates.Rainforest:
                        case (int)MapsFile.Climates.Swamp:
                        case (int)MapsFile.Climates.Subtropical:
                        case (int)MapsFile.Climates.MountainWoods:
                        case (int)MapsFile.Climates.Woodlands:
                        case (int)MapsFile.Climates.HauntedWoodlands:
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

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