using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Spaceship;

public static class Util_Faction
{
    public static FactionDef MiningCoFactionDef => FactionDef.Named("MiningCo");

    public static Faction MiningCoFaction => Find.FactionManager.FirstFactionOfDef(MiningCoFactionDef);

    public static bool AffectGoodwillWith(Faction faction, Faction other, int goodwillChange,
        bool canSendMessage = true, bool canSendHostilityLetter = true, string reason = null,
        GlobalTargetInfo? lookTarget = null)
    {
        if (faction == null || other == null)
        {
            Log.ErrorOnce(
                "MiningCo. Spaceship: faction or other is null. Did you started a new world/colony when enabling this mod?",
                123456779);
            return false;
        }

        if (goodwillChange == 0)
        {
            return true;
        }

        var num = faction.GoodwillWith(other);
        var num2 = Mathf.Clamp(num + goodwillChange, -100, 100);
        if (num == num2)
        {
            return true;
        }

        var factionRelation = faction.RelationWith(other);
        factionRelation.baseGoodwill = num2;
        factionRelation.CheckKindThresholds(faction, canSendHostilityLetter, reason,
            lookTarget ?? GlobalTargetInfo.Invalid, out _);
        var factionRelation2 = other.RelationWith(faction);
        var kind = factionRelation2.kind;
        factionRelation2.baseGoodwill = factionRelation.baseGoodwill;
        factionRelation2.kind = factionRelation.kind;
        if (kind != factionRelation2.kind)
        {
            other.Notify_RelationKindChanged(faction, kind, canSendHostilityLetter, reason,
                lookTarget ?? GlobalTargetInfo.Invalid, out _);
        }

        if (faction != MiningCoFaction || other != Faction.OfPlayer ||
            factionRelation.kind != FactionRelationKind.Hostile)
        {
            return true;
        }

        Util_Misc.Partnership.globalGoodwillFeeInSilver = 2000;
        Util_Misc.OrbitalHealing.Notify_BecameHostileToColony();
        foreach (var map in Find.Maps)
        {
            if (map.IsPlayerHome)
            {
                var list = new List<Building_Spaceship>();
                foreach (var allThing in map.listerThings.AllThings)
                {
                    if (allThing is Building_Spaceship thing)
                    {
                        list.Add(thing);
                    }
                }

                foreach (var item in list)
                {
                    item.RequestTakeOff();
                }
            }

            var lordManager = map.lordManager;
            foreach (var lord in lordManager.lords)
            {
                if (lord.faction == MiningCoFaction)
                {
                    lord.ReceiveMemo("BecameHostileToColony");
                }
            }
        }

        return true;
    }
}