using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Spaceship;

public static class MiningCoPawnGenerator
{
    private static readonly Color colorArmyGreenBright = new Color(10f / 51f, 20f / 51f, 0f);

    private static readonly Color colorArmyGreenDark = new Color(0.11764706f, 16f / 51f, 0f);

    private static readonly Color colorArmyBrown = new Color(0.47058824f, 14f / 51f, 0f);

    private static readonly Color colorArmyWhite = new Color(44f / 51f, 44f / 51f, 44f / 51f);

    private static readonly Color colorArmyGrey = new Color(40f / 51f, 40f / 51f, 40f / 51f);

    private static readonly Color colorArmyPaleSand = new Color(44f / 51f, 0.8235294f, 0.5882353f);

    private static readonly Color colorArmyBrownSand = new Color(43f / 51f, 0.7058824f, 0.47058824f);

    private static readonly Color colorCivilLightGrey = new Color(40f / 51f, 40f / 51f, 40f / 51f);

    private static readonly Color colorCivilGrey = new Color(32f / 51f, 32f / 51f, 32f / 51f);

    private static Color pantColor;

    private static Color shirtColor;

    private static Color armorColor;

    private static Color helmetColor;

    private static bool needParka;

    public static Pawn GeneratePawn(PawnKindDef kindDef, Map map)
    {
        SetUniformColor(map.Biome, map.mapTemperature.SeasonalTemp);
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
        GeneratePawnApparelAndWeapon(ref pawn, kindDef, map.mapTemperature.SeasonalTemp);
        return pawn;
    }

    public static void SetUniformColor(BiomeDef biome, float temperature)
    {
        var defName = biome.defName;
        if (defName is "IceSheet" or "SeaIce")
        {
            pantColor = colorArmyGrey;
            shirtColor = colorArmyWhite;
            armorColor = colorArmyWhite;
            helmetColor = colorArmyGrey;
            needParka = true;
            return;
        }

        if (defName is "AridShrubland" or "Desert" or "ExtremeDesert")
        {
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

    public static void SetMedicSkill(ref Pawn pawn)
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

    public static void GeneratePawnApparelAndWeapon(ref Pawn pawn, PawnKindDef kindDef, float temperature)
    {
        if (kindDef == Util_PawnKindDefOf.Technician)
        {
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"),
                ThingDef.Named("Synthread"), colorCivilLightGrey);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CollarShirt"),
                ThingDef.Named("Synthread"), colorCivilLightGrey);
            if (temperature < 20f)
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Tuque,
                    ThingDef.Named("Synthread"), colorCivilLightGrey);
            }

            if (needParka)
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Parka,
                    ThingDef.Named("Synthread"), armorColor);
            }

            GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_Autopistol"));
        }
        else if (kindDef == Util_PawnKindDefOf.Miner)
        {
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"),
                ThingDef.Named("Synthread"), colorCivilLightGrey);
            if (Util_Misc.IsModActive("Rikiki.MiningCo.MiningHelmet"))
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_MiningHelmet"), null,
                    Color.black, false);
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_MiningVest"), null,
                    Color.black, false);
            }
            else
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_BasicShirt"),
                    ThingDef.Named("Synthread"), colorCivilLightGrey);
                if (temperature < 20f)
                {
                    GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Tuque,
                        ThingDef.Named("Synthread"), colorCivilLightGrey);
                }

                if (needParka)
                {
                    GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Parka,
                        ThingDef.Named("Synthread"), armorColor);
                }
            }

            GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_HeavySMG"));
        }
        else if (kindDef == Util_PawnKindDefOf.Geologist)
        {
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"),
                ThingDef.Named("Synthread"), colorCivilLightGrey);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CollarShirt"),
                ThingDef.Named("Synthread"), colorCivilLightGrey);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Jacket"),
                ThingDef.Named("Synthread"), colorCivilGrey);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CowboyHat"),
                ThingDef.Named("Synthread"), colorCivilGrey);
            if (needParka)
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Parka,
                    ThingDef.Named("Synthread"), armorColor);
            }

            GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_MachinePistol"));
        }
        else if (kindDef == Util_PawnKindDefOf.Medic)
        {
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_FlakPants"), null, pantColor);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CollarShirt"),
                ThingDef.Named("Synthread"), shirtColor);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_FlakVest"), null, Color.black,
                false);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Duster"),
                ThingDef.Named("Synthread"), colorArmyWhite);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_SimpleHelmetMedic"), null,
                colorArmyWhite);
            if (needParka)
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Parka,
                    ThingDef.Named("Synthread"), colorArmyWhite);
            }

            GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_MachinePistol"));
        }
        else if (kindDef == Util_PawnKindDefOf.Pilot || kindDef == Util_PawnKindDefOf.Scout)
        {
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Pants"),
                ThingDef.Named("Synthread"), pantColor);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_BasicShirt"),
                ThingDef.Named("Synthread"), shirtColor);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_FlakVest"), null, Color.black,
                false);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_SimpleHelmet"),
                ThingDefOf.Plasteel, helmetColor);
            if (needParka)
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDefOf.Apparel_Parka,
                    ThingDef.Named("Synthread"), armorColor);
            }

            if (kindDef == Util_PawnKindDefOf.Pilot)
            {
                GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_Autopistol"));
            }
            else
            {
                GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_AssaultRifle"));
            }
        }
        else if (kindDef == Util_PawnKindDefOf.Guard)
        {
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_FlakPants"), null, pantColor);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_BasicShirt"),
                ThingDef.Named("Synthread"), shirtColor);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_PowerArmor"), null, armorColor);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_AdvancedHelmet"),
                ThingDefOf.Plasteel, helmetColor);
            GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_ChargeRifle"));
            if (Rand.Value < 0.5f)
            {
                GeneratePawnApparel(ref pawn, kindDef.itemQuality, Util_ThingDefOf.Apparel_SmokepopBelt, null,
                    armorColor);
            }
        }
        else if (kindDef == Util_PawnKindDefOf.ShockTrooper || kindDef == Util_PawnKindDefOf.HeavyGuard)
        {
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_FlakPants"), null, pantColor);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_BasicShirt"),
                ThingDef.Named("Synthread"), shirtColor);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_PowerArmor"), null, armorColor);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_PowerArmorHelmet"), null,
                helmetColor);
            if (kindDef == Util_PawnKindDefOf.ShockTrooper)
            {
                GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_ChainShotgun"));
            }
            else
            {
                GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_Minigun"));
            }
        }
        else if (kindDef == Util_PawnKindDefOf.Officer)
        {
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_FlakPants"), null, pantColor);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CollarShirt"),
                Util_ThingDefOf.Hyperweave, shirtColor);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_FlakVest"), null, Color.black,
                false);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_Duster"),
                Util_ThingDefOf.Hyperweave,
                armorColor);
            GeneratePawnApparel(ref pawn, kindDef.itemQuality, ThingDef.Named("Apparel_CowboyHat"),
                Util_ThingDefOf.Hyperweave, helmetColor);
            GeneratePawnWeapon(ref pawn, kindDef.itemQuality, ThingDef.Named("Gun_SniperRifle"));
        }
        else
        {
            Log.ErrorOnce($"MiningCo. Spaceship: unhandled PawnKindDef ({kindDef}).", 123456786);
        }
    }

    public static void GeneratePawnApparel(ref Pawn pawn, QualityCategory apparelQuality, ThingDef apparelDef,
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

    private static void GeneratePawnWeapon(ref Pawn pawn, QualityCategory weaponQuality, ThingDef weaponDef)
    {
        var thingWithComps = ThingMaker.MakeThing(weaponDef) as ThingWithComps;
        thingWithComps.TryGetComp<CompQuality>().SetQuality(weaponQuality, ArtGenerationContext.Outsider);
        thingWithComps.TryGetComp<CompBiocodable>().CodeFor(pawn);
        pawn.equipment.AddEquipment(thingWithComps);
    }
}