using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Spaceship;

public static class MiningCoPawnGenerator
{
    private static readonly Color colorArmyGreenBright = new(10f / 51f, 20f / 51f, 0f);

    private static readonly Color colorArmyGreenDark = new(0.11764706f, 16f / 51f, 0f);

    private static readonly Color colorArmyBrown = new(0.47058824f, 14f / 51f, 0f);

    private static readonly Color colorArmyWhite = new(44f / 51f, 44f / 51f, 44f / 51f);

    private static readonly Color colorArmyGrey = new(40f / 51f, 40f / 51f, 40f / 51f);

    private static readonly Color colorArmyPaleSand = new(44f / 51f, 0.8235294f, 0.5882353f);

    private static readonly Color colorArmyBrownSand = new(43f / 51f, 0.7058824f, 0.47058824f);

    private static readonly Color colorCivilLightGrey = new(40f / 51f, 40f / 51f, 40f / 51f);

    private static readonly Color colorCivilGrey = new(32f / 51f, 32f / 51f, 32f / 51f);

    private static Color pantColor;

    private static Color shirtColor;

    private static Color armorColor;

    private static Color helmetColor;

    private static bool needParka;

    public static Pawn GeneratePawn(PawnKindDef kindDef, Map map)
    {
        setUniformColor(map.Biome, map.mapTemperature.SeasonalTemp);
        Predicate<Pawn> predicate = null;
        if (kindDef == Util_PawnKindDefOf.Medic)
        {
            predicate = medic => (medic.story.DisabledWorkTagsBackstoryAndTraits & WorkTags.Caring) == 0;
        }

        var request = new PawnGenerationRequest(kindDef, Util_Faction.MiningCoFaction, PawnGenerationContext.NonPlayer,
            -1, true, false, false, false, true, 0f, allowFood: true, allowAddictions: true, inhabitant: false,
            certainlyBeenInCryptosleep: true, forceRedressWorldPawnIfFormerColonist: false,
            worldPawnFactionDoesntMatter: false, biocodeWeaponChance: 0f, biocodeApparelChance: 0f,
            extraPawnForExtraRelationChance: null, relationWithExtraPawnChanceFactor: 1f, validatorPreGear: predicate);
        var pawn = PawnGenerator.GeneratePawn(request);
        SetMedicSkill(ref pawn);
        generatePawnApparelAndWeapon(ref pawn, kindDef, map.mapTemperature.SeasonalTemp);
        return pawn;
    }

    private static void setUniformColor(BiomeDef biome, float temperature)
    {
        var defName = biome.defName;
        switch (defName)
        {
            case "IceSheet" or "SeaIce":
                pantColor = colorArmyGrey;
                shirtColor = colorArmyWhite;
                armorColor = colorArmyWhite;
                helmetColor = colorArmyGrey;
                needParka = true;
                return;
            case "AridShrubland" or "Desert" or "ExtremeDesert":
                pantColor = colorArmyBrownSand;
                shirtColor = colorArmyPaleSand;
                armorColor = colorArmyPaleSand;
                helmetColor = colorArmyBrownSand;
                return;
        }

        if (temperature < 0f)
        {
            switch (defName)
            {
                default:
                    if (defName != "TropicalSwamp")
                    {
                        break;
                    }

                    goto case "Tundra";
                case "Tundra":
                case "BorealForest":
                case "ColdBog":
                case "TemperateForest":
                case "TemperateSwamp":
                case "TropicalRainforest":
                    pantColor = colorArmyGrey;
                    shirtColor = colorArmyWhite;
                    armorColor = colorArmyWhite;
                    helmetColor = colorArmyGrey;
                    needParka = true;
                    return;
            }
        }

        pantColor = colorArmyBrown;
        shirtColor = colorArmyGreenBright;
        armorColor = colorArmyGreenBright;
        helmetColor = colorArmyGreenDark;
    }

    private static void SetMedicSkill(ref Pawn pawn)
    {
        if (pawn.kindDef != Util_PawnKindDefOf.Medic)
        {
            return;
        }

        pawn.skills.GetSkill(SkillDefOf.Medicine).passion = Passion.Major;
        if (pawn.skills.GetSkill(SkillDefOf.Medicine).Level < 14)
        {
            pawn.skills.GetSkill(SkillDefOf.Medicine).Level = 14;
        }
    }

    private static void generatePawnApparelAndWeapon(ref Pawn pawn, PawnKindDef kindDef, float temperature)
    {
        if (kindDef == Util_PawnKindDefOf.Technician)
        {
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"),
                ThingDef.Named("Synthread"), colorCivilLightGrey);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CollarShirt"),
                ThingDef.Named("Synthread"), colorCivilLightGrey);
            if (temperature < 20f)
            {
                generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Tuque,
                    ThingDef.Named("Synthread"), colorCivilLightGrey);
            }

            if (needParka)
            {
                generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Parka,
                    ThingDef.Named("Synthread"), armorColor);
            }

            generatePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_Autopistol"));
        }
        else if (kindDef == Util_PawnKindDefOf.Miner)
        {
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"),
                ThingDef.Named("Synthread"), colorCivilLightGrey);
            if (Util_Misc.IsModActive("Rikiki.MiningCo.MiningHelmet"))
            {
                generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_MiningHelmet"), null,
                    Color.black, false);
                generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_MiningVest"), null,
                    Color.black, false);
            }
            else
            {
                generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_BasicShirt"),
                    ThingDef.Named("Synthread"), colorCivilLightGrey);
                if (temperature < 20f)
                {
                    generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Tuque,
                        ThingDef.Named("Synthread"), colorCivilLightGrey);
                }

                if (needParka)
                {
                    generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Parka,
                        ThingDef.Named("Synthread"), armorColor);
                }
            }

            generatePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_HeavySMG"));
        }
        else if (kindDef == Util_PawnKindDefOf.Geologist)
        {
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"),
                ThingDef.Named("Synthread"), colorCivilLightGrey);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CollarShirt"),
                ThingDef.Named("Synthread"), colorCivilLightGrey);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Jacket"),
                ThingDef.Named("Synthread"), colorCivilGrey);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CowboyHat"),
                ThingDef.Named("Synthread"), colorCivilGrey);
            if (needParka)
            {
                generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Parka,
                    ThingDef.Named("Synthread"), armorColor);
            }

            generatePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_MachinePistol"));
        }
        else if (kindDef == Util_PawnKindDefOf.Medic)
        {
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_FlakPants"), null, pantColor);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CollarShirt"),
                ThingDef.Named("Synthread"), shirtColor);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_FlakVest"), null, Color.black,
                false);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Duster"),
                ThingDef.Named("Synthread"), colorArmyWhite);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_SimpleHelmetMedic"), null,
                colorArmyWhite);
            if (needParka)
            {
                generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Parka,
                    ThingDef.Named("Synthread"), colorArmyWhite);
            }

            generatePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_MachinePistol"));
        }
        else if (kindDef == Util_PawnKindDefOf.Pilot || kindDef == Util_PawnKindDefOf.Scout)
        {
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"),
                ThingDef.Named("Synthread"), pantColor);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_BasicShirt"),
                ThingDef.Named("Synthread"), shirtColor);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_FlakVest"), null, Color.black,
                false);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_SimpleHelmet"),
                ThingDefOf.Plasteel, helmetColor);
            if (needParka)
            {
                generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Parka,
                    ThingDef.Named("Synthread"), armorColor);
            }

            if (kindDef == Util_PawnKindDefOf.Pilot)
            {
                generatePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_Autopistol"));
            }
            else
            {
                generatePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_AssaultRifle"));
            }
        }
        else if (kindDef == Util_PawnKindDefOf.Guard)
        {
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_FlakPants"), null, pantColor);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_BasicShirt"),
                ThingDef.Named("Synthread"), shirtColor);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_PowerArmor"), null, armorColor);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_AdvancedHelmet"),
                ThingDefOf.Plasteel, helmetColor);
            generatePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_ChargeRifle"));
            if (Rand.Value < 0.5f)
            {
                generatePawnApparel(ref pawn, kindDef.itemQuality, Util_ThingDefOf.Apparel_SmokepopBelt, null,
                    armorColor);
            }
        }
        else if (kindDef == Util_PawnKindDefOf.ShockTrooper || kindDef == Util_PawnKindDefOf.HeavyGuard)
        {
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_FlakPants"), null, pantColor);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_BasicShirt"),
                ThingDef.Named("Synthread"), shirtColor);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_PowerArmor"), null, armorColor);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_PowerArmorHelmet"), null,
                helmetColor);
            if (kindDef == Util_PawnKindDefOf.ShockTrooper)
            {
                generatePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_ChainShotgun"));
            }
            else
            {
                generatePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_Minigun"));
            }
        }
        else if (kindDef == Util_PawnKindDefOf.Officer)
        {
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_FlakPants"), null, pantColor);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CollarShirt"),
                Util_ThingDefOf.Hyperweave, shirtColor);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_FlakVest"), null, Color.black,
                false);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Duster"),
                Util_ThingDefOf.Hyperweave,
                armorColor);
            generatePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CowboyHat"),
                Util_ThingDefOf.Hyperweave, helmetColor);
            generatePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_SniperRifle"));
        }
        else
        {
            Log.ErrorOnce($"MiningCo. Spaceship: unhandled PawnKindDef ({kindDef}).", 123456786);
        }
    }

    private static void generatePawnApparel(ref Pawn pawn, QualityCategory apparelQuality, ThingDef apparelDef,
        ThingDef apparelStuff, Color apparelColor, bool applyColor = true)
    {
        var apparel = ThingMaker.MakeThing(apparelDef, apparelStuff) as Apparel;
        if (applyColor)
        {
            apparel.SetColor(apparelColor);
        }

        apparel.TryGetComp<CompQuality>().SetQuality(apparelQuality, ArtGenerationContext.Outsider);
        pawn.apparel.Wear(apparel, false);
    }

    private static void generatePawnWeapon(ref Pawn pawn, QualityCategory weaponQuality, ThingDef weaponDef)
    {
        var thingWithComps = ThingMaker.MakeThing(weaponDef) as ThingWithComps;
        thingWithComps.TryGetComp<CompQuality>().SetQuality(weaponQuality, ArtGenerationContext.Outsider);
        thingWithComps.TryGetComp<CompBiocodable>().CodeFor(pawn);
        pawn.equipment.AddEquipment(thingWithComps);
    }
}