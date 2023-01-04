using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class JobGiver_CarryDownedPawn : ThinkNode_JobGiver
{
    protected LocomotionUrgency defaultLocomotion = LocomotionUrgency.Jog;

    protected int jobMaxDuration = 999999;

    public override ThinkNode DeepCopy(bool resolve = true)
    {
        return (JobGiver_CarryDownedPawn)base.DeepCopy(resolve);
    }

    protected override Job TryGiveJob(Pawn pawn)
    {
        var lordToil_EscortDownedPawn = pawn.GetLord().CurLordToil as LordToil_EscortDownedPawn;
        var nearestReachableDownedPawn = Util_DownedPawn.GetNearestReachableDownedPawn(pawn);
        if (nearestReachableDownedPawn != null)
        {
            if (lordToil_EscortDownedPawn != null)
            {
                var job = JobMaker.MakeJob(Util_JobDefOf.CarryDownedPawn, nearestReachableDownedPawn,
                    lordToil_EscortDownedPawn.Data.targetDestination);
                job.count = 1;
                return job;
            }
        }

        lordToil_EscortDownedPawn?.Notify_RescueEnded();
        return null;
    }
}