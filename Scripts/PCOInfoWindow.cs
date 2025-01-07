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

        PhysicalCombatOverhaulMain.CVARS holder;

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

        Panel headItemTextPanel;
        Panel rightArmItemTextPanel;
        Panel chestItemTextPanel;
        Panel glovesItemTextPanel;
        Panel rightHandItemTextPanel;

        Panel extraInfoItemTextPanel;
        Panel leftArmItemTextPanel;
        Panel legsItemTextPanel;
        Panel bootsItemTextPanel;
        Panel leftHandItemTextPanel;

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

            holder = new PhysicalCombatOverhaulMain.CVARS();

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
            headItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 14, 20, 28), NativePanel);
            headItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            headItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(headItemIconPanel, EquipSlots.Head);
            headItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 43, 54, 2), NativePanel);
            AddItemDurabilityBar(headItemDurabilityBarPanel, EquipSlots.Head);
            headItemTextPanel = DaggerfallUI.AddPanel(new Rect(124, 14, 34, 28), NativePanel);
            headItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(headItemTextPanel, EquipSlots.Head, "Head");

            rightArmItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 50, 20, 28), NativePanel);
            rightArmItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            rightArmItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(rightArmItemIconPanel, EquipSlots.RightArm);
            rightArmItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 79, 54, 2), NativePanel);
            AddItemDurabilityBar(rightArmItemDurabilityBarPanel, EquipSlots.RightArm);
            rightArmItemTextPanel = DaggerfallUI.AddPanel(new Rect(124, 50, 34, 28), NativePanel);
            rightArmItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(rightArmItemTextPanel, EquipSlots.RightArm, "Right Arm");

            chestItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 86, 20, 28), NativePanel);
            chestItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            chestItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(chestItemIconPanel, EquipSlots.ChestArmor);
            chestItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 115, 54, 2), NativePanel);
            AddItemDurabilityBar(chestItemDurabilityBarPanel, EquipSlots.ChestArmor);
            chestItemTextPanel = DaggerfallUI.AddPanel(new Rect(124, 86, 34, 28), NativePanel);
            chestItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(chestItemTextPanel, EquipSlots.ChestArmor, "Chest");

            glovesItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 122, 20, 28), NativePanel);
            glovesItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            glovesItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(glovesItemIconPanel, EquipSlots.Gloves);
            glovesItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 151, 54, 2), NativePanel);
            AddItemDurabilityBar(glovesItemDurabilityBarPanel, EquipSlots.Gloves);
            glovesItemTextPanel = DaggerfallUI.AddPanel(new Rect(124, 122, 34, 28), NativePanel);
            glovesItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(glovesItemTextPanel, EquipSlots.Gloves, "Gloves");

            rightHandItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 158, 20, 28), NativePanel);
            rightHandItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            rightHandItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(rightHandItemIconPanel, EquipSlots.RightHand);
            rightHandItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 187, 54, 2), NativePanel);
            AddItemDurabilityBar(rightHandItemDurabilityBarPanel, EquipSlots.RightHand);
            rightHandItemTextPanel = DaggerfallUI.AddPanel(new Rect(124, 158, 34, 28), NativePanel);
            rightHandItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(rightHandItemTextPanel, EquipSlots.RightHand, "Right Hand");

            extraInfoItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 14, 20, 28), NativePanel);
            extraInfoItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            extraInfoItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(extraInfoItemIconPanel, EquipSlots.Amulet0);
            extraInfoItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 43, 54, 2), NativePanel);
            AddItemDurabilityBar(extraInfoItemDurabilityBarPanel, EquipSlots.Amulet0);
            extraInfoItemTextPanel = DaggerfallUI.AddPanel(new Rect(186, 14, 34, 28), NativePanel);
            extraInfoItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(extraInfoItemTextPanel, EquipSlots.Amulet0, "Extra Info");

            leftArmItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 50, 20, 28), NativePanel);
            leftArmItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            leftArmItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(leftArmItemIconPanel, EquipSlots.LeftArm);
            leftArmItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 79, 54, 2), NativePanel);
            AddItemDurabilityBar(leftArmItemDurabilityBarPanel, EquipSlots.LeftArm);
            leftArmItemTextPanel = DaggerfallUI.AddPanel(new Rect(186, 50, 34, 28), NativePanel);
            leftArmItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(leftArmItemTextPanel, EquipSlots.LeftArm, "Left Arm");

            legsItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 86, 20, 28), NativePanel);
            legsItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            legsItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(legsItemIconPanel, EquipSlots.LegsArmor);
            legsItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 115, 54, 2), NativePanel);
            AddItemDurabilityBar(legsItemDurabilityBarPanel, EquipSlots.LegsArmor);
            legsItemTextPanel = DaggerfallUI.AddPanel(new Rect(186, 86, 34, 28), NativePanel);
            legsItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(legsItemTextPanel, EquipSlots.LegsArmor, "Legs");

            bootsItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 122, 20, 28), NativePanel);
            bootsItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            bootsItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(bootsItemIconPanel, EquipSlots.Feet);
            bootsItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 151, 54, 2), NativePanel);
            AddItemDurabilityBar(bootsItemDurabilityBarPanel, EquipSlots.Feet);
            bootsItemTextPanel = DaggerfallUI.AddPanel(new Rect(186, 122, 34, 28), NativePanel);
            bootsItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(bootsItemTextPanel, EquipSlots.Feet, "Feet");

            leftHandItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 158, 20, 28), NativePanel);
            leftHandItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            leftHandItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            DrawEquipItemToIconPanel(leftHandItemIconPanel, EquipSlots.LeftHand);
            leftHandItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 187, 54, 2), NativePanel);
            AddItemDurabilityBar(leftHandItemDurabilityBarPanel, EquipSlots.LeftHand);
            leftHandItemTextPanel = DaggerfallUI.AddPanel(new Rect(186, 158, 34, 28), NativePanel);
            leftHandItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(leftHandItemTextPanel, EquipSlots.LeftHand, "Left Hand");

            // Tomorrow, possibly start working on the text values to be displayed next to the equip slot items, then after that maybe the "extra info" windows, will see.
            // After that maybe add an "EXIT" button, as well as a keybind to open the window instead of the current console command only.
            // And after that maybe see about adding a button to each slot to open a pop-out window to show relevant items that can be equipped to that slot currently in the player inventory?
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

        public void AddItemDurabilityBar(Panel itemDurPanel, EquipSlots slot)
        {
            PlayerEntity playerEnt = GameManager.Instance.PlayerEntity;
            DaggerfallUnityItem item = playerEnt.ItemEquipTable.GetItem(slot);

            if (item != null)
            {
                int maxBarWidth = 54;
                float curDur = item.currentCondition;
                float maxDur = item.maxCondition;

                float barWidth = Mathf.Floor((curDur / maxDur) * maxBarWidth);
                float offset = (maxBarWidth - barWidth) / 2;
                float condPerc = (curDur / maxDur) * 100;

                byte colorAlpha = 180;
                Color32 barColor = new Color32(0, 255, 0, colorAlpha);

                string itemName = item.LongName;
                string condName;

                if (condPerc >= 92) { condName = "New"; barColor = new Color32(120, 255, 0, colorAlpha); }
                else if (condPerc <= 91 && condPerc >= 76) { condName = "Almost New"; barColor = new Color32(120, 255, 0, colorAlpha); }
                else if (condPerc <= 75 && condPerc >= 61) { condName = "Slightly Used"; barColor = new Color32(180, 255, 0, colorAlpha); }
                else if (condPerc <= 60 && condPerc >= 41) { condName = "Used"; barColor = new Color32(255, 255, 0, colorAlpha); }
                else if (condPerc <= 40 && condPerc >= 16) { condName = "Worn"; barColor = new Color32(255, 150, 0, colorAlpha); }
                else if (condPerc <= 15 && condPerc >= 6) { condName = "Battered"; barColor = new Color32(255, 0, 0, colorAlpha); }
                else if (condPerc <= 5) { condName = "Useless"; barColor = new Color32(255, 0, 0, colorAlpha); }
                else { condName = "New"; barColor = new Color32(120, 255, 0, colorAlpha); }

                string toolTipText = string.Format(itemName + "\r" + condName + "\r{0}%        {1} / {2}", Mathf.CeilToInt(condPerc), curDur, maxDur);

                itemDurPanel.Components.Clear();
                Panel itemDurBar = DaggerfallUI.AddPanel(new Rect(offset, 0, barWidth, 2), itemDurPanel);
                itemDurBar.BackgroundColor = barColor;
                itemDurBar.VerticalAlignment = VerticalAlignment.Middle;
                Panel itemDurBarToolTipPanel = DaggerfallUI.AddPanel(new Rect(0, 0, maxBarWidth, 2), itemDurBar);
                itemDurBarToolTipPanel.ToolTip = defaultToolTip;
                itemDurBarToolTipPanel.ToolTipText = toolTipText;
            }
            else
            {
                itemDurPanel.Components.Clear();
            }
        }

        public void AddItemTextLabels(Panel itemTextPanel, EquipSlots slot, string slotName)
        {
            PlayerEntity playerEnt = GameManager.Instance.PlayerEntity;
            DaggerfallUnityItem item = playerEnt.ItemEquipTable.GetItem(slot);

            if (item != null)
            {
                if (slot == EquipSlots.RightHand || slot == EquipSlots.LeftHand)
                {
                    // Work on this tomorrow I suppose, that being showing the different text depending on if using a shield or a weapon, etc.
                }
                else
                {
                    int maxLineWidth = 34;
                    int maxHeight = 28;

                    float armorDR = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDRAmount(item, player, ref holder) * 100, 1, System.MidpointRounding.AwayFromZero);
                    float armorDT = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDTAmount(item, player, ref holder), 1, System.MidpointRounding.AwayFromZero);

                    //string toolTipText = string.Format(itemName + "\r" + condName + "\r{0}%        {1} / {2}", Mathf.CeilToInt(condPerc), curDur, maxDur);

                    itemTextPanel.Components.Clear();
                    TextLabel itemTestText1 = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 1), slotName, itemTextPanel);
                    itemTestText1.HorizontalAlignment = HorizontalAlignment.Center;
                    TextLabel itemTestText2 = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 5), "-------", itemTextPanel);
                    itemTestText2.HorizontalAlignment = HorizontalAlignment.Center;
                    TextLabel itemTestText3 = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 11), "DR: " + armorDR + "%", itemTextPanel);
                    itemTestText3.HorizontalAlignment = HorizontalAlignment.Center;
                    TextLabel itemTestText4 = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 19), "DT: " + armorDT, itemTextPanel);
                    itemTestText4.HorizontalAlignment = HorizontalAlignment.Center;
                }
            }
            else
            {
                itemTextPanel.Components.Clear();
                TextLabel itemTestText1 = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 1), slotName, itemTextPanel);
                itemTestText1.HorizontalAlignment = HorizontalAlignment.Center;
                TextLabel itemTestText2 = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 5), "-------", itemTextPanel);
                itemTestText2.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }

        public void UpdatePanels()
        {
            //AddItemDurabilityBar(headItemDurabilityBarPanel, EquipSlots.Head, 0);

            //extraInfoItemTextPanel.Position = new Vector2(butt1.x, butt1.y);
            //extraInfoItemTextPanel.Size = new Vector2(butt1.width, butt1.height);

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
