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
        var lordToilEscortDownedPawn = pawn.GetLord().CurLordToil as LordToil_EscortDownedPawn;
        var nearestReachableDownedPawn = Util_DownedPawn.GetNearestReachableDownedPawn(pawn);
        if (nearestReachableDownedPawn != null)
        {
            if (lordToilEscortDownedPawn != null)
            {
                var job = JobMaker.MakeJob(Util_JobDefOf.JobDef_CarryDownedPawn, nearestReachableDownedPawn,
                    lordToilEscortDownedPawn.Data.targetDestination);
                job.count = 1;
                return job;
            }
        }

        lordToilEscortDownedPawn?.Notify_RescueEnded();
        return null;
    }
}