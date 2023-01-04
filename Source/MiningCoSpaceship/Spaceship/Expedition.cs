using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Spaceship;

public static class Expedition
{
    public enum ExpeditionKind
    {
        Geologists = 1,
        Miners,
        OutpostSettlers,
        Scouts,
        Troopers
    }

    public static readonly Dictionary<PawnKindDef, int> expeditionGeologist = new Dictionary<PawnKindDef, int>
    {
        {
            Util_PawnKindDefOf.Geologist,
            4
        },
        {
            Util_PawnKindDefOf.Technician,
            1
        },
        {
            Util_PawnKindDefOf.Scout,
            1
        },
        {
            Util_PawnKindDefOf.Guard,
            3
        },
        {
            Util_PawnKindDefOf.Officer,
            1
        }
    };

    public static readonly Dictionary<PawnKindDef, int> expeditionMiners = new Dictionary<PawnKindDef, int>
    {
        {
            Util_PawnKindDefOf.Technician,
            1
        },
        {
            Util_PawnKindDefOf.Miner,
            4
        },
        {
            Util_PawnKindDefOf.Guard,
            4
        },
        {
            Util_PawnKindDefOf.Officer,
            1
        }
    };

    public static readonly Dictionary<PawnKindDef, int> expeditionOutpostSettlers = new Dictionary<PawnKindDef, int>
    {
        {
            Util_PawnKindDefOf.Technician,
            6
        },
        {
            Util_PawnKindDefOf.Scout,
            1
        },
        {
            Util_PawnKindDefOf.Guard,
            4
        },
        {
            Util_PawnKindDefOf.Officer,
            1
        }
    };

    public static readonly Dictionary<PawnKindDef, int> expeditionScouts = new Dictionary<PawnKindDef, int>
    {
        {
            Util_PawnKindDefOf.Scout,
            3
        },
        {
            Util_PawnKindDefOf.Officer,
            1
        }
    };

    public static readonly Dictionary<PawnKindDef, int> expeditionTroopers = new Dictionary<PawnKindDef, int>
    {
        {
            Util_PawnKindDefOf.Scout,
            2
        },
        {
            Util_PawnKindDefOf.Guard,
            8
        },
        {
            Util_PawnKindDefOf.ShockTrooper,
            4
        },
        {
            Util_PawnKindDefOf.HeavyGuard,
            2
        },
        {
            Util_PawnKindDefOf.Officer,
            2
        }
    };

    public static List<Pawn> GenerateExpeditionPawns(Map map)
    {
        var list = new List<Pawn>();
        var expeditionKind = (ExpeditionKind)Rand.RangeInclusive(1, 5);
        Dictionary<PawnKindDef, int> dictionary = null;
        switch (expeditionKind)
        {
            case ExpeditionKind.Geologists:
                dictionary = expeditionGeologist;
                break;
            case ExpeditionKind.Miners:
                dictionary = expeditionMiners;
                break;
            case ExpeditionKind.OutpostSettlers:
                dictionary = expeditionOutpostSettlers;
                break;
            case ExpeditionKind.Scouts:
                dictionary = expeditionScouts;
                break;
            case ExpeditionKind.Troopers:
                dictionary = expeditionTroopers;
                break;
            default:
                Log.ErrorOnce($"MiningCo. Spaceship: unhandled ExpeditionKind ({expeditionKind}).", 123456782);
                break;
        }

        if (dictionary == null)
        {
            return list;
        }

        foreach (var key in dictionary.Keys)
        {
            for (var i = 0; i < dictionary[key]; i++)
            {
                var item = MiningCoPawnGenerator.GeneratePawn(key, map);
                list.Add(item);
            }
        }

        return list;
    }

    public static bool IsWeatherValidForExpedition(Map map)
    {
        var seasonalTemp = map.mapTemperature.SeasonalTemp;
        return seasonalTemp is >= -20f and <= 50f &&
               !map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout);
    }

    public static bool TryFindRandomExitSpot(Map map, IntVec3 startSpot, out IntVec3 exitSpot)
    {
        bool Validator(IntVec3 cell)
        {
            return !cell.Fogged(map) && !map.roofGrid.Roofed(cell) && cell.Standable(map) &&
                   map.reachability.CanReach(startSpot, cell, PathEndMode.Touch, TraverseMode.PassDoors, Danger.Some);
        }

        return CellFinder.TryFindRandomEdgeCellWith(Validator, map, CellFinder.EdgeRoadChance_Always, out exitSpot);
    }

    public static void RandomlyDamagePawn(Pawn pawn, int injuriesNumber, int damageAmount)
    {
        if (pawn.story.traits.HasTrait(TraitDef.Named("Wimp")))
        {
            return;
        }

        var hediffSet = pawn.health.hediffSet;
        var num = 0;
        while (!pawn.Dead && num < injuriesNumber && HittablePartsViolence(hediffSet).Any())
        {
            num++;
            var bodyPartRecord = HittablePartsViolence(hediffSet).RandomElementByWeight(x => x.coverageAbs);
            var def = bodyPartRecord.depth != BodyPartDepth.Outside
                ? DamageDefOf.Blunt
                : HealthUtility.RandomViolenceDamageType();
            var dinfo = new DamageInfo(def, damageAmount, 0f, -1f, null, bodyPartRecord);
            pawn.TakeDamage(dinfo);
        }
    }

    private static IEnumerable<BodyPartRecord> HittablePartsViolence(HediffSet bodyModel)
    {
        return from x in bodyModel.GetNotMissingParts()
            where x.depth == BodyPartDepth.Outside ||
                  x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, bodyModel.hediffs)
            select x;
    }
}