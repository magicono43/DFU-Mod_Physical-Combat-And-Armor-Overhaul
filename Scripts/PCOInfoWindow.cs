using UnityEngine;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using PhysicalCombatOverhaul;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Serialization;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    /// <summary>
    /// Implements Physical Combat Overhaul's Custom Info Window.
    /// </summary>
    public class PCOInfoWindow : DaggerfallPopupWindow
    {
        public static PCOInfoWindow Instance;

        PlayerEntity player;

        PlayerEntity Player
        {
            get { return (player != null) ? player : player = GameManager.Instance.PlayerEntity; }
        }

        #region Testing Properties

        public static Rect butt1 = new Rect(0, 0, 0, 0);
        public static Rect butt2 = new Rect(0, 0, 0, 0);
        public static Rect butt3 = new Rect(0, 0, 0, 0);
        public float testNum1 = 100f;

        #endregion

        #region Constructors

        public PCOInfoWindow(IUserInterfaceManager uiManager)
            : base(uiManager)
        {
            Instance = this;
        }

        #endregion

        #region UI Textures

        Texture2D baseTexture;

        #endregion

        Panel headItemIconPanel;
        Panel rightArmItemIconPanel;
        Panel chestItemIconPanel;
        Panel glovesItemIconPanel;
        Panel rightHandItemIconPanel;

        Panel extraInfoItemIconPanel;
        Panel leftArmItemIconPanel;
        Panel legsItemIconPanel;
        Panel bootsItemIconPanel;
        Panel leftHandItemIconPanel;

        Panel headItemDurabilityBarPanel;
        Panel rightArmItemDurabilityBarPanel;
        Panel chestItemDurabilityBarPanel;
        Panel glovesItemDurabilityBarPanel;
        Panel rightHandItemDurabilityBarPanel;

        Panel extraInfoItemDurabilityBarPanel;
        Panel leftArmItemDurabilityBarPanel;
        Panel legsItemDurabilityBarPanel;
        Panel bootsItemDurabilityBarPanel;
        Panel leftHandItemDurabilityBarPanel;

        Panel[] itemDurBars = {null, null, null, null, null, null, null, null, null, null};

        protected override void Setup()
        {
            base.Setup();

            // Load textures
            LoadTextures();

            // This makes the background "transparent" instead of a blank black screen when opening this window.
            ParentPanel.BackgroundColor = ScreenDimColor;

            // Setup native panel background
            NativePanel.BackgroundColor = ScreenDimColor;
            NativePanel.BackgroundTexture = baseTexture;

            SetupChestChoiceButtons();

            SetupTestItemImagePanels();
        }

        protected virtual void LoadTextures()
        {
            baseTexture = PhysicalCombatOverhaulMain.Instance.EquipInfoGUITexture;
        }

        protected void SetupChestChoiceButtons()
        {
            // Inspect Chest button
            Button inspectChestButton = DaggerfallUI.AddButton(new Rect(144, 70, 33, 16), NativePanel);
            inspectChestButton.ToolTip = defaultToolTip;
            inspectChestButton.ToolTipText = "Inspect Chest";
            //inspectChestButton.OnMouseClick += InspectChestButton_OnMouseClick;
            inspectChestButton.ClickSound = DaggerfallUI.Instance.GetAudioClip(SoundClips.ButtonClick);

            // Attempt Lockpick button
            Button attemptLockpickButton = DaggerfallUI.AddButton(new Rect(144, 92, 33, 16), NativePanel);
            attemptLockpickButton.ToolTip = defaultToolTip;
            attemptLockpickButton.ToolTipText = "Attempt Lockpick";
            //attemptLockpickButton.OnMouseClick += AttemptLockpickButton_OnMouseClick;
            attemptLockpickButton.ClickSound = DaggerfallUI.Instance.GetAudioClip(SoundClips.ButtonClick);

            // Exit button
            Button exitButton = DaggerfallUI.AddButton(new Rect(142, 114, 36, 17), NativePanel);
            exitButton.OnMouseClick += ExitButton_OnMouseClick;
            exitButton.ClickSound = DaggerfallUI.Instance.GetAudioClip(SoundClips.ButtonClick);
        }

        protected void SetupTestItemImagePanels()
        {
            headItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 14, 30, 28), NativePanel);
            headItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            headItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(headItemIconPanel, EquipSlots.Head);
            headItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 43, 54, 2), NativePanel);
            AddItemDurabilityBar(headItemDurabilityBarPanel, EquipSlots.Head, 0);

            rightArmItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 50, 30, 28), NativePanel);
            rightArmItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            rightArmItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(rightArmItemIconPanel, EquipSlots.RightArm);
            rightArmItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 79, 54, 2), NativePanel);

            chestItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 86, 30, 28), NativePanel);
            chestItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            chestItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(chestItemIconPanel, EquipSlots.ChestArmor);
            chestItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 115, 54, 2), NativePanel);

            glovesItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 122, 30, 28), NativePanel);
            glovesItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            glovesItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(glovesItemIconPanel, EquipSlots.Gloves);
            glovesItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 151, 54, 2), NativePanel);

            rightHandItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 158, 30, 28), NativePanel);
            rightHandItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            rightHandItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(rightHandItemIconPanel, EquipSlots.RightHand);
            rightHandItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 187, 54, 2), NativePanel);

            extraInfoItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 14, 30, 28), NativePanel);
            extraInfoItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            extraInfoItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(extraInfoItemIconPanel, EquipSlots.Amulet0);
            extraInfoItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 43, 54, 2), NativePanel);

            leftArmItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 50, 30, 28), NativePanel);
            leftArmItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            leftArmItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(leftArmItemIconPanel, EquipSlots.LeftArm);
            leftArmItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 79, 54, 2), NativePanel);

            legsItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 86, 30, 28), NativePanel);
            legsItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            legsItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(legsItemIconPanel, EquipSlots.LegsArmor);
            legsItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 115, 54, 2), NativePanel);

            bootsItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 122, 30, 28), NativePanel);
            bootsItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            bootsItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(bootsItemIconPanel, EquipSlots.Feet);
            bootsItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 151, 54, 2), NativePanel);

            leftHandItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 158, 30, 28), NativePanel);
            leftHandItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            leftHandItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(leftHandItemIconPanel, EquipSlots.LeftHand);
            leftHandItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 187, 54, 2), NativePanel);
        }

        public void DrawEquipItemToIconPanel(Panel iconPanel, EquipSlots slot)
        {
            PlayerEntity playerEnt = GameManager.Instance.PlayerEntity;
            DaggerfallUnityItem item = playerEnt.ItemEquipTable.GetItem(slot);

            if (item == null)
            {
                iconPanel.BackgroundTexture = null;
                //button.ToolTipText = string.Empty;
                //button.AnimatedBackgroundTextures = null;
                return;
            }

            ImageData image = DaggerfallUnity.Instance.ItemHelper.GetInventoryImage(item);
            iconPanel.BackgroundTexture = image.texture;
            //button.ToolTipText = item.LongName;
            //button.AnimatedBackgroundTextures = (item.IsEnchanted) ? magicAnimation.animatedTextures : null;
        }

        public void AddItemDurabilityBar(Panel itemDurPanel, EquipSlots slot, int index)
        {
            PlayerEntity playerEnt = GameManager.Instance.PlayerEntity;
            DaggerfallUnityItem item = playerEnt.ItemEquipTable.GetItem(slot);

            int maxBarWidth = 54;
            float curDur = testNum1;
            float maxDur = 100f;

            float barWidth = Mathf.Floor((curDur / maxDur) * maxBarWidth);
            float offset = (maxBarWidth - barWidth) / 2;
            float condPerc = (curDur / maxDur) * 100;

            byte colorAlpha = 180;
            Color32 barColor = new Color32(0, 255, 0, colorAlpha);

            // Tomorrow get the rest of the condition bars in place, as well as tie the status to the current condition of the item in that slot, also remove if slot is empty, etc.

            if (condPerc <= 91 && condPerc >= 76)  // Almost New
                barColor = new Color32(120, 255, 0, colorAlpha);
            else if (condPerc <= 75 && condPerc >= 61)  // Slightly Used
                barColor = new Color32(180, 255, 0, colorAlpha);
            else if (condPerc <= 60 && condPerc >= 41)  // Used
                barColor = new Color32(255, 255, 0, colorAlpha);
            else if (condPerc <= 40 && condPerc >= 16)  // Worn
                barColor = new Color32(255, 150, 0, colorAlpha);
            else if (condPerc <= 15)   // Battered & Useless, Broken
                barColor = new Color32(255, 0, 0, colorAlpha);

            itemDurPanel.Components.Clear();
            itemDurBars[index] = DaggerfallUI.AddPanel(new Rect(offset, 0, barWidth, 2), itemDurPanel);
            itemDurBars[index].BackgroundColor = barColor;
            itemDurBars[index].VerticalAlignment = VerticalAlignment.Middle;
        }

        public void UpdatePanels()
        {
            AddItemDurabilityBar(headItemDurabilityBarPanel, EquipSlots.Head, 0);

            //headItemDurabilityBarPanel.Position = new Vector2(butt1.x, butt1.y);
            //headItemDurabilityBarPanel.Size = new Vector2(butt1.width, butt1.height);

            //secondCategoryPanel.Position = new Vector2(butt2.x, butt2.y);
            //secondCategoryPanel.Size = new Vector2(butt2.width, butt2.height);
        }

        /*
        private void InspectChestButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            CloseWindow();
            if (chest.HasBeenInspected) { } // Do nothing, will likely change this eventually, so reinspection/rerolling for inspection results is possible at some cost or something.
            else
            {
                LockedLootContainersMain.ApplyInspectionCosts();
                chest.RecentInspectValues = LockedLootContainersMain.GetInspectionValues(chest);
                chest.HasBeenInspected = true;
            }
            InspectionInfoWindow inspectionInfoWindow = new InspectionInfoWindow(DaggerfallUI.UIManager, chest);
            DaggerfallUI.UIManager.PushWindow(inspectionInfoWindow);
        }

        private void AttemptLockpickButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            CloseWindow();
            if (chest != null)
            {
                DaggerfallAudioSource dfAudioSource = chest.GetComponent<DaggerfallAudioSource>();
                ItemCollection closedChestLoot = chest.AttachedLoot;
                Transform closedChestTransform = chest.transform;
                Vector3 pos = chest.transform.position;

                LockedLootContainersMain.IsThisACrime(ChestInteractionType.Lockpick);

                if (chest.IsLockJammed)
                {
                    DaggerfallUI.AddHUDText(LockedLootContainersMain.GetLockAlreadyJammedText(), 2f);
                    if (dfAudioSource != null && !dfAudioSource.IsPlaying())
                        dfAudioSource.AudioSource.PlayOneShot(LockedLootContainersMain.GetLockAlreadyJammedClip(), UnityEngine.Random.Range(0.9f, 1.42f) * DaggerfallUnity.Settings.SoundVolume);
                }
                else if (LockedLootContainersMain.LockPickChance(chest))
                {
                    chest.PicksAttempted++;
                    LockedLootContainersMain.ApplyLockPickAttemptCosts();

                    DaggerfallLoot openChestLoot = null;
                    if (LockedLootContainersMain.ChestGraphicType == 0) // Use sprite based graphics for chests
                    {
                        int spriteID = closedChestLoot.Count <= 0 ? LockedLootContainersMain.OpenEmptyChestSpriteID : LockedLootContainersMain.OpenFullChestSpriteID;
                        openChestLoot = GameObjectHelper.CreateLootContainer(LootContainerTypes.Nothing, InventoryContainerImages.Chest, pos, closedChestTransform.parent, spriteID, 0, DaggerfallUnity.NextUID, null, false);
                        openChestLoot.gameObject.name = GameObjectHelper.GetGoFlatName(spriteID, 0);
                        openChestLoot.Items.TransferAll(closedChestLoot); // Transfers items from closed chest's items to the new open chest's item collection.
                        GameObject.Destroy(openChestLoot.GetComponent<SerializableLootContainer>());
                    }
                    else // Use 3D models for chests
                    {
                        GameObject usedModelPrefab = null;
                        int modelID = 0;
                        if (closedChestLoot.Count <= 0) { usedModelPrefab = (LockedLootContainersMain.ChestGraphicType == 1) ? LockedLootContainersMain.Instance.LowPolyOpenEmptyChestPrefab : LockedLootContainersMain.Instance.HighPolyOpenEmptyChestPrefab; modelID = LockedLootContainersMain.OpenEmptyChestModelID; }
                        else { usedModelPrefab = (LockedLootContainersMain.ChestGraphicType == 1) ? LockedLootContainersMain.Instance.LowPolyOpenFullChestPrefab : LockedLootContainersMain.Instance.HighPolyOpenFullChestPrefab; modelID = LockedLootContainersMain.OpenFullChestModelID; }
                        GameObject chestGo = GameObjectHelper.InstantiatePrefab(usedModelPrefab, GameObjectHelper.GetGoModelName((uint)modelID), closedChestTransform.parent, pos);
                        chestGo.transform.rotation = chest.gameObject.transform.rotation;
                        Collider col = chestGo.AddComponent<BoxCollider>();
                        openChestLoot = chestGo.AddComponent<DaggerfallLoot>();
                        LockedLootContainersMain.ToggleChestShadowsOrCollision(chestGo);
                        if (openChestLoot)
                        {
                            openChestLoot.ContainerType = LootContainerTypes.Nothing;
                            openChestLoot.ContainerImage = InventoryContainerImages.Chest;
                            openChestLoot.LoadID = DaggerfallUnity.NextUID;
                            openChestLoot.TextureRecord = modelID;
                            openChestLoot.Items.TransferAll(closedChestLoot); // Transfers items from closed chest's items to the new open chest's item collection.
                        }
                    }

                    // Show success and play unlock sound
                    DaggerfallUI.AddHUDText(LockedLootContainersMain.GetLockPickSuccessText(), 3f);
                    if (dfAudioSource != null)
                        AudioSource.PlayClipAtPoint(LockedLootContainersMain.GetLockpickSuccessClip(), chest.gameObject.transform.position, UnityEngine.Random.Range(1.5f, 2.31f) * DaggerfallUnity.Settings.SoundVolume);

                    UnityEngine.Object.Destroy(LockedLootContainersMain.ChestObjRef); // Remove closed chest from scene.
                    LockedLootContainersMain.ChestObjRef = null;
                }
                else
                {
                    chest.PicksAttempted++; // Increase picks attempted counter by 1 on the chest.
                    LockedLootContainersMain.ApplyLockPickAttemptCosts();
                    int mechDamDealt = LockedLootContainersMain.DetermineDamageToLockMechanism(chest);

                    if (LockedLootContainersMain.DoesLockJam(chest, mechDamDealt))
                    {
                        DaggerfallUI.AddHUDText(LockedLootContainersMain.GetJammedLockText(), 3f);
                        if (dfAudioSource != null)
                            AudioSource.PlayClipAtPoint(LockedLootContainersMain.GetLockpickJammedClip(), chest.gameObject.transform.position, UnityEngine.Random.Range(8.2f, 9.71f) * DaggerfallUnity.Settings.SoundVolume);
                    }
                    else
                    {
                        DaggerfallUI.AddHUDText(LockedLootContainersMain.GetLockPickAttemptText(), 2f);
                        if (dfAudioSource != null && !dfAudioSource.IsPlaying())
                            dfAudioSource.AudioSource.PlayOneShot(LockedLootContainersMain.GetLockpickAttemptClip(), UnityEngine.Random.Range(1.2f, 1.91f) * DaggerfallUnity.Settings.SoundVolume);
                    }
                }
            }
            else
            {
                DaggerfallUI.AddHUDText("ERROR: Chest Was Found As Null.", 3f);
            }
        }
        */

        private void ExitButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            CloseWindow();
        }
    }
}
