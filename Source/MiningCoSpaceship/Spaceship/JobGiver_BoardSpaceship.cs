using RimWorld;
using Verse;
using Verse.AI;

namespace Spaceship;

public class JobGiver_BoardSpaceship : ThinkNode_JobGiver
{
    protected LocomotionUrgency defaultLocomotion;

    protected int jobMaxDuration = 999999;

    public override ThinkNode DeepCopy(bool resolve = true)
    {
        var jobGiver_BoardSpaceship = (JobGiver_BoardSpaceship)base.DeepCopy(resolve);
        jobGiver_BoardSpaceship.defaultLocomotion = defaultLocomotion;
        jobGiver_BoardSpaceship.jobMaxDuration = jobMaxDuration;
        return jobGiver_BoardSpaceship;
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

        Building_Spaceship building_Spaceship = null;
        foreach (var thing in pawn.Position.GetThingList(pawn.Map))
        {
            if (thing is not Building_Spaceship spaceship)
            {
                continue;
            }

            building_Spaceship = spaceship;
            break;
        }

        return building_Spaceship != null ? JobMaker.MakeJob(Util_JobDefOf.BoardSpaceship, building_Spaceship) : null;
    }
}