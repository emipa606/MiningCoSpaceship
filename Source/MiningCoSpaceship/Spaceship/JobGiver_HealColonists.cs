using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class JobGiver_HealColonists : ThinkNode_JobGiver
{
    protected int jobMaxDuration = 999999;

    public override ThinkNode DeepCopy(bool resolve = true)
    {
        var jobGiver_HealColonists = (JobGiver_HealColonists)base.DeepCopy(resolve);
        jobGiver_HealColonists.jobMaxDuration = jobMaxDuration;
        return jobGiver_HealColonists;
    }

    protected override Job TryGiveJob(Pawn pawn)
    {
        var tendableColonist = GetTendableColonist(pawn.Position, pawn.Map);
        if (tendableColonist != null)
        {
            Thing thing = null;
            if (Medicine.GetMedicineCountToFullyHeal(tendableColonist) > 0)
            {
                thing = FindBestUnforbiddenMedicine(pawn, tendableColonist);
            }

            return thing != null
                ? JobMaker.MakeJob(JobDefOf.TendPatient, tendableColonist, thing)
                : JobMaker.MakeJob(JobDefOf.TendPatient, tendableColonist);
        }

        pawn.GetLord()?.ReceiveMemo("HealFinished");
        return null;
    }

    public static Pawn GetTendableColonist(IntVec3 medicPosition, Map medicMap)
    {
        foreach (var item in medicMap.mapPawns.FreeColonistsSpawned.InRandomOrder())
        {
            if (item.health.HasHediffsNeedingTend() &&
                !medicMap.reservationManager.IsReservedByAnyoneOf(item, Faction.OfPlayer) &&
                !medicMap.reservationManager.IsReservedByAnyoneOf(item, Util_Faction.MiningCoFaction) && item.InBed() &&
                medicMap.reachability.CanReach(medicPosition, item, PathEndMode.ClosestTouch,
                    TraverseParms.For(TraverseMode.PassDoors)))
            {
                return item;
            }
        }

        return null;
    }

    public static Thing FindBestUnforbiddenMedicine(Pawn healer, Pawn patient)
    {
        if (patient.playerSettings == null || (int)patient.playerSettings.medCare <= 1)
        {
            return null;
        }

        bool Validator(Thing medicine)
        {
            return !medicine.IsForbidden(Faction.OfPlayer) &&
                   patient.playerSettings.medCare.AllowsMedicine(medicine.def) && healer.CanReserve(medicine);
        }

        float PriorityGetter(Thing t)
        {
            return t.def.GetStatValueAbstract(StatDefOf.MedicalPotency);
        }

        var position = patient.Position;
        var searchSet = patient.Map.listerThings.ThingsInGroup(ThingRequestGroup.PotentialBillGiver);
        var traverseParams = TraverseParms.For(healer, Danger.Some);
        return GenClosest.ClosestThing_Global_Reachable(position, patient.Map, searchSet, PathEndMode.ClosestTouch,
            traverseParams, 100f, Validator, PriorityGetter);
    }
}