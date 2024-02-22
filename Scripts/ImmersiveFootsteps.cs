using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility;

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
        DaggerfallAudioSource dfAudioSource;
        PlayerMotor playerMotor;
        PlayerEnterExit playerEnterExit;
        TransportManager transportManager;
        AudioSource customAudioSource;

        DaggerfallDateTime.Seasons currentSeason = DaggerfallDateTime.Seasons.Summer;
        int currentClimateIndex;
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