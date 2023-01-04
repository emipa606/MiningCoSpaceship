using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class JobGiver_RunToCarrier : JobGiver_Wander
{
    public JobGiver_RunToCarrier()
    {
        wanderRadius = 5f;
        locomotionUrgency = LocomotionUrgency.Sprint;
    }

    protected override IntVec3 GetWanderRoot(Pawn pawn)
    {
        var result = pawn.mindState.duty.focus.Cell;
        var carrier = (pawn.GetLord().CurLordToil as LordToil_EscortDownedPawn)?.Data.carrier;
        if (carrier?.pather != null && carrier.pather.Moving && carrier.pather.curPath is { NodesLeftCount: > 15 })
        {
            result = carrier.pather.curPath.Peek(15);
        }

        return result;
    }
}