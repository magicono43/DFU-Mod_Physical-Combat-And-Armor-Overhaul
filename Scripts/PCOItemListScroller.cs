using UnityEngine;
using System.Collections.Generic;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.Items;
using System;
using PhysicalCombatOverhaul;

namespace DaggerfallWorkshop.Game.UserInterface
{
    /// <summary>
    /// Item scroller UI panel component composed of scrollbar, scroll buttons and items list.
    /// </summary>
    public class PCOItemListScroller : Panel
    {
        #region UI Rects, Controls, Textures

        Rect[] itemButtonRects = itemButtonRects16;

        Texture2D GreenUpArrowTexture;
        Texture2D GreenDownArrowTexture;
        Texture2D RedUpArrowTexture;
        Texture2D RedDownArrowTexture;

        Button itemListUpButton;
        Button itemListDownButton;
        VerticalScrollBar itemListScrollBar;

        Button[] itemButtons;
        Panel[] itemIconPanels;
        Panel[] durabilityBarPanels;
        Panel[] itemAnimPanels;
        TextLabel[] itemStackLabels;
        TextLabel[] itemMiscLabels;

        #endregion

        #region UI Rects, Textures for enhanced 16 item mode

        static Rect[] itemButtonRects16 = new Rect[]
        {
            new Rect(0, 0, 23, 22),     new Rect(23, 0, 23, 22),
            new Rect(0, 22, 23, 22),    new Rect(23, 22, 23, 22),
            new Rect(0, 44, 23, 22),    new Rect(23, 44, 23, 22),
            new Rect(0, 66, 23, 22),    new Rect(23, 66, 23, 22),
            new Rect(0, 88, 23, 22),    new Rect(23, 88, 23, 22),
            new Rect(0, 110, 23, 22),    new Rect(23, 110, 23, 22),
            new Rect(0, 132, 23, 22),   new Rect(23, 132, 23, 22),
            new Rect(0, 154, 23, 22),   new Rect(23, 154, 23, 22)
        };

        Texture2D[] itemListTextures;

        #endregion

        #region Fields

        int listDisplayUnits = 8;   // Number of item rows displayed in scrolling areas
        int listWidth = 2;          // Number of items on each row
        int listDisplayTotal;       // Total number of items displayed in scrolling areas
        int itemButtonMargin = 1;   // Margin of item buttons
        float textScale = 0.75f;       // Scale of text on item buttons
        bool scroller = true;       // Scroller active or not
        int miscLabelOffsetDist = 0;// Vertical distance to offset the misc label
        int miscLabelOffsetIdx = 0; //Index of column for which to offset the misc label

        float foregroundAnimationDelay = 0.2f;    
        float backgroundAnimationDelay = 0.2f;

        List<DaggerfallUnityItem> items = new List<DaggerfallUnityItem>();

        ToolTip toolTip;
        EquipSlots associatedSlot;
        ItemBackgroundColourHandler backgroundColourHandler;
        ItemBackgroundAnimationHandler backgroundAnimationHandler;
        ItemForegroundAnimationHandler foregroundAnimationHandler;
        ItemLabelTextHandler labelTextHandler;

        #endregion

        #region Delegates, Events, Properties

        public delegate Color ItemBackgroundColourHandler(DaggerfallUnityItem item);

        public delegate Texture2D[] ItemBackgroundAnimationHandler(DaggerfallUnityItem item);
        public delegate Texture2D[] ItemForegroundAnimationHandler(DaggerfallUnityItem item);

        public delegate string ItemLabelTextHandler(DaggerfallUnityItem item);

        public delegate void OnItemClickHandler(DaggerfallUnityItem item);
        public event OnItemClickHandler OnItemClick;

        public delegate void OnItemRightClickHandler(DaggerfallUnityItem item);
        public event OnItemRightClickHandler OnItemRightClick;

        public delegate void OnItemMiddleClickHandler(DaggerfallUnityItem item);
        public event OnItemMiddleClickHandler OnItemMiddleClick;

        public delegate void OnItemHoverHandler(DaggerfallUnityItem item);
        public event OnItemHoverHandler OnItemHover;

        /// <summary>Equipment Slot This Panel Is Related To</summary>
        public EquipSlots AssociatedSlot
        {
            get { return associatedSlot; }
            set { associatedSlot = value; }
        }

        /// <summary>Handler for colour highlighting</summary>
        public ItemBackgroundColourHandler BackgroundColourHandler
        {
            get { return backgroundColourHandler; }
            set { backgroundColourHandler = value; }
        }

        /// <summary>Handler for background animations (can't be colour highlighted)</summary>
        public ItemBackgroundAnimationHandler BackgroundAnimationHandler
        {
            get { return backgroundAnimationHandler; }
            set { backgroundAnimationHandler = value; }
        }
        /// <summary>Delay in seconds between each frame of animation</summary>
        public float BackgroundAnimationDelay
        {
            get { return backgroundAnimationDelay; }
            set { backgroundAnimationDelay = value; }
        }
        /// <summary>Handler for foreground animations (can be colour highlighted)</summary>
        public ItemForegroundAnimationHandler ForegroundAnimationHandler
        {
            get { return foregroundAnimationHandler; }
            set { foregroundAnimationHandler = value; }
        }
        /// <summary>Delay in seconds between each frame of animation</summary>
        public float ForegroundAnimationDelay
        {
            get { return foregroundAnimationDelay; }
            set { foregroundAnimationDelay = value; }
        }
        /// <summary>Handler for misc label text (defaults to top left)</summary>
        public ItemLabelTextHandler LabelTextHandler
        {
            get { return labelTextHandler; }
            set { labelTextHandler = value; }
        }

        public List<DaggerfallUnityItem> Items
        {
            get { return items; }
            set {
                items = value;
                UpdateItemsDisplay(true);
            }
        }

        #endregion

        #region Constructors, Public methods

        public PCOItemListScroller(ToolTip toolTip, EquipSlots slot, bool rightSide = true)
            : base()
        {
            this.toolTip = toolTip;

            AssociatedSlot = slot;

            listDisplayTotal = listDisplayUnits * listWidth;
            TextLabel miscLabelTemplate = new TextLabel()
            {
                Position = Vector2.zero,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                TextScale = textScale
            };

            LoadTextures(rightSide);
            SetupScrollBar(rightSide);
            SetupScrollButtons(rightSide);
            SetupItemsList(true, miscLabelTemplate, rightSide);
        }

        #endregion

        #region Private, Setup methods}

        void SetupScrollBar(bool rightSide)
        {
            if (rightSide)
            {
                itemListScrollBar = new VerticalScrollBar
                {
                    Position = new Vector2(-1, 17),
                    Size = new Vector2(8, 144),
                    DisplayUnits = listDisplayUnits
                };
            }
            else
            {
                itemListScrollBar = new VerticalScrollBar
                {
                    Position = new Vector2(46, 17),
                    Size = new Vector2(8, 144),
                    DisplayUnits = listDisplayUnits
                };
            }

            Components.Add(itemListScrollBar);
            itemListScrollBar.OnScroll += ItemsScrollBar_OnScroll;
        }

        void SetupScrollButtons(bool rightSide)
        {
            if (rightSide)
            {
                itemListUpButton = new Button
                {
                    Position = new Vector2(0, 1),
                    Size = new Vector2(8, 15),
                    BackgroundTexture = RedUpArrowTexture
                };
                itemListDownButton = new Button
                {
                    Position = new Vector2(0, 160),
                    Size = new Vector2(8, 15),
                    BackgroundTexture = RedDownArrowTexture
                };
            }
            else
            {
                itemListUpButton = new Button
                {
                    Position = new Vector2(46, 1),
                    Size = new Vector2(8, 15),
                    BackgroundTexture = RedUpArrowTexture
                };
                itemListDownButton = new Button
                {
                    Position = new Vector2(46, 160),
                    Size = new Vector2(8, 15),
                    BackgroundTexture = RedDownArrowTexture
                };
            }

            Components.Add(itemListUpButton);
            itemListUpButton.OnMouseClick += ItemsUpButton_OnMouseClick;
            Components.Add(itemListDownButton);
            itemListDownButton.OnMouseClick += ItemsDownButton_OnMouseClick;
        }

        void SetupItemsList(bool enhanced, TextLabel miscLabelTemplate, bool rightSide)
        {
            Rect itemListPanelRect;
            if (rightSide) { itemListPanelRect = new Rect(8, 0, 46, 176); }
            else { itemListPanelRect = new Rect(0, 0, 46, 176); }

            // List panel for scrolling behaviour
            Panel itemsListPanel = DaggerfallUI.AddPanel(itemListPanelRect, this);
            itemsListPanel.OnMouseScrollUp += ItemsListPanel_OnMouseScrollUp;
            itemsListPanel.OnMouseScrollDown += ItemsListPanel_OnMouseScrollDown;
            itemsListPanel.OnMouseLeave += ItemsListPanel_OnMouseLeave;

            // Setup buttons
            itemButtons = new Button[listDisplayTotal];
            itemIconPanels = new Panel[listDisplayTotal];
            durabilityBarPanels = new Panel[listDisplayTotal];
            itemAnimPanels = new Panel[listDisplayTotal];
            itemStackLabels = new TextLabel[listDisplayTotal];
            itemMiscLabels = new TextLabel[listDisplayTotal];

            // Setup column misc label offsetting.
            Vector2 offsetPosition = miscLabelTemplate.Position + new Vector2(0, miscLabelOffsetDist);
            int osi = miscLabelOffsetIdx;

            for (int i = 0; i < listDisplayTotal; i++)
            {
                // Buttons (also handle highlight colours)
                itemButtons[i] = DaggerfallUI.AddButton(itemButtonRects[i], itemsListPanel);
                itemButtons[i].SetMargins(Margins.All, itemButtonMargin);
                itemButtons[i].ToolTip = toolTip;
                itemButtons[i].Tag = i;
                itemButtons[i].OnMouseClick += ItemButton_OnMouseClick;
                itemButtons[i].OnRightMouseClick += ItemButton_OnRightMouseClick;
                itemButtons[i].OnMiddleMouseClick += ItemButton_OnMiddleMouseClick;
                itemButtons[i].OnMouseEnter += ItemButton_OnMouseEnter;
                itemButtons[i].OnMouseScrollUp += ItemButton_OnMouseEnter;
                itemButtons[i].OnMouseScrollDown += ItemButton_OnMouseEnter;

                // Item foreground animation panel
                itemAnimPanels[i] = DaggerfallUI.AddPanel(itemButtonRects[i], itemsListPanel);
                itemAnimPanels[i].AnimationDelayInSeconds = foregroundAnimationDelay;

                // Icon image panel
                itemIconPanels[i] = DaggerfallUI.AddPanel(itemButtons[i], AutoSizeModes.ScaleToFit);
                itemIconPanels[i].HorizontalAlignment = HorizontalAlignment.Center;
                itemIconPanels[i].VerticalAlignment = VerticalAlignment.Middle;
                itemIconPanels[i].MaxAutoScale = 1f;

                // Stack labels
                itemStackLabels[i] = DaggerfallUI.AddTextLabel(DaggerfallUI.Instance.Font4, Vector2.zero, string.Empty, itemButtons[i]);
                itemStackLabels[i].HorizontalAlignment = HorizontalAlignment.Right;
                itemStackLabels[i].VerticalAlignment = VerticalAlignment.Bottom;
                itemStackLabels[i].ShadowPosition = Vector2.zero;
                itemStackLabels[i].TextScale = textScale;
                itemStackLabels[i].TextColor = DaggerfallUI.DaggerfallUnityDefaultToolTipTextColor;

                durabilityBarPanels[i] = DaggerfallUI.AddPanel(new Rect(1, 18, 19, 1), itemButtons[i]);

                // Misc labels
                Vector2 position = miscLabelTemplate.Position;
                if (miscLabelOffsetDist != 0 && i == osi)
                {
                    position = offsetPosition;
                    osi += listWidth;
                }
                itemMiscLabels[i] = DaggerfallUI.AddTextLabel(miscLabelTemplate.Font, position, string.Empty, itemButtons[i]);
                itemMiscLabels[i].HorizontalAlignment = miscLabelTemplate.HorizontalAlignment;
                itemMiscLabels[i].VerticalAlignment = miscLabelTemplate.VerticalAlignment;
                itemMiscLabels[i].TextScale = miscLabelTemplate.TextScale;
            }
        }

        public void AddItemMiniDurabilityBar(Panel itemDurPanel, DaggerfallUnityItem item)
        {
            if (item != null)
            {
                int maxBarWidth = 19;
                float curDur = item.currentCondition;
                float maxDur = item.maxCondition;

                float barWidth = Mathf.Floor((curDur / maxDur) * maxBarWidth);
                float offset = (maxBarWidth - barWidth) / 2;
                float condPerc = (curDur / maxDur) * 100;

                byte colorAlpha = 220;
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

                //string toolTipText = string.Format(itemName + "\r" + condName + "\r{0}%        {1} / {2}", Mathf.CeilToInt(condPerc), curDur, maxDur);

                itemDurPanel.Components.Clear();
                Panel itemDurBar = DaggerfallUI.AddPanel(new Rect(offset, 0, barWidth, 1), itemDurPanel);
                itemDurBar.BackgroundColor = barColor;
                itemDurBar.VerticalAlignment = VerticalAlignment.Middle;
                //Panel itemDurBarToolTipPanel = DaggerfallUI.AddPanel(new Rect(0, 0, maxBarWidth, 2), itemDurBar);
                //itemDurBarToolTipPanel.ToolTip = defaultToolTip;
                //itemDurBarToolTipPanel.ToolTipText = toolTipText;
            }
            else
            {
                itemDurPanel.Components.Clear();
            }
        }

        void ClearItemsList()
        {
            for (int i = 0; i < listDisplayTotal; i++)
            {
                itemStackLabels[i].Text = string.Empty;
                itemMiscLabels[i].Text = string.Empty;
                itemButtons[i].ToolTipText = string.Empty;
                itemIconPanels[i].BackgroundTexture = null;
                durabilityBarPanels[i].Components.Clear();
                itemButtons[i].BackgroundColor = Color.clear;
                itemButtons[i].AnimatedBackgroundTextures = null;
                itemIconPanels[i].AnimatedBackgroundTextures = null;
                itemAnimPanels[i].AnimatedBackgroundTextures = null;
            }
            if (scroller)
            {
                itemListUpButton.BackgroundTexture = RedUpArrowTexture;
                itemListDownButton.BackgroundTexture = RedDownArrowTexture;
            }
        }

        void UpdateItemsDisplay(bool delayScrollUp)
        {
            // Clear list elements
            ClearItemsList();
            if (items == null)
                return;

            int scrollIndex = 0;
            if (scroller)
            {
                // Update scrollbar
                int rows = (items.Count + listWidth - 1) / listWidth;
                itemListScrollBar.TotalUnits = rows;
                scrollIndex = GetSafeScrollIndex(itemListScrollBar, delayScrollUp);

                // Update scroller buttons
                UpdateListScrollerButtons(scrollIndex, rows);

                // Convert scroller index to item based scroll index
                scrollIndex *= listWidth;
            }
            // Update images and tooltips
            for (int i = 0; i < listDisplayTotal; i++)
            {
                // Skip if out of bounds
                if (scrollIndex + i >= items.Count)
                    continue;

                // Get item and image
                DaggerfallUnityItem item = items[scrollIndex + i];
                ImageData image = DaggerfallUnity.Instance.ItemHelper.GetInventoryImage(item);

                // Set animated image frames to button icon (if any)
                if (image.animatedTextures != null && image.animatedTextures.Length > 0)
                    itemIconPanels[i].AnimatedBackgroundTextures = image.animatedTextures;

                // Set image to button icon
                itemIconPanels[i].BackgroundTexture = image.texture;
                // Use texture size if base image size is zero (i.e. new images that are not present in classic data)
                if (image.width != 0 && image.height != 0)
                    itemIconPanels[i].Size = new Vector2(image.width, image.height);
                else
                    itemIconPanels[i].Size = new Vector2(image.texture.width, image.texture.height);

                // Set stack count
                if (item.stackCount > 1)
                    itemStackLabels[i].Text = item.stackCount.ToString();

                // Add mini durability bars
                AddItemMiniDurabilityBar(durabilityBarPanels[i], item);

                // Handle context specific background colour, animations & label
                if (backgroundColourHandler != null)
                    itemButtons[i].BackgroundColor = backgroundColourHandler(item);
                if (backgroundAnimationHandler != null) {
                    itemButtons[i].AnimationDelayInSeconds = backgroundAnimationDelay;
                    itemButtons[i].AnimatedBackgroundTextures = backgroundAnimationHandler(item);
                }
                if (foregroundAnimationHandler != null) {
                    itemAnimPanels[i].AnimationDelayInSeconds = foregroundAnimationDelay;
                    itemAnimPanels[i].AnimatedBackgroundTextures = foregroundAnimationHandler(item);
                }
                if (labelTextHandler != null)
                    itemMiscLabels[i].Text = labelTextHandler(item);

                // Tooltip text
                itemButtons[i].ToolTipText =
                    (item.ItemGroup == ItemGroups.Books && !item.IsArtifact) ? DaggerfallUnity.Instance.ItemHelper.GetBookTitle(item.message, item.LongName) : item.LongName;
            }
        }

        int GetSafeScrollIndex(VerticalScrollBar scroller, bool delayScrollUp)
        {
            // Get current scroller index
            int scrollIndex = scroller.ScrollIndex;
            if (scrollIndex < 0)
                scrollIndex = 0;

            // Ensure scroll index within current range
            if (!delayScrollUp && scrollIndex + scroller.DisplayUnits > scroller.TotalUnits || scrollIndex >= scroller.TotalUnits)
            {
                scrollIndex = scroller.TotalUnits - scroller.DisplayUnits;
                if (scrollIndex < 0) scrollIndex = 0;
                scroller.Reset(scroller.DisplayUnits, scroller.TotalUnits, scrollIndex);
            }
            return scrollIndex;
        }

        // Updates red/green state of scroller buttons
        void UpdateListScrollerButtons(int index, int count)
        {
            // Update up button
            itemListUpButton.BackgroundTexture = (index > 0) ? GreenUpArrowTexture : RedUpArrowTexture;

            // Update down button
            itemListDownButton.BackgroundTexture = (index < (count - listDisplayUnits)) ? GreenDownArrowTexture : RedDownArrowTexture;

            // No items above or below
            if (count <= listDisplayUnits)
            {
                itemListUpButton.BackgroundTexture = RedUpArrowTexture;
                itemListDownButton.BackgroundTexture = RedDownArrowTexture;
            }
        }

        private void LoadTextures(bool rightSide)
        {
            if (rightSide)
            {
                GreenUpArrowTexture = PhysicalCombatOverhaulMain.Instance.EquipInfoExtraRightGreenUpArrowTexture;
                GreenDownArrowTexture = PhysicalCombatOverhaulMain.Instance.EquipInfoExtraRightGreenDownArrowTexture;
                RedUpArrowTexture = PhysicalCombatOverhaulMain.Instance.EquipInfoExtraRightRedUpArrowTexture;
                RedDownArrowTexture = PhysicalCombatOverhaulMain.Instance.EquipInfoExtraRightRedDownArrowTexture;
            }
            else
            {
                GreenUpArrowTexture = PhysicalCombatOverhaulMain.Instance.EquipInfoExtraLeftGreenUpArrowTexture;
                GreenDownArrowTexture = PhysicalCombatOverhaulMain.Instance.EquipInfoExtraLeftGreenDownArrowTexture;
                RedUpArrowTexture = PhysicalCombatOverhaulMain.Instance.EquipInfoExtraLeftRedUpArrowTexture;
                RedDownArrowTexture = PhysicalCombatOverhaulMain.Instance.EquipInfoExtraLeftRedDownArrowTexture;
            }
        }

        private int GetScrollIndex()
        {
            return (scroller) ? itemListScrollBar.ScrollIndex : 0;
        }

        #endregion

        #region Event handlers

        void ItemButton_OnClick(BaseScreenComponent sender, Vector2 position, bool rightClick, bool middleClick = false)
        {
            // Get index
            int index = (GetScrollIndex() * listWidth) + (int)sender.Tag;
            if (index >= items.Count)
                return;

            // Get item and raise item click event
            DaggerfallUnityItem item = items[index];

            if (middleClick && item != null && OnItemMiddleClick != null)
                OnItemMiddleClick(item);
            else if (rightClick && item != null && OnItemRightClick != null)
                OnItemRightClick(item);
            else if (item != null && OnItemClick != null)
                OnItemClick(item);

            ItemButton_OnMouseEnter(sender);
        }

        void ItemButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            ItemButton_OnClick(sender, position, false);
        }

        void ItemButton_OnRightMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            ItemButton_OnClick(sender, position, true);
        }

        void ItemButton_OnMiddleMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            ItemButton_OnClick(sender, position, false, true);
        }

        void ItemButton_OnMouseEnter(BaseScreenComponent sender)
        {
            // Get index
            int index = (GetScrollIndex() * listWidth) + (int)sender.Tag;
            if (index >= items.Count)
                return;

            // Get item and raise item hover event
            DaggerfallUnityItem item = items[index];
            if (item != null && OnItemHover != null)
                OnItemHover(item);
        }

        void ItemsScrollBar_OnScroll()
        {
            UpdateItemsDisplay(false);
        }

        void ItemsUpButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            if (scroller)
            {
                itemListScrollBar.ScrollIndex--;
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            }
        }

        void ItemsDownButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            if (scroller)
            {
                itemListScrollBar.ScrollIndex++;
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            }
        }

        void ItemsListPanel_OnMouseScrollUp(BaseScreenComponent sender)
        {
            if (scroller)
                itemListScrollBar.ScrollIndex--;
        }

        void ItemsListPanel_OnMouseScrollDown(BaseScreenComponent sender)
        {
            if (scroller)
                itemListScrollBar.ScrollIndex++;
        }

        void ItemsListPanel_OnMouseLeave(BaseScreenComponent sender)
        {
            UpdateItemsDisplay(false);
            OnItemHover(null);
        }

        #endregion
    }
}