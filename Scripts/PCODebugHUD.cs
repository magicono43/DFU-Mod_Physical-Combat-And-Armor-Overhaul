using UnityEngine;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Utility;
using System;
using PhysicalCombatOverhaul;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    public class PCODebugHUD : Panel
    {
        public static PCODebugHUD Instance;

        Panel firstCategoryPanel;
        TextLabel firstCatHeaderText;

        Panel secondCategoryPanel;
        TextLabel secondCatHeaderText;

        Panel thirdCategoryPanel;
        TextLabel thirdCatHeaderText;

        TextLabel testTextLabel1 = new TextLabel();
        int numberd = 0;

        #region Constructors

        public PCODebugHUD()
            : base()
        {
            Enabled = true;
            Position = Vector2.zero;
            Size = new Vector2(600, 200);
            BackgroundColor = Color.black;

            SetupDebugPanelOne();
            SetupDebugPanelTwo();
            SetDebugPanelThree();

            PhysicalCombatOverhaulMain.OnPlayerAttackedMonster += UpdateCategoryOneText_OnPlayerAttackedMonster;
        }

        #endregion

        public void SetupDebugPanelOne()
        {
            firstCategoryPanel = DaggerfallUI.AddPanel(new Rect(0, 0, 200, 200), this);
            firstCategoryPanel.BackgroundColor = Color.red;

            firstCatHeaderText = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, new Vector2(0, 0), "Player VS Monster", firstCategoryPanel);
            firstCatHeaderText.TextColor = Color.white;
            firstCatHeaderText.TextScale = 2.3f;
            firstCatHeaderText.HorizontalAlignment = HorizontalAlignment.Center;

            testTextLabel1 = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, new Vector2(0, 42), string.Empty, firstCategoryPanel);
            testTextLabel1.TextColor = Color.white;
            testTextLabel1.TextScale = 2.3f;
        }

        public void SetupDebugPanelTwo()
        {
            secondCategoryPanel = DaggerfallUI.AddPanel(new Rect(200, 0, 200, 200), this);
            secondCategoryPanel.BackgroundColor = Color.blue;

            secondCatHeaderText = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, new Vector2(0, 0), "Monster VS Player", secondCategoryPanel);
            secondCatHeaderText.TextColor = Color.white;
            secondCatHeaderText.TextScale = 2.3f;
            secondCatHeaderText.HorizontalAlignment = HorizontalAlignment.Center;
        }

        public void SetDebugPanelThree()
        {
            thirdCategoryPanel = DaggerfallUI.AddPanel(new Rect(400, 0, 200, 200), this);
            thirdCategoryPanel.BackgroundColor = Color.green;

            thirdCatHeaderText = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, new Vector2(0, 0), "Monster VS Monster", thirdCategoryPanel);
            thirdCatHeaderText.TextColor = Color.white;
            thirdCatHeaderText.TextScale = 2.3f;
            thirdCatHeaderText.HorizontalAlignment = HorizontalAlignment.Center;
        }

        public void UpdateCategoryOneText_OnPlayerAttackedMonster(PhysicalCombatOverhaulMain.CDATA args)
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            base.Update();

            numberd++;
            testTextLabel1.Text = numberd.ToString();
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}