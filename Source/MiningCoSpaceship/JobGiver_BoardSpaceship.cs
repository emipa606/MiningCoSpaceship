using RimWorld;
using Verse;
using Verse.AI;

namespace Spaceship;

public class JobGiver_BoardSpaceship : ThinkNode_JobGiver
{
    private LocomotionUrgency defaultLocomotion;

    private int jobMaxDuration = 999999;

    public override ThinkNode DeepCopy(bool resolve = true)
    {
        var jobGiverBoardSpaceship = (JobGiver_BoardSpaceship)base.DeepCopy(resolve);
        jobGiverBoardSpaceship.defaultLocomotion = defaultLocomotion;
        jobGiverBoardSpaceship.jobMaxDuration = jobMaxDuration;
        return jobGiverBoardSpaceship;
    }

    protected override Job TryGiveJob(Pawn pawn)
    {
        if (pawn.Position != pawn.DutyLocation())
        {
            var job = JobMaker.MakeJob(JobDefOf.Goto, pawn.DutyLocation());
            job.locomotionUrgency = pawn.mindState.duty.locomotion;
            job.expiryInterval = jobMaxDuration;
            return job;
        }

        Building_Spaceship buildingSpaceship = null;
        foreach (var thing in pawn.Position.GetThingList(pawn.Map))
        {
            if (thing is not Building_Spaceship spaceship)
            {
                continue;
            }

            buildingSpaceship = spaceship;
            break;
        }

        return buildingSpaceship != null
            ? JobMaker.MakeJob(Util_JobDefOf.JobDef_BoardSpaceship, buildingSpaceship)
            : null;
    }
}