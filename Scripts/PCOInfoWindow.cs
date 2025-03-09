using UnityEngine;
using DaggerfallWorkshop.Game.UserInterface;
using PhysicalCombatOverhaul;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using System.Collections.Generic;
using DaggerfallConnect.Arena2;

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
        Texture2D slotBorderTexture;
        Texture2D rightExtraEquipTexture;
        Texture2D leftExtraEquipTexture;
        Texture2D rightItemComparisonTexture;
        Texture2D leftItemComparisonTexture;

        #endregion

        Button exitButton;

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

        Panel headSlotBorderPanel;
        Panel rightArmSlotBorderPanel;
        Panel chestSlotBorderPanel;
        Panel glovesSlotBorderPanel;
        Panel rightHandSlotBorderPanel;

        Panel leftArmSlotBorderPanel;
        Panel legsSlotBorderPanel;
        Panel bootsSlotBorderPanel;
        Panel leftHandSlotBorderPanel;

        Panel extraInfoTextPanel;

        Panel rightExtraEquipPanel;
        Panel leftExtraEquipPanel;

        Panel rightItemComparisonPanel;
        Panel leftItemComparisonPanel;

        Panel rightComparisonMainTextPanel;
        Panel leftComparisonMainTextPanel;

        PCOItemListScroller localPCOItemListScroller;

        ItemCollection localItems = null;
        List<DaggerfallUnityItem> localItemsFiltered = new List<DaggerfallUnityItem>();

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

            localItems = Player.Items;

            SetupTestItemImagePanels();
        }

        protected virtual void LoadTextures()
        {
            baseTexture = PhysicalCombatOverhaulMain.Instance.EquipInfoGUITexture;
            slotBorderTexture = PhysicalCombatOverhaulMain.Instance.EquipInfoSlotBorderTexture;
            rightExtraEquipTexture = PhysicalCombatOverhaulMain.Instance.EquipInfoExtraRightPanelTexture;
            leftExtraEquipTexture = PhysicalCombatOverhaulMain.Instance.EquipInfoExtraLeftPanelTexture;
            rightItemComparisonTexture = PhysicalCombatOverhaulMain.Instance.EquipInfoRightComparisonPanelTexture;
            leftItemComparisonTexture = PhysicalCombatOverhaulMain.Instance.EquipInfoLeftComparisonPanelTexture;
        }

        protected void SetupChestChoiceButtons()
        {
            // Exit button
            exitButton = DaggerfallUI.AddButton(new Rect(144, 190, 36, 9), NativePanel);
            exitButton.OnMouseClick += ExitButton_OnMouseClick;
            exitButton.ClickSound = DaggerfallUI.Instance.GetAudioClip(SoundClips.ButtonClick);
        }

        protected void SetupTestItemImagePanels()
        {
            headItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 14, 20, 28), NativePanel);
            headItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            headItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            headItemIconPanel.Tag = EquipSlots.Head;
            AddEquipSlotSelectionButton(headItemIconPanel, EquipSlots.Head);
            DrawEquipItemToIconPanel(headItemIconPanel, EquipSlots.Head);
            AddEquipSlotSelectionBorderPanel(ref headSlotBorderPanel, new Rect(99, 10, 63, 36));
            headItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 43, 54, 2), NativePanel);
            AddItemDurabilityBar(headItemDurabilityBarPanel, EquipSlots.Head);
            headItemTextPanel = DaggerfallUI.AddPanel(new Rect(124, 14, 34, 28), NativePanel);
            headItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(headItemTextPanel, EquipSlots.Head, "Head");

            rightArmItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 50, 20, 28), NativePanel);
            rightArmItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            rightArmItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            rightArmItemIconPanel.Tag = EquipSlots.RightArm;
            AddEquipSlotSelectionButton(rightArmItemIconPanel, EquipSlots.RightArm);
            DrawEquipItemToIconPanel(rightArmItemIconPanel, EquipSlots.RightArm);
            AddEquipSlotSelectionBorderPanel(ref rightArmSlotBorderPanel, new Rect(99, 46, 63, 36));
            rightArmItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 79, 54, 2), NativePanel);
            AddItemDurabilityBar(rightArmItemDurabilityBarPanel, EquipSlots.RightArm);
            rightArmItemTextPanel = DaggerfallUI.AddPanel(new Rect(124, 50, 34, 28), NativePanel);
            rightArmItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(rightArmItemTextPanel, EquipSlots.RightArm, "Right Arm");

            chestItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 86, 20, 28), NativePanel);
            chestItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            chestItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            chestItemIconPanel.Tag = EquipSlots.ChestArmor;
            AddEquipSlotSelectionButton(chestItemIconPanel, EquipSlots.ChestArmor);
            DrawEquipItemToIconPanel(chestItemIconPanel, EquipSlots.ChestArmor);
            AddEquipSlotSelectionBorderPanel(ref chestSlotBorderPanel, new Rect(99, 82, 63, 36));
            chestItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 115, 54, 2), NativePanel);
            AddItemDurabilityBar(chestItemDurabilityBarPanel, EquipSlots.ChestArmor);
            chestItemTextPanel = DaggerfallUI.AddPanel(new Rect(124, 86, 34, 28), NativePanel);
            chestItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(chestItemTextPanel, EquipSlots.ChestArmor, "Chest");

            glovesItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 122, 20, 28), NativePanel);
            glovesItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            glovesItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            glovesItemIconPanel.Tag = EquipSlots.Gloves;
            AddEquipSlotSelectionButton(glovesItemIconPanel, EquipSlots.Gloves);
            DrawEquipItemToIconPanel(glovesItemIconPanel, EquipSlots.Gloves);
            AddEquipSlotSelectionBorderPanel(ref glovesSlotBorderPanel, new Rect(99, 118, 63, 36));
            glovesItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(104, 151, 54, 2), NativePanel);
            AddItemDurabilityBar(glovesItemDurabilityBarPanel, EquipSlots.Gloves);
            glovesItemTextPanel = DaggerfallUI.AddPanel(new Rect(124, 122, 34, 28), NativePanel);
            glovesItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(glovesItemTextPanel, EquipSlots.Gloves, "Gloves");

            rightHandItemIconPanel = DaggerfallUI.AddPanel(new Rect(103, 158, 20, 28), NativePanel);
            rightHandItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            rightHandItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            rightHandItemIconPanel.Tag = EquipSlots.RightHand;
            AddEquipSlotSelectionButton(rightHandItemIconPanel, EquipSlots.RightHand);
            DrawEquipItemToIconPanel(rightHandItemIconPanel, EquipSlots.RightHand);
            AddEquipSlotSelectionBorderPanel(ref rightHandSlotBorderPanel, new Rect(99, 154, 63, 36));
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
            AddEquipSlotSelectionButton(leftArmItemIconPanel, EquipSlots.LeftArm);
            DrawEquipItemToIconPanel(leftArmItemIconPanel, EquipSlots.LeftArm);
            AddEquipSlotSelectionBorderPanel(ref leftArmSlotBorderPanel, new Rect(161, 46, 63, 36));
            leftArmItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 79, 54, 2), NativePanel);
            AddItemDurabilityBar(leftArmItemDurabilityBarPanel, EquipSlots.LeftArm);
            leftArmItemTextPanel = DaggerfallUI.AddPanel(new Rect(186, 50, 34, 28), NativePanel);
            leftArmItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(leftArmItemTextPanel, EquipSlots.LeftArm, "Left Arm");

            legsItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 86, 20, 28), NativePanel);
            legsItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            legsItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            legsItemIconPanel.Tag = EquipSlots.LegsArmor;
            AddEquipSlotSelectionButton(legsItemIconPanel, EquipSlots.LegsArmor);
            DrawEquipItemToIconPanel(legsItemIconPanel, EquipSlots.LegsArmor);
            AddEquipSlotSelectionBorderPanel(ref legsSlotBorderPanel, new Rect(161, 82, 63, 36));
            legsItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 115, 54, 2), NativePanel);
            AddItemDurabilityBar(legsItemDurabilityBarPanel, EquipSlots.LegsArmor);
            legsItemTextPanel = DaggerfallUI.AddPanel(new Rect(186, 86, 34, 28), NativePanel);
            legsItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(legsItemTextPanel, EquipSlots.LegsArmor, "Legs");

            bootsItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 122, 20, 28), NativePanel);
            bootsItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            bootsItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            bootsItemIconPanel.Tag = EquipSlots.Feet;
            AddEquipSlotSelectionButton(bootsItemIconPanel, EquipSlots.Feet);
            DrawEquipItemToIconPanel(bootsItemIconPanel, EquipSlots.Feet);
            AddEquipSlotSelectionBorderPanel(ref bootsSlotBorderPanel, new Rect(161, 118, 63, 36));
            bootsItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 151, 54, 2), NativePanel);
            AddItemDurabilityBar(bootsItemDurabilityBarPanel, EquipSlots.Feet);
            bootsItemTextPanel = DaggerfallUI.AddPanel(new Rect(186, 122, 34, 28), NativePanel);
            bootsItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(bootsItemTextPanel, EquipSlots.Feet, "Feet");

            leftHandItemIconPanel = DaggerfallUI.AddPanel(new Rect(165, 158, 20, 28), NativePanel);
            leftHandItemIconPanel.BackgroundColor = new Color32(0, 255, 0, 120);
            leftHandItemIconPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            leftHandItemIconPanel.Tag = EquipSlots.LeftHand;
            AddEquipSlotSelectionButton(leftHandItemIconPanel, EquipSlots.LeftHand);
            DrawEquipItemToIconPanel(leftHandItemIconPanel, EquipSlots.LeftHand);
            AddEquipSlotSelectionBorderPanel(ref leftHandSlotBorderPanel, new Rect(161, 154, 63, 36));
            leftHandItemDurabilityBarPanel = DaggerfallUI.AddPanel(new Rect(166, 187, 54, 2), NativePanel);
            AddItemDurabilityBar(leftHandItemDurabilityBarPanel, EquipSlots.LeftHand);
            leftHandItemTextPanel = DaggerfallUI.AddPanel(new Rect(186, 158, 34, 28), NativePanel);
            leftHandItemTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            AddItemTextLabels(leftHandItemTextPanel, EquipSlots.LeftHand, "Left Hand");

            if (extraInfoTextPanel != null)
            {
                SetupEquipSlotPanelsEventSubscriptions();
            }

            rightExtraEquipPanel = DaggerfallUI.AddPanel(new Rect(224, 12, 54, 176), NativePanel);
            rightExtraEquipPanel.BackgroundColor = ScreenDimColor;
            rightExtraEquipPanel.BackgroundTexture = rightExtraEquipTexture;
            rightExtraEquipPanel.Enabled = false;

            leftExtraEquipPanel = DaggerfallUI.AddPanel(new Rect(45, 12, 54, 176), NativePanel);
            leftExtraEquipPanel.BackgroundColor = ScreenDimColor;
            leftExtraEquipPanel.BackgroundTexture = leftExtraEquipTexture;
            leftExtraEquipPanel.Enabled = false;

            rightItemComparisonPanel = DaggerfallUI.AddPanel(new Rect(278, 12, 42, 176), NativePanel);
            rightItemComparisonPanel.BackgroundColor = ScreenDimColor;
            rightItemComparisonPanel.BackgroundTexture = rightItemComparisonTexture;
            rightItemComparisonPanel.Enabled = false;
            SetupItemComparisonPanelComponents(true);

            leftItemComparisonPanel = DaggerfallUI.AddPanel(new Rect(3, 12, 42, 176), NativePanel);
            leftItemComparisonPanel.BackgroundColor = ScreenDimColor;
            leftItemComparisonPanel.BackgroundTexture = leftItemComparisonTexture;
            leftItemComparisonPanel.Enabled = false;
            SetupItemComparisonPanelComponents(false);

            // Tomorrow, now that I imported the "Item Comparison Panel" textures, see about actually implementing them and such.
            // First thing I should do is get the size and position of the panel in the interface and get that working, then go from there, etc.
        }

        public void AddEquipSlotSelectionButton(Panel panel, EquipSlots slot)
        {
            Button slotButton = DaggerfallUI.AddButton(new Rect(panel.Position, panel.Size), NativePanel);
            slotButton.Tag = slot;
            slotButton.OnMouseClick += ShowSlotSelectionBorder_OnLeftMouseClick;
            slotButton.OnRightMouseClick += UnequipSlot_OnRightMouseClick;
            slotButton.ClickSound = DaggerfallUI.Instance.GetAudioClip(SoundClips.ButtonClick);
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

        public void AddEquipSlotSelectionBorderPanel(ref Panel panel, Rect rect)
        {
            panel = DaggerfallUI.AddPanel(rect, NativePanel);
            panel.BackgroundColor = ScreenDimColor;
            panel.BackgroundTexture = slotBorderTexture;
            panel.Enabled = false;
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

        public void UpdateItemInfoPanel(DaggerfallUnityItem item)
        {
            if (item == null)
            {
                extraInfoTextPanel.Components.Clear();
                return;
            }

            int maxLineWidth = (int)extraInfoTextPanel.Size.x;
            int maxHeight = (int)extraInfoTextPanel.Size.y;
            float textScale = 0.75f;
            ItemHands whichHand = ItemEquipTable.GetItemHands(item);

            if (whichHand != ItemHands.None)
            {
                if (item.IsShield)
                {
                    float armorDR = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDRAmount(item, player, true, ref holder) * 100, 1, System.MidpointRounding.AwayFromZero);
                    float armorDT = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDTAmount(item, player, true, ref holder), 1, System.MidpointRounding.AwayFromZero);

                    string shieldMatType = "Plate";
                    int shieldMat = PhysicalCombatOverhaulMain.GetArmorMaterial(item);

                    if (shieldMat == 0) { shieldMatType = "Leather"; }
                    else if (shieldMat == 1) { shieldMatType = "Chain"; }

                    extraInfoTextPanel.Components.Clear();
                    CreateCenteredTextLabel(item.LongName, new Vector2(0, 1), maxLineWidth, extraInfoTextPanel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 5), maxLineWidth, extraInfoTextPanel, textScale);
                    CreateCenteredTextLabel("Type: " + shieldMatType, new Vector2(0, 10), maxLineWidth, extraInfoTextPanel, textScale);
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
                    CreateCenteredTextLabel("Type: " + PhysicalCombatOverhaulMain.GetWeaponAttackTypeName(item), new Vector2(0, 10), maxLineWidth, extraInfoTextPanel, textScale);
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
                CreateCenteredTextLabel("Type: " + (PhysicalCombatOverhaulMain.ArmorType)PhysicalCombatOverhaulMain.GetArmorMatType(item), new Vector2(0, 10), maxLineWidth, extraInfoTextPanel, textScale);
                CreateCenteredTextLabel("Base DR: " + armorDR + "%", new Vector2(0, 16), maxLineWidth, extraInfoTextPanel, textScale);
                CreateCenteredTextLabel("Base DT: " + armorDT, new Vector2(0, 22), maxLineWidth, extraInfoTextPanel, textScale);
            }
        }

        public void UpdateItemComparisonPanel(DaggerfallUnityItem goingItem, bool rightSide)
        {
            if (goingItem == null)
            {
                rightComparisonMainTextPanel.Components.Clear();
                leftComparisonMainTextPanel.Components.Clear();
                return;
            }

            EquipSlots slot = DetermineActiveEquipSlot();
            if (slot == EquipSlots.None)
            {
                rightComparisonMainTextPanel.Components.Clear();
                leftComparisonMainTextPanel.Components.Clear();
                return;
            }

            rightComparisonMainTextPanel.Components.Clear();
            leftComparisonMainTextPanel.Components.Clear();

            Panel usedPanel = null;
            if (rightSide) { usedPanel = rightComparisonMainTextPanel; }
            else { usedPanel = leftComparisonMainTextPanel; }

            ItemHands whichHand = ItemEquipTable.GetItemHands(goingItem);

            // Tomorrow or next time I work on this. Make additional logic for when comparing an item that is going to cause both items currently in the hands to be unequipped.
            // So like if you are wielding a sword and shield, but going to equip a staff or other 2-handed weapon, take the changes of both hands into consideration for the
            // comparison gains and loses text. Same deal if you have an open left-hand and are looking at a 1-handed weapon, this would normally try to equip that in the
            // open left-hand, rather than replace the item in the right-hand, so update the text based on that as well.
            // Essentially make the stuff more context sensitive, I've gotten this far with this comparison text stuff, so this should go about as smoothly, hopefully.
            // Thankfully this only concerns the right and left hand slots, rather than all slots, so will try and do that next time, so far so good on this feature.

            if (whichHand != ItemHands.None)
            {
                DaggerfallUnityItem currRightHandItem = Player.ItemEquipTable.GetItem(EquipSlots.RightHand);
                DaggerfallUnityItem currLeftHandItem = Player.ItemEquipTable.GetItem(EquipSlots.LeftHand);

                if (goingItem.IsShield)
                {
                    if (currRightHandItem != null && ItemEquipTable.GetItemHands(currRightHandItem) == ItemHands.Both)
                    {
                        PopulateComparisonTextLabels(ComparisonType.WeaponToArmor, rightSide, ref usedPanel, ref goingItem, ref currRightHandItem);
                    }
                    else if (currLeftHandItem != null)
                    {
                        if (currLeftHandItem.IsShield)
                        {
                            PopulateComparisonTextLabels(ComparisonType.ArmorToArmor, rightSide, ref usedPanel, ref goingItem, ref currLeftHandItem);
                        }
                        else
                        {
                            PopulateComparisonTextLabels(ComparisonType.WeaponToArmor, rightSide, ref usedPanel, ref goingItem, ref currLeftHandItem);
                        }
                    }
                    else
                    {
                        PopulateComparisonTextLabels(ComparisonType.EmptyToArmor, rightSide, ref usedPanel, ref goingItem, ref currLeftHandItem);
                    }
                }
                else
                {
                    if (currRightHandItem != null && ItemEquipTable.GetItemHands(currRightHandItem) == ItemHands.Both)
                    {
                        PopulateComparisonTextLabels(ComparisonType.WeaponToWeapon, rightSide, ref usedPanel, ref goingItem, ref currRightHandItem);
                    }
                    else if (rightSide)
                    {
                        if (currLeftHandItem != null)
                        {
                            if (currLeftHandItem.IsShield)
                            {
                                PopulateComparisonTextLabels(ComparisonType.ArmorToWeapon, rightSide, ref usedPanel, ref goingItem, ref currLeftHandItem);
                            }
                            else
                            {
                                PopulateComparisonTextLabels(ComparisonType.WeaponToWeapon, rightSide, ref usedPanel, ref goingItem, ref currLeftHandItem);
                            }
                        }
                        else
                        {
                            PopulateComparisonTextLabels(ComparisonType.EmptyToWeapon, rightSide, ref usedPanel, ref goingItem, ref currLeftHandItem);
                        }
                    }
                    else
                    {
                        if (currRightHandItem != null)
                        {
                            PopulateComparisonTextLabels(ComparisonType.WeaponToWeapon, rightSide, ref usedPanel, ref goingItem, ref currRightHandItem);
                        }
                        else
                        {
                            PopulateComparisonTextLabels(ComparisonType.EmptyToWeapon, rightSide, ref usedPanel, ref goingItem, ref currRightHandItem);
                        }
                    }
                }
            }
            else
            {
                DaggerfallUnityItem currItem = Player.ItemEquipTable.GetItem(slot);

                if (currItem != null)
                {
                    PopulateComparisonTextLabels(ComparisonType.ArmorToArmor, rightSide, ref usedPanel, ref goingItem, ref currItem);
                }
                else
                {
                    PopulateComparisonTextLabels(ComparisonType.EmptyToArmor, rightSide, ref usedPanel, ref goingItem, ref currItem);
                }
            }
        }

        public void PopulateComparisonTextLabels(ComparisonType type, bool rightSide, ref Panel panel, ref DaggerfallUnityItem goingItem, ref DaggerfallUnityItem currItem)
        {
            int maxLineWidth = (int)rightComparisonMainTextPanel.Size.x;
            int maxHeight = (int)rightComparisonMainTextPanel.Size.y;
            float textScale = 0.55f;
            float textScaleMod = 0.25f;

            int currLineYPos = 95;
            int lineChange = 6;
            int bigLineChange = 10;
            List<string> gains = new List<string>();
            List<string> loses = new List<string>();

            float armorDRCurr;
            float armorDTCurr;
            float armorDRGoing;
            float armorDTGoing;
            float armorDRChange;
            float armorDTChange;

            int minDamRollCurr;
            int maxDamRollCurr;
            int minDamRollGoing;
            int maxDamRollGoing;
            int minDamRollChange;
            int maxDamRollChange;

            string armorMatTypeCurr;
            int armorMatCurr;
            string armorMatTypeGoing;
            int armorMatGoing;

            switch (type)
            {
                case ComparisonType.ArmorToArmor:
                    armorDRCurr = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDRAmount(currItem, player, true, ref holder) * 100, 1, System.MidpointRounding.AwayFromZero);
                    armorDTCurr = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDTAmount(currItem, player, true, ref holder), 1, System.MidpointRounding.AwayFromZero);

                    armorMatTypeCurr = "Plate";
                    armorMatCurr = PhysicalCombatOverhaulMain.GetArmorMaterial(currItem);

                    if (armorMatCurr == 0) { armorMatTypeCurr = "Leather"; }
                    else if (armorMatCurr == 1) { armorMatTypeCurr = "Chain"; }

                    CreateCenteredTextLabel("Currently Equipped", new Vector2(0, 1), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 5), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Type: " + armorMatTypeCurr, new Vector2(0, 10), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base DR: " + armorDRCurr + "%", new Vector2(0, 16), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base DT: " + armorDTCurr, new Vector2(0, 22), maxLineWidth, panel, textScale);

                    armorDRGoing = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDRAmount(goingItem, player, true, ref holder) * 100, 1, System.MidpointRounding.AwayFromZero);
                    armorDTGoing = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDTAmount(goingItem, player, true, ref holder), 1, System.MidpointRounding.AwayFromZero);

                    armorMatTypeGoing = "Plate";
                    armorMatGoing = PhysicalCombatOverhaulMain.GetArmorMaterial(goingItem);

                    if (armorMatGoing == 0) { armorMatTypeGoing = "Leather"; }
                    else if (armorMatGoing == 1) { armorMatTypeGoing = "Chain"; }

                    CreateCenteredTextLabel("To Be Equipped", new Vector2(0, 41), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 45), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Type: " + armorMatTypeGoing, new Vector2(0, 50), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base DR: " + armorDRGoing + "%", new Vector2(0, 56), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base DT: " + armorDTGoing, new Vector2(0, 62), maxLineWidth, panel, textScale);

                    armorDRChange = armorDRGoing - armorDRCurr;
                    armorDTChange = (float)System.Math.Round(armorDTGoing - armorDTCurr, 1, System.MidpointRounding.AwayFromZero);

                    CreateCenteredTextLabel("What Will Change", new Vector2(0, 81), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 85), maxLineWidth, panel, textScale);

                    if (armorDRChange > 0) { gains.Add("DR: +" + armorDRChange + "%"); }
                    else if (armorDRChange < 0) { loses.Add("DR: " + armorDRChange + "%"); }
                    if (armorDTChange > 0) { gains.Add("DT: +" + armorDTChange); }
                    else if (armorDTChange < 0) { loses.Add("DT: " + armorDTChange); }

                    if (gains.Count > 0)
                    {
                        CreateCenteredTextLabel("Gain:", new Vector2(0, currLineYPos), maxLineWidth, panel, textScale + textScaleMod);
                        currLineYPos += lineChange;

                        foreach (string line in gains)
                        {
                            CreateCenteredTextLabel(line, new Vector2(0, currLineYPos), maxLineWidth, panel, textScale + textScaleMod, DaggerfallUI.DaggerfallForcedEnchantmentTextColor);
                            currLineYPos += lineChange;
                        }
                        currLineYPos += bigLineChange;
                    }

                    if (loses.Count > 0)
                    {
                        CreateCenteredTextLabel("Lose:", new Vector2(0, currLineYPos), maxLineWidth, panel, textScale + textScaleMod);
                        currLineYPos += lineChange;

                        foreach (string line in loses)
                        {
                            CreateCenteredTextLabel(line, new Vector2(0, currLineYPos), maxLineWidth, panel, textScale + textScaleMod, DaggerfallUI.DaggerfallUnityStatDrainedTextColor);
                            currLineYPos += lineChange;
                        }
                        currLineYPos += bigLineChange;
                    }
                    break;
                case ComparisonType.WeaponToWeapon:
                    minDamRollCurr = currItem.GetBaseDamageMin() + currItem.GetWeaponMaterialModifier();
                    maxDamRollCurr = currItem.GetBaseDamageMax() + currItem.GetWeaponMaterialModifier();

                    CreateCenteredTextLabel("Currently Equipped", new Vector2(0, 1), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 5), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Type: " + PhysicalCombatOverhaulMain.GetWeaponAttackTypeName(currItem), new Vector2(0, 10), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base Min: " + minDamRollCurr, new Vector2(0, 16), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base Max: " + maxDamRollCurr, new Vector2(0, 22), maxLineWidth, panel, textScale);

                    minDamRollGoing = goingItem.GetBaseDamageMin() + goingItem.GetWeaponMaterialModifier();
                    maxDamRollGoing = goingItem.GetBaseDamageMax() + goingItem.GetWeaponMaterialModifier();

                    CreateCenteredTextLabel("To Be Equipped", new Vector2(0, 41), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 45), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Type: " + PhysicalCombatOverhaulMain.GetWeaponAttackTypeName(goingItem), new Vector2(0, 50), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base Min: " + minDamRollGoing, new Vector2(0, 56), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base Max: " + maxDamRollGoing, new Vector2(0, 62), maxLineWidth, panel, textScale);

                    minDamRollChange = minDamRollGoing - minDamRollCurr;
                    maxDamRollChange = maxDamRollGoing - maxDamRollCurr;

                    CreateCenteredTextLabel("What Will Change", new Vector2(0, 81), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 85), maxLineWidth, panel, textScale);

                    if (minDamRollChange > 0) { gains.Add("Base Min: +" + minDamRollChange); }
                    else if (minDamRollChange < 0) { loses.Add("Base Min: " + minDamRollChange); }
                    if (maxDamRollChange > 0) { gains.Add("Base Max: +" + maxDamRollChange); }
                    else if (maxDamRollChange < 0) { loses.Add("Base Max: " + maxDamRollChange); }

                    if (gains.Count > 0)
                    {
                        CreateCenteredTextLabel("Gain:", new Vector2(0, currLineYPos), maxLineWidth, panel, textScale + 0.25f);
                        currLineYPos += lineChange;

                        foreach (string line in gains)
                        {
                            CreateCenteredTextLabel(line, new Vector2(0, currLineYPos), maxLineWidth, panel, textScale + 0.25f, DaggerfallUI.DaggerfallForcedEnchantmentTextColor);
                            currLineYPos += lineChange;
                        }
                        currLineYPos += bigLineChange;
                    }

                    if (loses.Count > 0)
                    {
                        CreateCenteredTextLabel("Lose:", new Vector2(0, currLineYPos), maxLineWidth, panel, textScale + 0.25f);
                        currLineYPos += lineChange;

                        foreach (string line in loses)
                        {
                            CreateCenteredTextLabel(line, new Vector2(0, currLineYPos), maxLineWidth, panel, textScale + 0.25f, DaggerfallUI.DaggerfallUnityStatDrainedTextColor);
                            currLineYPos += lineChange;
                        }
                        currLineYPos += bigLineChange;
                    }
                    break;
                case ComparisonType.ArmorToWeapon:
                    armorDRCurr = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDRAmount(currItem, player, true, ref holder) * 100, 1, System.MidpointRounding.AwayFromZero);
                    armorDTCurr = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDTAmount(currItem, player, true, ref holder), 1, System.MidpointRounding.AwayFromZero);

                    armorMatTypeCurr = "Plate";
                    armorMatCurr = PhysicalCombatOverhaulMain.GetArmorMaterial(currItem);

                    if (armorMatCurr == 0) { armorMatTypeCurr = "Leather"; }
                    else if (armorMatCurr == 1) { armorMatTypeCurr = "Chain"; }

                    CreateCenteredTextLabel("Currently Equipped", new Vector2(0, 1), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 5), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Type: " + armorMatTypeCurr, new Vector2(0, 10), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base DR: " + armorDRCurr + "%", new Vector2(0, 16), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base DT: " + armorDTCurr, new Vector2(0, 22), maxLineWidth, panel, textScale);

                    minDamRollGoing = goingItem.GetBaseDamageMin() + goingItem.GetWeaponMaterialModifier();
                    maxDamRollGoing = goingItem.GetBaseDamageMax() + goingItem.GetWeaponMaterialModifier();

                    CreateCenteredTextLabel("To Be Equipped", new Vector2(0, 41), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 45), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Type: " + PhysicalCombatOverhaulMain.GetWeaponAttackTypeName(goingItem), new Vector2(0, 50), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base Min: " + minDamRollGoing, new Vector2(0, 56), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base Max: " + maxDamRollGoing, new Vector2(0, 62), maxLineWidth, panel, textScale);

                    CreateCenteredTextLabel("What Will Change", new Vector2(0, 81), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 85), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Gain:", new Vector2(0, 95), maxLineWidth, panel, textScale + textScaleMod);
                    CreateCenteredTextLabel("Dam Min: +" + minDamRollGoing, new Vector2(0, 101), maxLineWidth, panel, textScale + textScaleMod, DaggerfallUI.DaggerfallForcedEnchantmentTextColor);
                    CreateCenteredTextLabel("Dam Max: +" + maxDamRollGoing, new Vector2(0, 107), maxLineWidth, panel, textScale + textScaleMod, DaggerfallUI.DaggerfallForcedEnchantmentTextColor);
                    CreateCenteredTextLabel("Lose:", new Vector2(0, 117), maxLineWidth, panel, textScale + textScaleMod);
                    CreateCenteredTextLabel("DR: -" + armorDRCurr + "%", new Vector2(0, 123), maxLineWidth, panel, textScale + textScaleMod, DaggerfallUI.DaggerfallUnityStatDrainedTextColor);
                    CreateCenteredTextLabel("DT: -" + armorDTCurr, new Vector2(0, 129), maxLineWidth, panel, textScale + textScaleMod, DaggerfallUI.DaggerfallUnityStatDrainedTextColor);
                    break;
                case ComparisonType.WeaponToArmor:
                    minDamRollCurr = currItem.GetBaseDamageMin() + currItem.GetWeaponMaterialModifier();
                    maxDamRollCurr = currItem.GetBaseDamageMax() + currItem.GetWeaponMaterialModifier();

                    CreateCenteredTextLabel("Currently Equipped", new Vector2(0, 1), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 5), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Type: " + PhysicalCombatOverhaulMain.GetWeaponAttackTypeName(currItem), new Vector2(0, 10), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base Min: " + minDamRollCurr, new Vector2(0, 16), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base Max: " + maxDamRollCurr, new Vector2(0, 22), maxLineWidth, panel, textScale);

                    armorDRGoing = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDRAmount(goingItem, player, true, ref holder) * 100, 1, System.MidpointRounding.AwayFromZero);
                    armorDTGoing = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDTAmount(goingItem, player, true, ref holder), 1, System.MidpointRounding.AwayFromZero);

                    armorMatTypeGoing = "Plate";
                    armorMatGoing = PhysicalCombatOverhaulMain.GetArmorMaterial(goingItem);

                    if (armorMatGoing == 0) { armorMatTypeGoing = "Leather"; }
                    else if (armorMatGoing == 1) { armorMatTypeGoing = "Chain"; }

                    CreateCenteredTextLabel("To Be Equipped", new Vector2(0, 41), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 45), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Type: " + armorMatTypeGoing, new Vector2(0, 50), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base DR: " + armorDRGoing + "%", new Vector2(0, 56), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base DT: " + armorDTGoing, new Vector2(0, 62), maxLineWidth, panel, textScale);

                    CreateCenteredTextLabel("What Will Change", new Vector2(0, 81), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 85), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Gain:", new Vector2(0, 95), maxLineWidth, panel, textScale + textScaleMod);
                    CreateCenteredTextLabel("DR: +" + armorDRGoing + "%", new Vector2(0, 101), maxLineWidth, panel, textScale + textScaleMod, DaggerfallUI.DaggerfallForcedEnchantmentTextColor);
                    CreateCenteredTextLabel("DT: +" + armorDTGoing, new Vector2(0, 107), maxLineWidth, panel, textScale + textScaleMod, DaggerfallUI.DaggerfallForcedEnchantmentTextColor);
                    CreateCenteredTextLabel("Lose:", new Vector2(0, 117), maxLineWidth, panel, textScale + textScaleMod);
                    CreateCenteredTextLabel("Dam Min: -" + minDamRollCurr, new Vector2(0, 123), maxLineWidth, panel, textScale + textScaleMod, DaggerfallUI.DaggerfallUnityStatDrainedTextColor);
                    CreateCenteredTextLabel("Dam Max: -" + maxDamRollCurr, new Vector2(0, 129), maxLineWidth, panel, textScale + textScaleMod, DaggerfallUI.DaggerfallUnityStatDrainedTextColor);
                    break;
                case ComparisonType.EmptyToArmor:
                    armorDRGoing = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDRAmount(goingItem, player, true, ref holder) * 100, 1, System.MidpointRounding.AwayFromZero);
                    armorDTGoing = (float)System.Math.Round(PhysicalCombatOverhaulMain.GetBaseDTAmount(goingItem, player, true, ref holder), 1, System.MidpointRounding.AwayFromZero);

                    armorMatTypeGoing = "Plate";
                    armorMatGoing = PhysicalCombatOverhaulMain.GetArmorMaterial(goingItem);

                    if (armorMatGoing == 0) { armorMatTypeGoing = "Leather"; }
                    else if (armorMatGoing == 1) { armorMatTypeGoing = "Chain"; }

                    CreateCenteredTextLabel("To Be Equipped", new Vector2(0, 41), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 45), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Type: " + armorMatTypeGoing, new Vector2(0, 50), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base DR: " + armorDRGoing + "%", new Vector2(0, 56), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base DT: " + armorDTGoing, new Vector2(0, 62), maxLineWidth, panel, textScale);

                    CreateCenteredTextLabel("What Will Change", new Vector2(0, 81), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 85), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Gain:", new Vector2(0, 95), maxLineWidth, panel, textScale + textScaleMod);
                    CreateCenteredTextLabel("DR: +" + armorDRGoing + "%", new Vector2(0, 101), maxLineWidth, panel, textScale + textScaleMod, DaggerfallUI.DaggerfallForcedEnchantmentTextColor);
                    CreateCenteredTextLabel("DT: +" + armorDTGoing, new Vector2(0, 107), maxLineWidth, panel, textScale + textScaleMod, DaggerfallUI.DaggerfallForcedEnchantmentTextColor);
                    break;
                case ComparisonType.EmptyToWeapon:
                    minDamRollGoing = goingItem.GetBaseDamageMin() + goingItem.GetWeaponMaterialModifier();
                    maxDamRollGoing = goingItem.GetBaseDamageMax() + goingItem.GetWeaponMaterialModifier();

                    CreateCenteredTextLabel("To Be Equipped", new Vector2(0, 41), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 45), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Type: " + PhysicalCombatOverhaulMain.GetWeaponAttackTypeName(goingItem), new Vector2(0, 50), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base Min: " + minDamRollGoing, new Vector2(0, 56), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Base Max: " + maxDamRollGoing, new Vector2(0, 62), maxLineWidth, panel, textScale);

                    CreateCenteredTextLabel("What Will Change", new Vector2(0, 81), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("-------------", new Vector2(0, 85), maxLineWidth, panel, textScale);
                    CreateCenteredTextLabel("Gain:", new Vector2(0, 95), maxLineWidth, panel, textScale + textScaleMod);
                    CreateCenteredTextLabel("Dam Min: +" + minDamRollGoing, new Vector2(0, 101), maxLineWidth, panel, textScale + textScaleMod, DaggerfallUI.DaggerfallForcedEnchantmentTextColor);
                    CreateCenteredTextLabel("Dam Max: +" + maxDamRollGoing, new Vector2(0, 107), maxLineWidth, panel, textScale + textScaleMod, DaggerfallUI.DaggerfallForcedEnchantmentTextColor);
                    break;
                default: break;
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

        public void SetupItemComparisonPanelComponents(bool rightSide)
        {
            if (rightSide)
            {
                rightItemComparisonPanel.Components.Clear();
                rightComparisonMainTextPanel = DaggerfallUI.AddPanel(new Rect(4, 4, 34, 168), rightItemComparisonPanel);
                //rightComparisonMainTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            }
            else
            {
                leftItemComparisonPanel.Components.Clear();
                leftComparisonMainTextPanel = DaggerfallUI.AddPanel(new Rect(4, 4, 34, 168), leftItemComparisonPanel);
                //leftComparisonMainTextPanel.BackgroundColor = new Color32(255, 0, 0, 120);
            }
        }

        public static TextLabel CreateCenteredTextLabel(string text, Vector2 position, int maxWidth, Panel parentPanel, float textScale = 1, Color32? color = null)
        {
            if (color == null) { color = DaggerfallUI.DaggerfallDefaultTextColor; }

            TextLabel label = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, position, text, parentPanel);
            label.TextColor = (Color)color;
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
                UpdateItemInfoPanel(item);
        }

        public void UpdateExtraInfoPanel_OnMouseLeave(BaseScreenComponent sender)
        {
            if (extraInfoTextPanel != null)
                UpdateItemInfoPanel(null);
        }

        private void ShowSlotSelectionBorder_OnLeftMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            EquipSlots slot = (EquipSlots)sender.Tag;

            switch (slot)
            {
                case EquipSlots.Head: ToggleSlotBorderPanels(headSlotBorderPanel.Enabled, ref headSlotBorderPanel); break;
                case EquipSlots.RightArm: ToggleSlotBorderPanels(rightArmSlotBorderPanel.Enabled, ref rightArmSlotBorderPanel); break;
                case EquipSlots.ChestArmor: ToggleSlotBorderPanels(chestSlotBorderPanel.Enabled, ref chestSlotBorderPanel); break;
                case EquipSlots.Gloves: ToggleSlotBorderPanels(glovesSlotBorderPanel.Enabled, ref glovesSlotBorderPanel); break;
                case EquipSlots.RightHand: ToggleSlotBorderPanels(rightHandSlotBorderPanel.Enabled, ref rightHandSlotBorderPanel); break;
                case EquipSlots.LeftArm: ToggleSlotBorderPanels(leftArmSlotBorderPanel.Enabled, ref leftArmSlotBorderPanel); break;
                case EquipSlots.LegsArmor: ToggleSlotBorderPanels(legsSlotBorderPanel.Enabled, ref legsSlotBorderPanel); break;
                case EquipSlots.Feet: ToggleSlotBorderPanels(bootsSlotBorderPanel.Enabled, ref bootsSlotBorderPanel); break;
                case EquipSlots.LeftHand: ToggleSlotBorderPanels(leftHandSlotBorderPanel.Enabled, ref leftHandSlotBorderPanel); break;
                default: DisabledAllSlotBorderPanels(); break;
            }

            localPCOItemListScroller = null;
            rightExtraEquipPanel.Enabled = false;
            leftExtraEquipPanel.Enabled = false;
            rightItemComparisonPanel.Enabled = false;
            leftItemComparisonPanel.Enabled = false;

            if (headSlotBorderPanel.Enabled || rightArmSlotBorderPanel.Enabled || chestSlotBorderPanel.Enabled || glovesSlotBorderPanel.Enabled || rightHandSlotBorderPanel.Enabled)
            {
                leftExtraEquipPanel.Enabled = true;
                leftItemComparisonPanel.Enabled = true;
                SetupLocalPCOItemListScroller(false, slot);
            }
            else if (leftArmSlotBorderPanel.Enabled || legsSlotBorderPanel.Enabled || bootsSlotBorderPanel.Enabled || leftHandSlotBorderPanel.Enabled)
            {
                rightExtraEquipPanel.Enabled = true;
                rightItemComparisonPanel.Enabled = true;
                SetupLocalPCOItemListScroller(true, slot);
            }
        }

        private void ToggleSlotBorderPanels(bool currentlyEnabled, ref Panel panel)
        {
            DisabledAllSlotBorderPanels();

            if (!currentlyEnabled)
            {
                panel.Enabled = true;
            }
        }

        private void DisabledAllSlotBorderPanels()
        {
            headSlotBorderPanel.Enabled = false;
            rightArmSlotBorderPanel.Enabled = false;
            chestSlotBorderPanel.Enabled = false;
            glovesSlotBorderPanel.Enabled = false;
            rightHandSlotBorderPanel.Enabled = false;
            leftArmSlotBorderPanel.Enabled = false;
            legsSlotBorderPanel.Enabled = false;
            bootsSlotBorderPanel.Enabled = false;
            leftHandSlotBorderPanel.Enabled = false;
        }

        private void UnequipSlot_OnRightMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            EquipSlots slot = (EquipSlots)sender.Tag;

            if (Player.ItemEquipTable.IsSlotOpen(slot))
            {
                return;
            }

            List<DaggerfallUnityItem> unequippedList = new List<DaggerfallUnityItem>();

            // Try to unequip the item, and update armour values accordingly
            Player.ItemEquipTable.UnequipItem(slot, unequippedList);
            if (unequippedList != null)
            {
                foreach (DaggerfallUnityItem unequippedItem in unequippedList)
                {
                    Player.UpdateEquippedArmorValues(unequippedItem, false);

                    // Play click sound
                    DaggerfallUI.Instance.PlayOneShot(DaggerfallUI.Instance.GetAudioClip(SoundClips.ButtonClick));
                }
            }

            RefreshEquipScreen(slot);
        }

        private void SetupLocalPCOItemListScroller(bool rightSide, EquipSlots slot)
        {
            localPCOItemListScroller = new PCOItemListScroller(defaultToolTip, slot, rightSide)
            {
                Position = new Vector2(0, 0),
                Size = new Vector2(54, 176),
                //BackgroundColourHandler = ItemBackgroundColourHandler,
                //ForegroundAnimationHandler = MagicItemForegroundAnimationHander,
                //ForegroundAnimationDelay = magicAnimationDelay
            };

            rightExtraEquipPanel.Components.Clear();
            leftExtraEquipPanel.Components.Clear();

            if (rightSide) { rightExtraEquipPanel.Components.Add(localPCOItemListScroller); }
            else { leftExtraEquipPanel.Components.Add(localPCOItemListScroller); }

            localPCOItemListScroller.OnItemClick += LocalItemListScroller_OnItemLeftClick;
            //localPCOItemListScroller.OnItemRightClick += LocalItemListScroller_OnItemRightClick;
            //localPCOItemListScroller.OnItemMiddleClick += LocalItemListScroller_OnItemMiddleClick;
            if (extraInfoTextPanel != null) { localPCOItemListScroller.OnItemHover += LocalItemListScroller_OnHover; }

            // Tomorrow, see about working on and experimenting with another panel that will show a comparison between stats of the currently equipped
            // item, and whatever item is being hovered over in the item scroller panel, similar to that stat comparions function in say World of Warcraft, etc.

            FilterLocalItems(slot);
            localPCOItemListScroller.Items = localItemsFiltered;
        }

        private void FilterLocalItems(EquipSlots slot)
        {
            // Clear current references
            localItemsFiltered.Clear();

            if (localItems != null)
            {
                // Add items to list
                for (int i = 0; i < localItems.Count; i++)
                {
                    DaggerfallUnityItem item = localItems.GetItem(i);
                    // Add if not equipped
                    if (!item.IsEquipped)
                    {
                        AddLocalItem(item, slot);
                    }
                }
            }
        }

        private void AddLocalItem(DaggerfallUnityItem item, EquipSlots slot)
        {
            bool isWeaponOrArmor = (item.ItemGroup == ItemGroups.Weapons || item.ItemGroup == ItemGroups.Armor);

            if (isWeaponOrArmor)
            {
                ItemHands whichHand = ItemEquipTable.GetItemHands(item);

                if (slot == EquipSlots.LeftHand)
                {
                    if (whichHand == ItemHands.LeftOnly)
                    {
                        localItemsFiltered.Add(item);
                        return;
                    }
                }

                if (slot == EquipSlots.RightHand)
                {
                    if (whichHand == ItemHands.Either || whichHand == ItemHands.Both || whichHand == ItemHands.RightOnly)
                    {
                        localItemsFiltered.Add(item);
                        return;
                    }
                }

                if (slot == Player.ItemEquipTable.GetEquipSlot(item))
                {
                    localItemsFiltered.Add(item);
                }
            }
        }

        protected virtual void LocalItemListScroller_OnItemLeftClick(DaggerfallUnityItem item)
        {
            LocalItemListScroller_OnItemClick(item);
        }

        protected virtual void LocalItemListScroller_OnItemClick(DaggerfallUnityItem item)
        {
            EquipItem(item);
        }

        protected void EquipItem(DaggerfallUnityItem item)
        {
            const int itemBrokenTextId = 29;
            const int forbiddenEquipmentTextId = 1068;

            if (item.currentCondition < 1)
            {
                TextFile.Token[] tokens = DaggerfallUnity.TextProvider.GetRSCTokens(itemBrokenTextId);
                if (tokens != null && tokens.Length > 0)
                {
                    DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                    messageBox.SetTextTokens(tokens, item);
                    messageBox.ClickAnywhereToClose = true;
                    messageBox.Show();
                }
                return;
            }

            bool prohibited = false;

            if (item.ItemGroup == ItemGroups.Armor)
            {
                // Check for prohibited shield
                if (item.IsShield && ((1 << (item.TemplateIndex - (int)Armor.Buckler) & (int)Player.Career.ForbiddenShields) != 0))
                    prohibited = true;

                // Check for prohibited armor type (leather, chain or plate)
                else if (!item.IsShield && (1 << (item.NativeMaterialValue >> 8) & (int)Player.Career.ForbiddenArmors) != 0)
                    prohibited = true;

                // Check for prohibited material
                else if (((item.nativeMaterialValue >> 8) == 2)
                    && (1 << (item.NativeMaterialValue & 0xFF) & (int)Player.Career.ForbiddenMaterials) != 0)
                    prohibited = true;
            }
            else if (item.ItemGroup == ItemGroups.Weapons)
            {
                // Check for prohibited weapon type
                if ((item.GetWeaponSkillUsed() & (int)Player.Career.ForbiddenProficiencies) != 0)
                    prohibited = true;
                // Check for prohibited material
                else if ((1 << item.NativeMaterialValue & (int)Player.Career.ForbiddenMaterials) != 0)
                    prohibited = true;
            }

            if (prohibited)
            {
                TextFile.Token[] tokens = DaggerfallUnity.Instance.TextProvider.GetRSCTokens(forbiddenEquipmentTextId);
                if (tokens != null && tokens.Length > 0)
                {
                    DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                    messageBox.SetTextTokens(tokens);
                    messageBox.ClickAnywhereToClose = true;
                    messageBox.Show();
                }
                return;
            }

            // Try to equip the item, and update armour values accordingly
            List<DaggerfallUnityItem> unequippedList = Player.ItemEquipTable.EquipItem(item);
            if (unequippedList != null)
            {
                foreach (DaggerfallUnityItem unequippedItem in unequippedList)
                {
                    Player.UpdateEquippedArmorValues(unequippedItem, false);
                }
                Player.UpdateEquippedArmorValues(item, true);
            }

            RefreshEquipScreen();
        }

        public void RefreshEquipScreen(EquipSlots slot = EquipSlots.None)
        {
            if (localPCOItemListScroller != null)
            {
                if (slot == EquipSlots.None)
                {
                    slot = localPCOItemListScroller.AssociatedSlot;
                }

                if (slot == localPCOItemListScroller.AssociatedSlot)
                {
                    FilterLocalItems(slot);
                    localPCOItemListScroller.Items = localItemsFiltered;
                }
            }

            switch (slot)
            {
                case EquipSlots.Head:
                    headItemIconPanel.Components.Clear();
                    DrawEquipItemToIconPanel(headItemIconPanel, EquipSlots.Head);
                    AddItemDurabilityBar(headItemDurabilityBarPanel, EquipSlots.Head);
                    headItemTextPanel.Components.Clear();
                    AddItemTextLabels(headItemTextPanel, EquipSlots.Head, "Head"); break;
                case EquipSlots.RightArm:
                    rightArmItemIconPanel.Components.Clear();
                    DrawEquipItemToIconPanel(rightArmItemIconPanel, EquipSlots.RightArm);
                    AddItemDurabilityBar(rightArmItemDurabilityBarPanel, EquipSlots.RightArm);
                    rightArmItemTextPanel.Components.Clear();
                    AddItemTextLabels(rightArmItemTextPanel, EquipSlots.RightArm, "Right Arm"); break;
                case EquipSlots.ChestArmor:
                    chestItemIconPanel.Components.Clear();
                    DrawEquipItemToIconPanel(chestItemIconPanel, EquipSlots.ChestArmor);
                    AddItemDurabilityBar(chestItemDurabilityBarPanel, EquipSlots.ChestArmor);
                    chestItemTextPanel.Components.Clear();
                    AddItemTextLabels(chestItemTextPanel, EquipSlots.ChestArmor, "Chest"); break;
                case EquipSlots.Gloves:
                    glovesItemIconPanel.Components.Clear();
                    DrawEquipItemToIconPanel(glovesItemIconPanel, EquipSlots.Gloves);
                    AddItemDurabilityBar(glovesItemDurabilityBarPanel, EquipSlots.Gloves);
                    glovesItemTextPanel.Components.Clear();
                    AddItemTextLabels(glovesItemTextPanel, EquipSlots.Gloves, "Gloves"); break;
                case EquipSlots.LeftArm:
                    leftArmItemIconPanel.Components.Clear();
                    DrawEquipItemToIconPanel(leftArmItemIconPanel, EquipSlots.LeftArm);
                    AddItemDurabilityBar(leftArmItemDurabilityBarPanel, EquipSlots.LeftArm);
                    leftArmItemTextPanel.Components.Clear();
                    AddItemTextLabels(leftArmItemTextPanel, EquipSlots.LeftArm, "Left Arm"); break;
                case EquipSlots.LegsArmor:
                    legsItemIconPanel.Components.Clear();
                    DrawEquipItemToIconPanel(legsItemIconPanel, EquipSlots.LegsArmor);
                    AddItemDurabilityBar(legsItemDurabilityBarPanel, EquipSlots.LegsArmor);
                    legsItemTextPanel.Components.Clear();
                    AddItemTextLabels(legsItemTextPanel, EquipSlots.LegsArmor, "Legs"); break;
                case EquipSlots.Feet:
                    bootsItemIconPanel.Components.Clear();
                    DrawEquipItemToIconPanel(bootsItemIconPanel, EquipSlots.Feet);
                    AddItemDurabilityBar(bootsItemDurabilityBarPanel, EquipSlots.Feet);
                    bootsItemTextPanel.Components.Clear();
                    AddItemTextLabels(bootsItemTextPanel, EquipSlots.Feet, "Feet"); break;
                case EquipSlots.RightHand:
                case EquipSlots.LeftHand:
                    rightHandItemIconPanel.Components.Clear();
                    DrawEquipItemToIconPanel(rightHandItemIconPanel, EquipSlots.RightHand);
                    AddItemDurabilityBar(rightHandItemDurabilityBarPanel, EquipSlots.RightHand);
                    rightHandItemTextPanel.Components.Clear();
                    AddItemTextLabels(rightHandItemTextPanel, EquipSlots.RightHand, "Right Hand");
                    leftHandItemIconPanel.Components.Clear();
                    DrawEquipItemToIconPanel(leftHandItemIconPanel, EquipSlots.LeftHand);
                    AddItemDurabilityBar(leftHandItemDurabilityBarPanel, EquipSlots.LeftHand);
                    leftHandItemTextPanel.Components.Clear();
                    AddItemTextLabels(leftHandItemTextPanel, EquipSlots.LeftHand, "Left Hand"); break;
                default: return;
            }

            UpdateItemInfoPanel(null);
        }

        protected virtual void LocalItemListScroller_OnHover(DaggerfallUnityItem item)
        {
            ItemListScroller_OnHover(item);
            //RaiseOnItemHoverEvent(item, ItemHoverLocation.LocalList);
        }

        protected virtual void ItemListScroller_OnHover(DaggerfallUnityItem item)
        {
            // Update the info panel if used
            if (extraInfoTextPanel != null)
            {
                UpdateItemInfoPanel(item);
            }

            if (rightItemComparisonPanel.Enabled == true)
            {
                if (rightComparisonMainTextPanel != null)
                {
                    UpdateItemComparisonPanel(item, true);
                }
            }

            if (leftItemComparisonPanel.Enabled == true)
            {
                if (leftComparisonMainTextPanel != null)
                {
                    UpdateItemComparisonPanel(item, false);
                }
            }
        }

        private void ExitButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            CloseWindow();
        }

        public EquipSlots DetermineActiveEquipSlot()
        {
            if (headSlotBorderPanel.Enabled == true) { return EquipSlots.Head; }
            if (rightArmSlotBorderPanel.Enabled == true) { return EquipSlots.RightArm; }
            if (chestSlotBorderPanel.Enabled == true) { return EquipSlots.ChestArmor; }
            if (glovesSlotBorderPanel.Enabled == true) { return EquipSlots.Gloves; }
            if (rightHandSlotBorderPanel.Enabled == true) { return EquipSlots.RightHand; }
            if (leftArmSlotBorderPanel.Enabled == true) { return EquipSlots.LeftArm; }
            if (legsSlotBorderPanel.Enabled == true) { return EquipSlots.LegsArmor; }
            if (bootsSlotBorderPanel.Enabled == true) { return EquipSlots.Feet; }
            if (leftHandSlotBorderPanel.Enabled == true) { return EquipSlots.LeftHand; }

            return EquipSlots.None;
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

        public enum ComparisonType
        {
            None = -1,
            ArmorToArmor = 0,
            WeaponToWeapon = 1,
            ArmorToWeapon = 2,
            WeaponToArmor = 3,
            EmptyToArmor = 4,
            EmptyToWeapon = 5,
        }

        public void UpdatePanels()
        {
            //AddItemDurabilityBar(headItemDurabilityBarPanel, EquipSlots.Head, 0);

            //rightItemComparisonPanel.Position = new Vector2(butt1.x, butt1.y);
            //rightItemComparisonPanel.Size = new Vector2(butt1.width, butt1.height);

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
