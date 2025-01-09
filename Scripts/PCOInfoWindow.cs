using UnityEngine;
using DaggerfallWorkshop.Game.UserInterface;
using PhysicalCombatOverhaul;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;

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

        public static byte[] validEquipSlots = new byte[9] {12, 13, 18, 20, 19, 15, 23, 26, 21};

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

        Panel leftArmItemIconPanel;
        Panel legsItemIconPanel;
        Panel bootsItemIconPanel;
        Panel leftHandItemIconPanel;

        Panel headItemDurabilityBarPanel;
        Panel rightArmItemDurabilityBarPanel;
        Panel chestItemDurabilityBarPanel;
        Panel glovesItemDurabilityBarPanel;
        Panel rightHandItemDurabilityBarPanel;

        Panel leftArmItemDurabilityBarPanel;
        Panel legsItemDurabilityBarPanel;
        Panel bootsItemDurabilityBarPanel;
        Panel leftHandItemDurabilityBarPanel;

        Panel headItemTextPanel;
        Panel rightArmItemTextPanel;
        Panel chestItemTextPanel;
        Panel glovesItemTextPanel;
        Panel rightHandItemTextPanel;

        Panel leftArmItemTextPanel;
        Panel legsItemTextPanel;
        Panel bootsItemTextPanel;
        Panel leftHandItemTextPanel;

        Panel extraInfoTextPanel;

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
            headItemIconPanel.Tag = EquipSlots.Head;
            DrawEquipItemToIconPanel(headItemIconPanel, EquipSlots.Head);
            headItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 43, 54, 2), NativePanel);
            AddItemDurabilityBar(headItemDurabilityBarPanel, EquipSlots.Head);
            headItemTextPanel = DaggerfallUI.AddPanel(new Rect(124, 14, 34, 28), NativePanel);
            headItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(headItemTextPanel, EquipSlots.Head, "Head");

            rightArmItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 50, 20, 28), NativePanel);
            rightArmItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            rightArmItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            rightArmItemIconPanel.Tag = EquipSlots.RightArm;
            DrawEquipItemToIconPanel(rightArmItemIconPanel, EquipSlots.RightArm);
            rightArmItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 79, 54, 2), NativePanel);
            AddItemDurabilityBar(rightArmItemDurabilityBarPanel, EquipSlots.RightArm);
            rightArmItemTextPanel = DaggerfallUI.AddPanel(new Rect(124, 50, 34, 28), NativePanel);
            rightArmItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(rightArmItemTextPanel, EquipSlots.RightArm, "Right Arm");

            chestItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 86, 20, 28), NativePanel);
            chestItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            chestItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            chestItemIconPanel.Tag = EquipSlots.ChestArmor;
            DrawEquipItemToIconPanel(chestItemIconPanel, EquipSlots.ChestArmor);
            chestItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 115, 54, 2), NativePanel);
            AddItemDurabilityBar(chestItemDurabilityBarPanel, EquipSlots.ChestArmor);
            chestItemTextPanel = DaggerfallUI.AddPanel(new Rect(124, 86, 34, 28), NativePanel);
            chestItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(chestItemTextPanel, EquipSlots.ChestArmor, "Chest");

            glovesItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 122, 20, 28), NativePanel);
            glovesItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            glovesItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            glovesItemIconPanel.Tag = EquipSlots.Gloves;
            DrawEquipItemToIconPanel(glovesItemIconPanel, EquipSlots.Gloves);
            glovesItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 151, 54, 2), NativePanel);
            AddItemDurabilityBar(glovesItemDurabilityBarPanel, EquipSlots.Gloves);
            glovesItemTextPanel = DaggerfallUI.AddPanel(new Rect(124, 122, 34, 28), NativePanel);
            glovesItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(glovesItemTextPanel, EquipSlots.Gloves, "Gloves");

            rightHandItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 158, 20, 28), NativePanel);
            rightHandItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            rightHandItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            rightHandItemIconPanel.Tag = EquipSlots.RightHand;
            DrawEquipItemToIconPanel(rightHandItemIconPanel, EquipSlots.RightHand);
            rightHandItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 187, 54, 2), NativePanel);
            AddItemDurabilityBar(rightHandItemDurabilityBarPanel, EquipSlots.RightHand);
            rightHandItemTextPanel = DaggerfallUI.AddPanel(new Rect(124, 158, 34, 28), NativePanel);
            rightHandItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(rightHandItemTextPanel, EquipSlots.RightHand, "Right Hand");

            extraInfoTextPanel = DaggerfallUI.AddPanel(new Rect(165, 14, 55, 28), NativePanel);
            extraInfoTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            //AddExtraInfoTextLabels();

            leftArmItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 50, 20, 28), NativePanel);
            leftArmItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            leftArmItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            leftArmItemIconPanel.Tag = EquipSlots.LeftArm;
            DrawEquipItemToIconPanel(leftArmItemIconPanel, EquipSlots.LeftArm);
            leftArmItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 79, 54, 2), NativePanel);
            AddItemDurabilityBar(leftArmItemDurabilityBarPanel, EquipSlots.LeftArm);
            leftArmItemTextPanel = DaggerfallUI.AddPanel(new Rect(186, 50, 34, 28), NativePanel);
            leftArmItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(leftArmItemTextPanel, EquipSlots.LeftArm, "Left Arm");

            legsItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 86, 20, 28), NativePanel);
            legsItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            legsItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            legsItemIconPanel.Tag = EquipSlots.LegsArmor;
            DrawEquipItemToIconPanel(legsItemIconPanel, EquipSlots.LegsArmor);
            legsItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 115, 54, 2), NativePanel);
            AddItemDurabilityBar(legsItemDurabilityBarPanel, EquipSlots.LegsArmor);
            legsItemTextPanel = DaggerfallUI.AddPanel(new Rect(186, 86, 34, 28), NativePanel);
            legsItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(legsItemTextPanel, EquipSlots.LegsArmor, "Legs");

            bootsItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 122, 20, 28), NativePanel);
            bootsItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            bootsItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            bootsItemIconPanel.Tag = EquipSlots.Feet;
            DrawEquipItemToIconPanel(bootsItemIconPanel, EquipSlots.Feet);
            bootsItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 151, 54, 2), NativePanel);
            AddItemDurabilityBar(bootsItemDurabilityBarPanel, EquipSlots.Feet);
            bootsItemTextPanel = DaggerfallUI.AddPanel(new Rect(186, 122, 34, 28), NativePanel);
            bootsItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(bootsItemTextPanel, EquipSlots.Feet, "Feet");

            leftHandItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 158, 20, 28), NativePanel);
            leftHandItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            leftHandItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            leftHandItemIconPanel.Tag = EquipSlots.LeftHand;
            DrawEquipItemToIconPanel(leftHandItemIconPanel, EquipSlots.LeftHand);
            leftHandItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 187, 54, 2), NativePanel);
            AddItemDurabilityBar(leftHandItemDurabilityBarPanel, EquipSlots.LeftHand);
            leftHandItemTextPanel = DaggerfallUI.AddPanel(new Rect(186, 158, 34, 28), NativePanel);
            leftHandItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(leftHandItemTextPanel, EquipSlots.LeftHand, "Left Hand");

            if (extraInfoTextPanel != null)
            {
                SetupEquipSlotPanelsEventSubscriptions();
            }

            // Tomorrow, possibly start working on the text values to be displayed next to the equip slot items, then after that maybe the "extra info" windows, will see.
            // After that maybe add an "EXIT" button, as well as a keybind to open the window instead of the current console command only.
            // And after that maybe see about adding a button to each slot to open a pop-out window to show relevant items that can be equipped to that slot currently in the player inventory?
            // 1/8/2025: I'm thinking I should put a small button that is a child of the "itemIconPanel" for each equip slot, it would probably be a box looking button with a question-mark symbol
            // in it or something. When clicked it would open a sub-window with much more details about that specific item in that equip slot, probably involving damage type and all that, etc.
        }

        public void DrawEquipItemToIconPanel(Panel iconPanel, EquipSlots slot)
        {
            DaggerfallUnityItem item = Player.ItemEquipTable.GetItem(slot);

            if (item == null)
            {
                iconPanel.BackgroundTexture = null;
                iconPanel.ToolTipText = string.Empty;
                //button.AnimatedBackgroundTextures = null;
                return;
            }

            ImageData image = DaggerfallUnity.Instance.ItemHelper.GetInventoryImage(item);
            iconPanel.BackgroundTexture = image.texture;
            iconPanel.ToolTip = defaultToolTip;
            iconPanel.ToolTipText = item.LongName;
            //button.AnimatedBackgroundTextures = (item.IsEnchanted) ? magicAnimation.animatedTextures : null;
        }

        public void AddItemTextLabels(Panel itemTextPanel, EquipSlots slot, string slotName)
        {
            DaggerfallUnityItem item = Player.ItemEquipTable.GetItem(slot);

            int maxLineWidth = (int)itemTextPanel.Size.x;
            int maxHeight = (int)itemTextPanel.Size.y;
            //string toolTipText = string.Format(itemName + "\r" + condName + "\r{0}%        {1} / {2}", Mathf.CeilToInt(condPerc), curDur, maxDur);

            if (item != null)
            {
                if (slot == EquipSlots.RightHand || slot == EquipSlots.LeftHand)
                {
                    if (item.IsShield)
                    {
                        float armorDR = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDRAmount(item, player, false, ref holder) * 100, 1, System.MidpointRounding.AwayFromZero);
                        float armorDT = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDTAmount(item, player, false, ref holder), 1, System.MidpointRounding.AwayFromZero);

                        itemTextPanel.Components.Clear();
                        CreateCenteredTextLabel(slotName, new Vector2(0, 1), maxLineWidth, itemTextPanel);
                        CreateCenteredTextLabel("-------", new Vector2(0, 5), maxLineWidth, itemTextPanel);
                        CreateCenteredTextLabel("DR: " + armorDR + "%", new Vector2(0, 11), maxLineWidth, itemTextPanel);
                        CreateCenteredTextLabel("DT: " + armorDT, new Vector2(0, 19), maxLineWidth, itemTextPanel);
                    }
                    else
                    {
                        int minDamRoll = PhysicalCombatOverhaulMain.AlterDamageBasedOnWepCondition(item.GetBaseDamageMin() + item.GetWeaponMaterialModifier(), true, item);
                        int maxDamRoll = PhysicalCombatOverhaulMain.AlterDamageBasedOnWepCondition(item.GetBaseDamageMax() + item.GetWeaponMaterialModifier(), true, item);

                        itemTextPanel.Components.Clear();
                        CreateCenteredTextLabel(slotName, new Vector2(0, 1), maxLineWidth, itemTextPanel);
                        CreateCenteredTextLabel("-------", new Vector2(0, 5), maxLineWidth, itemTextPanel);
                        CreateCenteredTextLabel("Min: " + minDamRoll, new Vector2(0, 11), maxLineWidth, itemTextPanel);
                        CreateCenteredTextLabel("Max: " + maxDamRoll, new Vector2(0, 19), maxLineWidth, itemTextPanel);
                    }
                }
                else
                {
                    float armorDR = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDRAmount(item, player, false, ref holder) * 100, 1, System.MidpointRounding.AwayFromZero);
                    float armorDT = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDTAmount(item, player, false, ref holder), 1, System.MidpointRounding.AwayFromZero);

                    itemTextPanel.Components.Clear();
                    CreateCenteredTextLabel(slotName, new Vector2(0, 1), maxLineWidth, itemTextPanel);
                    CreateCenteredTextLabel("-------", new Vector2(0, 5), maxLineWidth, itemTextPanel);
                    CreateCenteredTextLabel("DR: " + armorDR + "%", new Vector2(0, 11), maxLineWidth, itemTextPanel);
                    CreateCenteredTextLabel("DT: " + armorDT, new Vector2(0, 19), maxLineWidth, itemTextPanel);
                }
            }
            else
            {
                itemTextPanel.Components.Clear();
                CreateCenteredTextLabel(slotName, new Vector2(0, 1), maxLineWidth, itemTextPanel);
                CreateCenteredTextLabel("-------", new Vector2(0, 5), maxLineWidth, itemTextPanel);
            }
        }

        public void AddItemDurabilityBar(Panel itemDurPanel, EquipSlots slot)
        {
            DaggerfallUnityItem item = Player.ItemEquipTable.GetItem(slot);

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

        public void UpdateItemInfoPanel(DaggerfallUnityItem item, BaseScreenComponent sender)
        {
            if (item == null || sender == null)
            {
                extraInfoTextPanel.Components.Clear();
                return;
            }

            int maxLineWidth = (int)extraInfoTextPanel.Size.x;
            int maxHeight = (int)extraInfoTextPanel.Size.y;
            float textScale = 0.75f;
            EquipSlots slot = (EquipSlots)sender.Tag;

            if (slot == EquipSlots.RightHand || slot == EquipSlots.LeftHand)
            {
                if (item.IsShield)
                {
                    float armorDR = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDRAmount(item, player, true, ref holder) * 100, 1, System.MidpointRounding.AwayFromZero);
                    float armorDT = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDTAmount(item, player, true, ref holder), 1, System.MidpointRounding.AwayFromZero);

                    extraInfoTextPanel.Components.Clear(); // Maybe continue here tomorrow, make some simple methods to get the "types" for armor and weapons, then more after that.
                    CreateCenteredTextLabel(item.LongName, new Vector2(0, 1), maxLineWidth, extraInfoTextPanel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 5), maxLineWidth, extraInfoTextPanel, textScale);
                    CreateCenteredTextLabel("Type: Plate", new Vector2(0, 10), maxLineWidth, extraInfoTextPanel, textScale);
                    CreateCenteredTextLabel("Base DR: " + armorDR + "%", new Vector2(0, 16), maxLineWidth, extraInfoTextPanel, textScale);
                    CreateCenteredTextLabel("Base DT: " + armorDT, new Vector2(0, 22), maxLineWidth, extraInfoTextPanel, textScale);
                }
                else
                {
                    int minDamRoll = item.GetBaseDamageMin() + item.GetWeaponMaterialModifier();
                    int maxDamRoll = item.GetBaseDamageMax() + item.GetWeaponMaterialModifier();

                    extraInfoTextPanel.Components.Clear();
                    CreateCenteredTextLabel(item.LongName, new Vector2(0, 1), maxLineWidth, extraInfoTextPanel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 5), maxLineWidth, extraInfoTextPanel, textScale);
                    CreateCenteredTextLabel("Type: Slash", new Vector2(0, 10), maxLineWidth, extraInfoTextPanel, textScale);
                    CreateCenteredTextLabel("Base Min: " + minDamRoll, new Vector2(0, 16), maxLineWidth, extraInfoTextPanel, textScale);
                    CreateCenteredTextLabel("Base Max: " + maxDamRoll, new Vector2(0, 22), maxLineWidth, extraInfoTextPanel, textScale);
                }
            }
            else
            {
                float armorDR = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDRAmount(item, player, true, ref holder) * 100, 1, System.MidpointRounding.AwayFromZero);
                float armorDT = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDTAmount(item, player, true, ref holder), 1, System.MidpointRounding.AwayFromZero);

                extraInfoTextPanel.Components.Clear();
                CreateCenteredTextLabel(item.LongName, new Vector2(0, 1), maxLineWidth, extraInfoTextPanel, textScale);
                CreateCenteredTextLabel("-------------", new Vector2(0, 5), maxLineWidth, extraInfoTextPanel, textScale);
                CreateCenteredTextLabel("Type: Chainmail", new Vector2(0, 10), maxLineWidth, extraInfoTextPanel, textScale);
                CreateCenteredTextLabel("Base DR: " + armorDR + "%", new Vector2(0, 16), maxLineWidth, extraInfoTextPanel, textScale);
                CreateCenteredTextLabel("Base DT: " + armorDT, new Vector2(0, 22), maxLineWidth, extraInfoTextPanel, textScale);
            }
        }

        public void SetupEquipSlotPanelsEventSubscriptions()
        {
            for (int i = 0; i < validEquipSlots.Length; i++)
            {
                Panel panel = GetPanelRefFromEquipSlot((EquipSlots)validEquipSlots[i]);
                DaggerfallUnityItem item = Player.ItemEquipTable.GetItem((EquipSlots)validEquipSlots[i]);

                if (panel == null) { continue; }

                if (item == null)
                {
                    panel.OnMouseEnter -= UpdateExtraInfoPanel_OnMouseEnter;
                    panel.OnMouseLeave -= UpdateExtraInfoPanel_OnMouseLeave;
                    continue;
                }

                panel.OnMouseEnter += UpdateExtraInfoPanel_OnMouseEnter;
                panel.OnMouseLeave += UpdateExtraInfoPanel_OnMouseLeave;
            }
        }

        public static TextLabel CreateCenteredTextLabel(string text, Vector2 position, int maxWidth, Panel parentPanel, float textScale = 1)
        {
            TextLabel label = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, position, text, parentPanel);
            label.MaxWidth = maxWidth;
            label.TextScale = textScale;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            return label;
        }

        public void UpdateExtraInfoPanel_OnMouseEnter(BaseScreenComponent sender)
        {
            EquipSlots slot = (EquipSlots)sender.Tag;
            DaggerfallUnityItem item = Player.ItemEquipTable.GetItem(slot);

            if (extraInfoTextPanel != null)
                UpdateItemInfoPanel(item, sender);
        }

        public void UpdateExtraInfoPanel_OnMouseLeave(BaseScreenComponent sender)
        {
            if (extraInfoTextPanel != null)
                UpdateItemInfoPanel(null, sender);
        }

        private void ExitButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            CloseWindow();
        }

        public Panel GetPanelRefFromEquipSlot(EquipSlots slot)
        {
            switch (slot)
            {
                case EquipSlots.Head: return headItemIconPanel;
                case EquipSlots.RightArm: return rightArmItemIconPanel;
                case EquipSlots.ChestArmor: return chestItemIconPanel;
                case EquipSlots.Gloves: return glovesItemIconPanel;
                case EquipSlots.RightHand: return rightHandItemIconPanel;
                case EquipSlots.LeftArm: return leftArmItemIconPanel;
                case EquipSlots.LegsArmor: return legsItemIconPanel;
                case EquipSlots.Feet: return bootsItemIconPanel;
                case EquipSlots.LeftHand: return leftHandItemIconPanel;
                default: return null;
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
        */
    }
}
