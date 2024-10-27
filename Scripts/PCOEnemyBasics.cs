using System;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.FallExe;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using UnityEngine;

namespace PhysicalCombatOverhaul
{
    /// <summary>
    /// Defines basic properties of mobile enemies.
    /// </summary>
    public struct Monster
    {
        public int ID;
        public BodySize Size;
        public NaturalArmorType ArmorType;
        public AttackType Attack;
        public AttackElementType AttackElement;
        public int AttackOdds;
        public AttackType Attack2;
        public AttackElementType AttackElement2;
        public int AttackOdds2;
        public AttackType Attack3;
        public AttackElementType AttackElement3;
        public int AttackOdds3;
        public AttackType Attack4;
        public AttackElementType AttackElement4;
        public int AttackOdds4;
        public DummyDFUItem MonsterWeapon;
        public float NaturalDT;
        public float BluntDR;
        public float SlashDR;
        public float PierceDR;
    }

    /// <summary>
    /// What type of attack the creature is using.
    /// </summary>
    public enum AttackType
    {
        Bash = 0,
        Bludgeon = 1,
        Slash = 2,
        Stab = 3,
        Bite = 4,
        Claw = 5,
        Maul = 6,
        Kick = 7,
        Scratch = 8,
        Pinch = 9,
        Sting = 10,
        Ethereal = 11,
        Elemental_Touch = 12,
        Elemental_Breath = 13,
        Elemental_Bludgeon = 14,
        Elemental_Slash = 15,
        Elemental_Stab = 16,
    }

    /// <summary>
    /// What element this attack uses.
    /// </summary>
    public enum AttackElementType
    {
        None = -1,
        Magic = 0,
        Acid = 1,
        Fire = 2,
        Ice = 3,
        Lightning = 4,
        Draining = 5,
    }

    /// <summary>
    /// What type of natural armor the creature is made of.
    /// </summary>
    public enum NaturalArmorType
    {
        Flesh = 0,
        Fur = 1,
        Scale = 2,
        Bone = 3,
        Rock = 4,
        Metal = 5,
        Ethereal = 6,
    }

    /// <summary>
    /// Size categories for any creatures, used in some combat formula.
    /// </summary>
    public enum BodySize
    {
        None = -1,
        Tiny = 0,
        Small = 1,
        Average = 2,
        Large = 3,
        Huge = 4,
    }

    /// <summary>Class to hopefully keep from spamming new UID values from enemies being assigned 'fake' weapons, will see if it works or not.</summary>
    public class DummyDFUItem
    {
        public ItemGroups itemGroup { get; set; }
        public int groupIndex { get; set; }
        public int nativeMaterialValue { get; set; }
        public int templateIndex { get; set; }
        public float weightInKg { get; set; }

        // Default constructor
        public DummyDFUItem()
        {
            // Default values
            itemGroup = ItemGroups.None;
            groupIndex = 0;
            nativeMaterialValue = -1;
            templateIndex = -1;
            weightInKg = 0.0f;
        }

        /// <summary>Generates a dummy weapon.</summary>
        public static DummyDFUItem CreateDummyWeapon(Weapons weapon, WeaponMaterialTypes material) // Later may consider just assigning all these values manually for "dummy" weapons. 
        {
            // Create dummy item
            DummyDFUItem newItem = new DummyDFUItem();
            newItem.itemGroup = ItemGroups.Weapons;
            newItem.groupIndex = DaggerfallUnity.Instance.ItemHelper.GetGroupIndex(ItemGroups.Weapons, (int)weapon);
            newItem.nativeMaterialValue = (int)material;

            ItemTemplate itemTemplate = DaggerfallUnity.Instance.ItemHelper.GetItemTemplate(newItem.itemGroup, newItem.groupIndex);

            newItem.templateIndex = itemTemplate.index;

            int quarterKgs = (int)(itemTemplate.baseWeight * 4);
            float matQuarterKgs = (float)(quarterKgs * PhysicalCombatOverhaulMain.weightMultipliersByMaterial[(int)material]) / 4;
            newItem.weightInKg = Mathf.Round(matQuarterKgs) / 4;

            return newItem;
        }

        public short GetWeaponSkillIDAsShort()
        {
            int skill = GetWeaponSkillUsed();
            switch (skill)
            {
                case (int)DFCareer.ProficiencyFlags.ShortBlades:
                    return (int)Skills.ShortBlade;
                case (int)DFCareer.ProficiencyFlags.LongBlades:
                    return (int)Skills.LongBlade;
                case (int)DFCareer.ProficiencyFlags.Axes:
                    return (int)Skills.Axe;
                case (int)DFCareer.ProficiencyFlags.BluntWeapons:
                    return (int)Skills.BluntWeapon;
                case (int)DFCareer.ProficiencyFlags.MissileWeapons:
                    return (int)Skills.Archery;

                default:
                    return (int)Skills.None;
            }
        }

        public int GetWeaponSkillUsed()
        {
            switch (templateIndex)
            {
                case (int)Weapons.Dagger:
                case (int)Weapons.Tanto:
                case (int)Weapons.Wakazashi:
                case (int)Weapons.Shortsword:
                    return (int)DFCareer.ProficiencyFlags.ShortBlades;
                case (int)Weapons.Broadsword:
                case (int)Weapons.Longsword:
                case (int)Weapons.Saber:
                case (int)Weapons.Katana:
                case (int)Weapons.Claymore:
                case (int)Weapons.Dai_Katana:
                    return (int)DFCareer.ProficiencyFlags.LongBlades;
                case (int)Weapons.Battle_Axe:
                case (int)Weapons.War_Axe:
                    return (int)DFCareer.ProficiencyFlags.Axes;
                case (int)Weapons.Flail:
                case (int)Weapons.Mace:
                case (int)Weapons.Warhammer:
                case (int)Weapons.Staff:
                    return (int)DFCareer.ProficiencyFlags.BluntWeapons;
                case (int)Weapons.Short_Bow:
                case (int)Weapons.Long_Bow:
                    return (int)DFCareer.ProficiencyFlags.MissileWeapons;

                default:
                    return (int)Skills.None;
            }
        }
    }

    /// <summary>
    /// Static definitions for enemies and their various properties.
    /// </summary>
    public static class PCOEnemyBasics
    {
        #region Enemy Definitions

        public static Monster[] Monsters = new Monster[]
        {
            // Rat
            new Monster()
            {
                ID = 0,
                Size = BodySize.Small,
                ArmorType = NaturalArmorType.Fur,
                Attack = AttackType.Bite,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
            },

            // Imp
            new Monster()
            {
                ID = 1,
                Size = BodySize.Tiny,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Elemental_Touch,
                AttackElement = AttackElementType.Magic,
                AttackOdds = 75,
                Attack2 = AttackType.Elemental_Touch,
                AttackElement2 = AttackElementType.Acid,
                AttackOdds2 = 25,
            },

            // Spriggan
            new Monster()
            {
                ID = 2,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Bone, // Might make a "wood" natural armor type, but will see.
                Attack = AttackType.Bash,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
                NaturalDT = 3.25f,
                BluntDR = 0.8f,
                SlashDR = 1.75f,
                PierceDR = 0.6f,
            },

            // Giant Bat
            new Monster()
            {
                ID = 3,
                Size = BodySize.Small,
                ArmorType = NaturalArmorType.Fur,
                Attack = AttackType.Bite,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
            },

            // Grizzly Bear
            new Monster()
            {
                ID = 4,
                Size = BodySize.Large,
                ArmorType = NaturalArmorType.Fur,
                Attack = AttackType.Claw,
                AttackElement = AttackElementType.None,
                AttackOdds = 75,
                Attack2 = AttackType.Maul,
                AttackElement2 = AttackElementType.None,
                AttackOdds2 = 25,
                NaturalDT = 2f,
                BluntDR = 0.8f,
            },

            // Sabertooth Tiger
            new Monster()
            {
                ID = 5,
                Size = BodySize.Large,
                ArmorType = NaturalArmorType.Fur,
                Attack = AttackType.Bite,
                AttackElement = AttackElementType.None,
                AttackOdds = 50,
                Attack2 = AttackType.Claw,
                AttackElement2 = AttackElementType.None,
                AttackOdds2 = 50,
                NaturalDT = 1f,
                BluntDR = 0.8f,
            },

            // Spider
            new Monster()
            {
                ID = 6,
                Size = BodySize.Small,
                ArmorType = NaturalArmorType.Scale,
                Attack = AttackType.Bite,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
                NaturalDT = 1.5f,
                BluntDR = 1.4f,
                SlashDR = 0.8f,
                PierceDR = 0.8f,
            },

            // Orc
            new Monster()
            {
                ID = 7,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Bash,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.Saber, WeaponMaterialTypes.Steel),
            },

            // Centaur
            new Monster()
            {
                ID = 8,
                Size = BodySize.Large,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Bash,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.Claymore, WeaponMaterialTypes.Elven),
            },

            // Werewolf
            new Monster()
            {
                ID = 9,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Fur,
                Attack = AttackType.Claw,
                AttackElement = AttackElementType.None,
                AttackOdds = 80,
                Attack2 = AttackType.Bite,
                AttackElement2 = AttackElementType.None,
                AttackOdds2 = 20,
                NaturalDT = 1f,
                BluntDR = 0.8f,
            },

            // Nymph
            new Monster()
            {
                ID = 10,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Elemental_Touch,
                AttackElement = AttackElementType.Draining,
                AttackOdds = 100,
            },

            // Slaughterfish
            new Monster()
            {
                ID = 11,
                Size = BodySize.Small,
                ArmorType = NaturalArmorType.Scale,
                Attack = AttackType.Bite,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
                NaturalDT = 1f,
                SlashDR = 0.8f,
            },

            // Orc Sergeant
            new Monster()
            {
                ID = 12,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Bash,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.Battle_Axe, WeaponMaterialTypes.Dwarven),
            },

            // Harpy
            new Monster()
            {
                ID = 13,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Fur,
                Attack = AttackType.Claw,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
                BluntDR = 0.9f,
            },

            // Wereboar
            new Monster()
            {
                ID = 14,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Fur,
                Attack = AttackType.Bash,
                AttackElement = AttackElementType.None,
                AttackOdds = 50,
                Attack2 = AttackType.Claw,
                AttackElement2 = AttackElementType.None,
                AttackOdds2 = 40,
                Attack3 = AttackType.Bite,
                AttackElement3 = AttackElementType.None,
                AttackOdds3 = 10,
                NaturalDT = 1.5f,
                BluntDR = 0.8f,
            },

            // Skeletal Warrior
            new Monster()
            {
                ID = 15,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Bone,
                Attack = AttackType.Bash,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.War_Axe, WeaponMaterialTypes.Steel),
                NaturalDT = 2f,
                BluntDR = 1.5f,
                SlashDR = 0.9f,
                PierceDR = 0.6f,
            },

            // Giant
            new Monster()
            {
                ID = 16,
                Size = BodySize.Large,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Bash,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.Warhammer, WeaponMaterialTypes.Steel),
            },

            // Zombie
            new Monster()
            {
                ID = 17,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Scratch,
                AttackElement = AttackElementType.None,
                AttackOdds = 40,
                Attack2 = AttackType.Bash,
                AttackElement2 = AttackElementType.None,
                AttackOdds2 = 35,
                Attack3 = AttackType.Bite,
                AttackElement3 = AttackElementType.None,
                AttackOdds3 = 25,
            },

            // Ghost
            new Monster()
            {
                ID = 18,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Ethereal,
                Attack = AttackType.Ethereal,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
            },

            // Mummy
            new Monster()
            {
                ID = 19,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Scratch,
                AttackElement = AttackElementType.None,
                AttackOdds = 40,
                Attack2 = AttackType.Bash,
                AttackElement2 = AttackElementType.None,
                AttackOdds2 = 35,
                Attack3 = AttackType.Bite,
                AttackElement3 = AttackElementType.None,
                AttackOdds3 = 25,
            },

            // Giant Scorpion
            new Monster()
            {
                ID = 20,
                Size = BodySize.Large,
                ArmorType = NaturalArmorType.Scale,
                Attack = AttackType.Sting,
                AttackElement = AttackElementType.None,
                AttackOdds = 65,
                Attack2 = AttackType.Pinch,
                AttackElement2 = AttackElementType.None,
                AttackOdds2 = 35,
                NaturalDT = 2.5f,
                BluntDR = 1.4f,
                SlashDR = 0.8f,
                PierceDR = 0.8f,
            },

            // Orc Shaman
            new Monster()
            {
                ID = 21,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Elemental_Touch,
                AttackElement = AttackElementType.Lightning,
                AttackOdds = 65,
                Attack2 = AttackType.Bash,
                AttackElement2 = AttackElementType.None,
                AttackOdds2 = 35,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.Staff, WeaponMaterialTypes.Adamantium),
            },

            // Gargoyle
            new Monster()
            {
                ID = 22,
                Size = BodySize.Large,
                ArmorType = NaturalArmorType.Rock,
                Attack = AttackType.Bash,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.Flail, WeaponMaterialTypes.Steel),
                NaturalDT = 4.25f,
                BluntDR = 2.0f,
                SlashDR = 0.7f,
                PierceDR = 0.35f,
            },

            // Wraith
            new Monster()
            {
                ID = 23,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Ethereal,
                Attack = AttackType.Ethereal,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
            },

            // Orc Warlord
            new Monster()
            {
                ID = 24,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Bash,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.Battle_Axe, WeaponMaterialTypes.Orcish),
            },

            // Frost Daedra
            new Monster()
            {
                ID = 25,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Metal,
                Attack = AttackType.Elemental_Bludgeon,
                AttackElement = AttackElementType.Ice,
                AttackOdds = 100,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.Warhammer, WeaponMaterialTypes.Daedric),
                NaturalDT = 4f,
                BluntDR = 1.25f,
                PierceDR = 0.8f,
            },

            // Fire Daedra
            new Monster()
            {
                ID = 26,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Metal,
                Attack = AttackType.Elemental_Slash,
                AttackElement = AttackElementType.Fire,
                AttackOdds = 100,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.Broadsword, WeaponMaterialTypes.Daedric),
                NaturalDT = 3f,
            },

            // Daedroth
            new Monster()
            {
                ID = 27,
                Size = BodySize.Large,
                ArmorType = NaturalArmorType.Scale,
                Attack = AttackType.Bite,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.Battle_Axe, WeaponMaterialTypes.Orcish),
                NaturalDT = 3f,
                SlashDR = 0.7f,
            },

            // Vampire
            new Monster()
            {
                ID = 28,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Scratch,
                AttackElement = AttackElementType.None,
                AttackOdds = 70,
                Attack2 = AttackType.Bite,
                AttackElement2 = AttackElementType.None,
                AttackOdds2 = 30,
            },

            // Daedra Seducer
            new Monster()
            {
                ID = 29,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Scratch,
                AttackElement = AttackElementType.None,
                AttackOdds = 50,
                Attack2 = AttackType.Elemental_Touch,
                AttackElement2 = AttackElementType.Magic,
                AttackOdds2 = 50,
            },

            // Vampire Ancient
            new Monster()
            {
                ID = 30,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Scratch,
                AttackElement = AttackElementType.None,
                AttackOdds = 70,
                Attack2 = AttackType.Bite,
                AttackElement2 = AttackElementType.None,
                AttackOdds2 = 30,
            },

            // Daedra Lord
            new Monster()
            {
                ID = 31,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Bash,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.Broadsword, WeaponMaterialTypes.Daedric),
                NaturalDT = 3f,
            },

            // Lich
            new Monster()
            {
                ID = 32,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Bone,
                Attack = AttackType.Elemental_Touch,
                AttackElement = AttackElementType.Magic,
                AttackOdds = 40,
                Attack2 = AttackType.Elemental_Touch,
                AttackElement2 = AttackElementType.Lightning,
                AttackOdds2 = 40,
                Attack3 = AttackType.Bash,
                AttackElement3 = AttackElementType.None,
                AttackOdds3 = 20,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.Staff, WeaponMaterialTypes.Adamantium),
                NaturalDT = 2f,
                BluntDR = 1.5f,
                SlashDR = 0.9f,
                PierceDR = 0.6f,
            },

            // Ancient Lich
            new Monster()
            {
                ID = 33,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Bone,
                Attack = AttackType.Elemental_Touch,
                AttackElement = AttackElementType.Magic,
                AttackOdds = 45,
                Attack2 = AttackType.Elemental_Touch,
                AttackElement2 = AttackElementType.Lightning,
                AttackOdds2 = 45,
                Attack3 = AttackType.Bash,
                AttackElement3 = AttackElementType.None,
                AttackOdds3 = 10,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.Staff, WeaponMaterialTypes.Adamantium),
                NaturalDT = 2f,
                BluntDR = 1.5f,
                SlashDR = 0.9f,
                PierceDR = 0.6f,
            },

            // Dragonling
            new Monster()
            {
                ID = 34,
                Size = BodySize.Small,
                ArmorType = NaturalArmorType.Scale,
                Attack = AttackType.Elemental_Breath,
                AttackElement = AttackElementType.Fire,
                AttackOdds = 70,
                Attack2 = AttackType.Claw,
                AttackElement2 = AttackElementType.None,
                AttackOdds2 = 30,
                NaturalDT = 2.75f,
                SlashDR = 0.7f,
            },

            // Fire Atronach
            new Monster()
            {
                ID = 35,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Elemental_Bludgeon,
                AttackElement = AttackElementType.Fire,
                AttackOdds = 100,
            },

            // Iron Atronach
            new Monster()
            {
                ID = 36,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Metal,
                Attack = AttackType.Bash,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.Mace, WeaponMaterialTypes.Iron),
                NaturalDT = 4.75f,
                BluntDR = 0.4f,
                SlashDR = 0.7f,
                PierceDR = 0.6f,
            },

            // Flesh Atronach
            new Monster()
            {
                ID = 37,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Flesh,
                Attack = AttackType.Bash,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
            },

            // Ice Atronach
            new Monster()
            {
                ID = 38,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Rock,
                Attack = AttackType.Elemental_Slash,
                AttackElement = AttackElementType.Ice,
                AttackOdds = 100,
                MonsterWeapon = DummyDFUItem.CreateDummyWeapon(Weapons.Katana, WeaponMaterialTypes.Elven),
                NaturalDT = 3.25f,
                BluntDR = 1.5f,
                PierceDR = 0.6f,
            },

            // Dragonling Alternate
            new Monster()
            {
                ID = 40,
                Size = BodySize.Huge,
                ArmorType = NaturalArmorType.Scale,
                Attack = AttackType.Elemental_Breath,
                AttackElement = AttackElementType.Fire,
                AttackOdds = 40,
                Attack2 = AttackType.Claw,
                AttackElement2 = AttackElementType.None,
                AttackOdds2 = 30,
                Attack3 = AttackType.Bite,
                AttackElement3 = AttackElementType.None,
                AttackOdds3 = 30,
                NaturalDT = 5f,
                BluntDR = 0.75f,
                SlashDR = 0.5f,
                PierceDR = 0.75f,
            },

            // Dreugh
            new Monster()
            {
                ID = 41,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Scale,
                Attack = AttackType.Pinch,
                AttackElement = AttackElementType.None,
                AttackOdds = 100,
                NaturalDT = 2.5f,
                BluntDR = 1.4f,
                SlashDR = 0.8f,
                PierceDR = 0.8f,
            },

            // Lamia
            new Monster()
            {
                ID = 42,
                Size = BodySize.Average,
                ArmorType = NaturalArmorType.Scale,
                Attack = AttackType.Elemental_Touch,
                AttackElement = AttackElementType.Draining,
                AttackOdds = 75,
                Attack2 = AttackType.Scratch,
                AttackElement2 = AttackElementType.None,
                AttackOdds2 = 25,
                NaturalDT = 1f,
                SlashDR = 0.8f,
            },
        };

        #endregion

        #region Helpers

        /// <summary>
        /// Build a dictionary of monsters keyed by ID.
        /// Use this once and store for faster monster lookups.
        /// </summary>
        /// <returns>Resulting dictionary of mobile monsters.</returns>
        public static Dictionary<int, Monster> BuildMonsterDict()
        {
            Dictionary<int, Monster> enemyDict = new Dictionary<int, Monster>();
            foreach (var monster in Monsters)
            {
                enemyDict.Add(monster.ID, monster);
            }

            return enemyDict;
        }

        #endregion

    }
}
