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
        TextLabel pvmCareerText;
        TextLabel pvmBodySizeText;
        TextLabel pvmBodyPartHitText;
        TextLabel pvmWeaponAndArmorText;
        TextLabel pvmAttackTypeEleText;
        TextLabel pvmCritStateText;
        TextLabel pvmInitialDamText;
        TextLabel pvmDamAfterArmorText;
        TextLabel pvmTargetNatArmorTypeText;
        TextLabel pvmDamAfterNatArmorText;

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
            Size = new Vector2(900, 300);
            BackgroundColor = Color.black;

            SetupDebugPanelOne();
            SetupDebugPanelTwo();
            SetDebugPanelThree();

            PhysicalCombatOverhaulMain.OnPlayerAttackedMonster += UpdateCategoryOneText_OnPlayerAttackedMonster;
        }

        #endregion

        public void SetupDebugPanelOne()
        {
            firstCategoryPanel = DaggerfallUI.AddPanel(new Rect(0, 0, 300, 300), this);
            firstCategoryPanel.BackgroundColor = Color.red;

            firstCatHeaderText = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, new Vector2(0, 0), "Player VS Monster", firstCategoryPanel);
            firstCatHeaderText.TextColor = Color.white;
            firstCatHeaderText.TextScale = 2.3f;
            firstCatHeaderText.HorizontalAlignment = HorizontalAlignment.Center;

            pvmCareerText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 70), "Nothing", firstCategoryPanel);
            pvmCareerText.TextColor = Color.white;
            pvmCareerText.TextScale = 3.0f;

            pvmBodySizeText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 90), "Nothing", firstCategoryPanel);
            pvmBodySizeText.TextColor = Color.white;
            pvmBodySizeText.TextScale = 3.0f;

            pvmBodyPartHitText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 110), "Nothing", firstCategoryPanel);
            pvmBodyPartHitText.TextColor = Color.white;
            pvmBodyPartHitText.TextScale = 3.0f;

            pvmWeaponAndArmorText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 130), "Nothing", firstCategoryPanel);
            pvmWeaponAndArmorText.TextColor = Color.white;
            pvmWeaponAndArmorText.TextScale = 3.0f;

            pvmAttackTypeEleText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 150), "Nothing", firstCategoryPanel);
            pvmAttackTypeEleText.TextColor = Color.white;
            pvmAttackTypeEleText.TextScale = 3.0f;

            pvmCritStateText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 170), "Nothing", firstCategoryPanel);
            pvmCritStateText.TextColor = Color.white;
            pvmCritStateText.TextScale = 3.0f;

            pvmInitialDamText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 190), "Nothing", firstCategoryPanel);
            pvmInitialDamText.TextColor = Color.white;
            pvmInitialDamText.TextScale = 3.0f;

            pvmDamAfterArmorText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 210), "Nothing", firstCategoryPanel);
            pvmDamAfterArmorText.TextColor = Color.white;
            pvmDamAfterArmorText.TextScale = 3.0f;

            pvmTargetNatArmorTypeText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 230), "Nothing", firstCategoryPanel);
            pvmTargetNatArmorTypeText.TextColor = Color.white;
            pvmTargetNatArmorTypeText.TextScale = 3.0f;

            pvmDamAfterNatArmorText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 250), "Nothing", firstCategoryPanel);
            pvmDamAfterNatArmorText.TextColor = Color.white;
            pvmDamAfterNatArmorText.TextScale = 3.0f;

            testTextLabel1 = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, new Vector2(0, 30), string.Empty, firstCategoryPanel);
            testTextLabel1.TextColor = Color.white;
            testTextLabel1.TextScale = 2.3f;
        }

        public void SetupDebugPanelTwo()
        {
            secondCategoryPanel = DaggerfallUI.AddPanel(new Rect(300, 0, 300, 300), this);
            secondCategoryPanel.BackgroundColor = Color.blue;

            secondCatHeaderText = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, new Vector2(0, 0), "Monster VS Player", secondCategoryPanel);
            secondCatHeaderText.TextColor = Color.white;
            secondCatHeaderText.TextScale = 2.3f;
            secondCatHeaderText.HorizontalAlignment = HorizontalAlignment.Center;
        }

        public void SetDebugPanelThree()
        {
            thirdCategoryPanel = DaggerfallUI.AddPanel(new Rect(600, 0, 300, 300), this);
            thirdCategoryPanel.BackgroundColor = Color.green;

            thirdCatHeaderText = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, new Vector2(0, 0), "Monster VS Monster", thirdCategoryPanel);
            thirdCatHeaderText.TextColor = Color.white;
            thirdCatHeaderText.TextScale = 2.3f;
            thirdCatHeaderText.HorizontalAlignment = HorizontalAlignment.Center;
        }

        public void UpdateCategoryOneText_OnPlayerAttackedMonster(PhysicalCombatOverhaulMain.CDATA args)
        {
            pvmCareerText.Text = "A Car: " + args.aCareer.ToString() + "  T Car: " + args.tCareer.ToString();
            pvmBodySizeText.Text = "A Size: " + args.aSize.ToString() + "  T Size: " + args.tSize.ToString();
            pvmBodyPartHitText.Text = "Body Part: " + (BodyParts)args.struckBodyPart;
            pvmWeaponAndArmorText.Text = "A Wep: " + args.aWeapon + "  T Armor: " + args.tArmor;
            pvmAttackTypeEleText.Text = "A Type: " + args.attackType + "  A Ele: " + args.attackElement;
            pvmCritStateText.Text = "Crit Hit: " + args.critHit;
            pvmInitialDamText.Text = "Initial Damage: " + args.initialDam;
            pvmDamAfterArmorText.Text = "Dam After Armor: " + args.damAfterArmor;
            pvmTargetNatArmorTypeText.Text = "T Nat Armor Type: " + args.tNatArmType;
            pvmDamAfterNatArmorText.Text = "Final Damage: " + args.damAfterNatArmor; // Continue refining this more tomorrow I suppose?
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