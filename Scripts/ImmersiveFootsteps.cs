using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game;

namespace PhysicalCombatOverhaul
{
    [RequireComponent(typeof(DaggerfallAudioSource))]
    public class ImmersiveFootsteps : MonoBehaviour
    {
        #region Fields

        ulong loadID = 0;

        DaggerfallAudioSource dfAudioSource;

        #endregion

        public float WalkStepInterval = 2.5f; // Matched to classic. Was 1.6f;
        public float RunStepInterval = 2.5f; // Matched to classic. Was 1.8f;
        public float FootstepVolumeScale = 0.7f;

        #region Properties

        public ulong LoadID
        {
            get { return loadID; }
            set { loadID = value; }
        }

        #endregion

        void Awake()
        {
            dfAudioSource = GetComponent<DaggerfallAudioSource>();
        }
    }
}