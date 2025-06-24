using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Spaceship;

public class WorkGiver_TransferToMedibay : WorkGiver_Scanner
{
    public override PathEndMode PathEndMode => PathEndMode.OnCell;

    public override bool ShouldSkip(Pawn pawn, bool forced = false)
    {
        return !Find.Selector.IsSelected(pawn) ||
               pawn.Map.listerThings.ThingsOfDef(Util_Spaceship.SpaceshipMedical).NullOrEmpty();
    }

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        var list = new List<Thing>();
        foreach (var item in pawn.Map.mapPawns.FreeHumanlikesSpawnedOfFaction(pawn.Faction))
        {
            list.Add(item);
        }

        return list;
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (!forced)
        {
            return false;
        }

        if (t is not Pawn pawn2 || pawn.Faction != pawn2.Faction || pawn2.def.race.Animal)
        {
            return false;
        }

        if (!WorldComponent_OrbitalHealing.HasAnyTreatableHediff(pawn2))
        {
            return false;
        }

        foreach (var item in pawn.Map.listerThings.ThingsOfDef(Util_Spaceship.SpaceshipMedical))
        {
            if (item is Building_SpaceshipMedical buildingSpaceshipMedical && pawn2.Downed &&
                pawn.CanReserveAndReach(pawn2, PathEndMode, Danger.Deadly, 1, -1, null, true) &&
                pawn.CanReach(item, PathEndMode, Danger.Deadly) &&
                buildingSpaceshipMedical.orbitalHealingPawnsAboardCount < 6 &&
                TradeUtility.ColonyHasEnoughSilver(pawn.Map, Util_Spaceship.medicalSupplyCostInSilver))
            {
                return true;
            }
        }

        return false;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var num = 99999f;
        Building_SpaceshipMedical buildingSpaceshipMedical = null;
        foreach (var item in pawn.Map.listerThings.ThingsOfDef(Util_Spaceship.SpaceshipMedical))
        {
            var num2 = t.Position.DistanceTo(item.Position);
            if (!(num2 < num) || !pawn.CanReach(item, PathEndMode, Danger.Deadly))
            {
                continue;
            }

            num = num2;
            buildingSpaceshipMedical = item as Building_SpaceshipMedical;
        }

        var job = JobMaker.MakeJob(Util_JobDefOf.JobDef_TransferToMedibay, t, buildingSpaceshipMedical);
        job.count = 1;
        return job;
    }
}