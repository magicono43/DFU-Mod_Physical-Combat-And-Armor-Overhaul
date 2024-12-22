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
        TextLabel pvmWeaponText;
        TextLabel pvmAttackTypeEleText;
        TextLabel pvmCritStateText;
        TextLabel pvmInitialDamText;
        TextLabel pvmArmorText;
        TextLabel pvmDamAfterArmorText;
        TextLabel pvmTargetNatArmorTypeText;
        TextLabel pvmDamAfterNatArmorText;
        TextLabel pvmStartEndHPText;

        Panel secondCategoryPanel;
        TextLabel secondCatHeaderText;
        TextLabel mvpCareerText;
        TextLabel mvpBodySizeText;
        TextLabel mvpBodyPartHitText;
        TextLabel mvpWeaponText;
        TextLabel mvpAttackTypeEleText;
        TextLabel mvpCritStateText;
        TextLabel mvpInitialDamText;
        TextLabel mvpArmorText;
        TextLabel mvpDamAfterArmorText;
        TextLabel mvpTargetNatArmorTypeText;
        TextLabel mvpDamAfterNatArmorText;
        TextLabel mvpStartEndHPText;

        Panel thirdCategoryPanel;
        TextLabel thirdCatHeaderText;
        TextLabel mvmCareerText;
        TextLabel mvmBodySizeText;
        TextLabel mvmBodyPartHitText;
        TextLabel mvmWeaponText;
        TextLabel mvmAttackTypeEleText;
        TextLabel mvmCritStateText;
        TextLabel mvmInitialDamText;
        TextLabel mvmArmorText;
        TextLabel mvmDamAfterArmorText;
        TextLabel mvmTargetNatArmorTypeText;
        TextLabel mvmDamAfterNatArmorText;
        TextLabel mvmStartEndHPText;

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
            PhysicalCombatOverhaulMain.OnMonsterAttackedPlayer += UpdateCategoryTwoText_OnMonsterAttackedPlayer;
            PhysicalCombatOverhaulMain.OnMonsterAttackedMonster += UpdateCategoryThreeText_OnMonsterAttackedMonster;
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

            pvmCareerText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 60), "Nothing", firstCategoryPanel);
            pvmCareerText.TextColor = Color.white;
            pvmCareerText.TextScale = 3.0f;

            pvmBodySizeText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 80), "Nothing", firstCategoryPanel);
            pvmBodySizeText.TextColor = Color.white;
            pvmBodySizeText.TextScale = 3.0f;

            pvmBodyPartHitText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 100), "Nothing", firstCategoryPanel);
            pvmBodyPartHitText.TextColor = Color.white;
            pvmBodyPartHitText.TextScale = 3.0f;

            pvmWeaponText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 120), "Nothing", firstCategoryPanel);
            pvmWeaponText.TextColor = Color.white;
            pvmWeaponText.TextScale = 3.0f;

            pvmAttackTypeEleText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 140), "Nothing", firstCategoryPanel);
            pvmAttackTypeEleText.TextColor = Color.white;
            pvmAttackTypeEleText.TextScale = 3.0f;

            pvmCritStateText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 160), "Nothing", firstCategoryPanel);
            pvmCritStateText.TextColor = Color.white;
            pvmCritStateText.TextScale = 3.0f;

            pvmInitialDamText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 180), "Nothing", firstCategoryPanel);
            pvmInitialDamText.TextColor = Color.white;
            pvmInitialDamText.TextScale = 3.0f;

            pvmArmorText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 200), "Nothing", firstCategoryPanel);
            pvmArmorText.TextColor = Color.white;
            pvmArmorText.TextScale = 3.0f;

            pvmDamAfterArmorText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 220), "Nothing", firstCategoryPanel);
            pvmDamAfterArmorText.TextColor = Color.white;
            pvmDamAfterArmorText.TextScale = 3.0f;

            pvmTargetNatArmorTypeText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 240), "Nothing", firstCategoryPanel);
            pvmTargetNatArmorTypeText.TextColor = Color.white;
            pvmTargetNatArmorTypeText.TextScale = 3.0f;

            pvmDamAfterNatArmorText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 260), "Nothing", firstCategoryPanel);
            pvmDamAfterNatArmorText.TextColor = Color.white;
            pvmDamAfterNatArmorText.TextScale = 3.0f;

            pvmStartEndHPText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 280), "Nothing", firstCategoryPanel);
            pvmStartEndHPText.TextColor = Color.white;
            pvmStartEndHPText.TextScale = 3.0f;

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

            mvpCareerText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 60), "Nothing", secondCategoryPanel);
            mvpCareerText.TextColor = Color.white;
            mvpCareerText.TextScale = 3.0f;

            mvpBodySizeText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 80), "Nothing", secondCategoryPanel);
            mvpBodySizeText.TextColor = Color.white;
            mvpBodySizeText.TextScale = 3.0f;

            mvpBodyPartHitText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 100), "Nothing", secondCategoryPanel);
            mvpBodyPartHitText.TextColor = Color.white;
            mvpBodyPartHitText.TextScale = 3.0f;

            mvpWeaponText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 120), "Nothing", secondCategoryPanel);
            mvpWeaponText.TextColor = Color.white;
            mvpWeaponText.TextScale = 3.0f;

            mvpAttackTypeEleText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 140), "Nothing", secondCategoryPanel);
            mvpAttackTypeEleText.TextColor = Color.white;
            mvpAttackTypeEleText.TextScale = 3.0f;

            mvpCritStateText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 160), "Nothing", secondCategoryPanel);
            mvpCritStateText.TextColor = Color.white;
            mvpCritStateText.TextScale = 3.0f;

            mvpInitialDamText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 180), "Nothing", secondCategoryPanel);
            mvpInitialDamText.TextColor = Color.white;
            mvpInitialDamText.TextScale = 3.0f;

            mvpArmorText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 200), "Nothing", secondCategoryPanel);
            mvpArmorText.TextColor = Color.white;
            mvpArmorText.TextScale = 3.0f;

            mvpDamAfterArmorText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 220), "Nothing", secondCategoryPanel);
            mvpDamAfterArmorText.TextColor = Color.white;
            mvpDamAfterArmorText.TextScale = 3.0f;

            mvpTargetNatArmorTypeText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 240), "Nothing", secondCategoryPanel);
            mvpTargetNatArmorTypeText.TextColor = Color.white;
            mvpTargetNatArmorTypeText.TextScale = 3.0f;

            mvpDamAfterNatArmorText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 260), "Nothing", secondCategoryPanel);
            mvpDamAfterNatArmorText.TextColor = Color.white;
            mvpDamAfterNatArmorText.TextScale = 3.0f;

            mvpStartEndHPText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 280), "Nothing", secondCategoryPanel);
            mvpStartEndHPText.TextColor = Color.white;
            mvpStartEndHPText.TextScale = 3.0f;
        }

        public void SetDebugPanelThree()
        {
            thirdCategoryPanel = DaggerfallUI.AddPanel(new Rect(600, 0, 300, 300), this);
            thirdCategoryPanel.BackgroundColor = Color.green;

            thirdCatHeaderText = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, new Vector2(0, 0), "Monster VS Monster", thirdCategoryPanel);
            thirdCatHeaderText.TextColor = Color.white;
            thirdCatHeaderText.TextScale = 2.3f;
            thirdCatHeaderText.HorizontalAlignment = HorizontalAlignment.Center;

            mvmCareerText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 60), "Nothing", thirdCategoryPanel);
            mvmCareerText.TextColor = Color.white;
            mvmCareerText.TextScale = 3.0f;

            mvmBodySizeText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 80), "Nothing", thirdCategoryPanel);
            mvmBodySizeText.TextColor = Color.white;
            mvmBodySizeText.TextScale = 3.0f;

            mvmBodyPartHitText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 100), "Nothing", thirdCategoryPanel);
            mvmBodyPartHitText.TextColor = Color.white;
            mvmBodyPartHitText.TextScale = 3.0f;

            mvmWeaponText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 120), "Nothing", thirdCategoryPanel);
            mvmWeaponText.TextColor = Color.white;
            mvmWeaponText.TextScale = 3.0f;

            mvmAttackTypeEleText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 140), "Nothing", thirdCategoryPanel);
            mvmAttackTypeEleText.TextColor = Color.white;
            mvmAttackTypeEleText.TextScale = 3.0f;

            mvmCritStateText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 160), "Nothing", thirdCategoryPanel);
            mvmCritStateText.TextColor = Color.white;
            mvmCritStateText.TextScale = 3.0f;

            mvmInitialDamText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 180), "Nothing", thirdCategoryPanel);
            mvmInitialDamText.TextColor = Color.white;
            mvmInitialDamText.TextScale = 3.0f;

            mvmArmorText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 200), "Nothing", thirdCategoryPanel);
            mvmArmorText.TextColor = Color.white;
            mvmArmorText.TextScale = 3.0f;

            mvmDamAfterArmorText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 220), "Nothing", thirdCategoryPanel);
            mvmDamAfterArmorText.TextColor = Color.white;
            mvmDamAfterArmorText.TextScale = 3.0f;

            mvmTargetNatArmorTypeText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 240), "Nothing", thirdCategoryPanel);
            mvmTargetNatArmorTypeText.TextColor = Color.white;
            mvmTargetNatArmorTypeText.TextScale = 3.0f;

            mvmDamAfterNatArmorText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 260), "Nothing", thirdCategoryPanel);
            mvmDamAfterNatArmorText.TextColor = Color.white;
            mvmDamAfterNatArmorText.TextScale = 3.0f;

            mvmStartEndHPText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 280), "Nothing", thirdCategoryPanel);
            mvmStartEndHPText.TextColor = Color.white;
            mvmStartEndHPText.TextScale = 3.0f;
        }

        public void UpdateCategoryOneText_OnPlayerAttackedMonster(PhysicalCombatOverhaulMain.CDATA args)
        {
            pvmCareerText.Text = "Career: " + args.tCareer.ToString();
            pvmBodySizeText.Text = "Size: " + args.tSize.ToString();
            pvmBodyPartHitText.Text = "Body Part: " + (BodyParts)args.struckBodyPart;
            pvmWeaponText.Text = "Weapon Used: " + args.aWeapon;
            pvmAttackTypeEleText.Text = "A Type: " + args.attackType + "  A Ele: " + args.attackElement;
            pvmCritStateText.Text = "Crit Hit: " + args.critHit;
            pvmInitialDamText.Text = "Initial Damage: " + args.initialDam;
            pvmArmorText.Text = "Armor Worn: " + args.tArmor;
            pvmDamAfterArmorText.Text = "Dam After Armor: " + args.damAfterArmor;
            pvmTargetNatArmorTypeText.Text = "Nat Armor Type: " + args.tNatArmType;
            pvmDamAfterNatArmorText.Text = "Final Damage: " + args.damAfterNatArmor;
            pvmStartEndHPText.Text = "Start HP: " + args.tStartHP + "  End HP: " + (args.tStartHP - args.damAfterNatArmor);
        }

        public void UpdateCategoryTwoText_OnMonsterAttackedPlayer(PhysicalCombatOverhaulMain.CDATA args)
        {
            mvpCareerText.Text = "Career: " + args.aCareer.ToString();
            mvpBodySizeText.Text = "Size: " + args.aSize.ToString();
            mvpBodyPartHitText.Text = "Body Part: " + (BodyParts)args.struckBodyPart;
            mvpWeaponText.Text = "Weapon Used: " + args.aWeapon;
            mvpAttackTypeEleText.Text = "A Type: " + args.attackType + "  A Ele: " + args.attackElement;
            mvpCritStateText.Text = "Crit Hit: " + args.critHit;
            mvpInitialDamText.Text = "Initial Damage: " + args.initialDam;
            mvpArmorText.Text = "Armor Worn: " + args.tArmor;
            mvpDamAfterArmorText.Text = "Dam After Armor: " + args.damAfterArmor;
            mvpTargetNatArmorTypeText.Text = "Nat Armor Type: " + args.tNatArmType;
            mvpDamAfterNatArmorText.Text = "Final Damage: " + args.damAfterNatArmor;
            mvpStartEndHPText.Text = "Start HP: " + args.tStartHP + "  End HP: " + (args.tStartHP - args.damAfterNatArmor);
        }

        public void UpdateCategoryThreeText_OnMonsterAttackedMonster(PhysicalCombatOverhaulMain.CDATA args)
        {
            mvmCareerText.Text = "Career: " + args.aCareer.ToString();
            mvmBodySizeText.Text = "Size: " + args.aSize.ToString();
            mvmBodyPartHitText.Text = "Body Part: " + (BodyParts)args.struckBodyPart;
            mvmWeaponText.Text = "Weapon Used: " + args.aWeapon;
            mvmAttackTypeEleText.Text = "A Type: " + args.attackType + "  A Ele: " + args.attackElement;
            mvmCritStateText.Text = "Crit Hit: " + args.critHit;
            mvmInitialDamText.Text = "Initial Damage: " + args.initialDam;
            mvmArmorText.Text = "Armor Worn: " + args.tArmor;
            mvmDamAfterArmorText.Text = "Dam After Armor: " + args.damAfterArmor;
            mvmTargetNatArmorTypeText.Text = "Nat Armor Type: " + args.tNatArmType;
            mvmDamAfterNatArmorText.Text = "Final Damage: " + args.damAfterNatArmor;
            mvmStartEndHPText.Text = "Start HP: " + args.tStartHP + "  End HP: " + (args.tStartHP - args.damAfterNatArmor);
            // Maybe tomorrow, try to pretty all this debug stuff up a bit, so it can better be used, maybe add some commands to toggle it on and off and such, will see.
            // After that I'm not certain what to work on next, I'll just have to think a bit and see what the next thing to work on will be I guess.
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