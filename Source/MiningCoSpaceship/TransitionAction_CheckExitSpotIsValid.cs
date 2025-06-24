using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class TransitionAction_CheckExitSpotIsValid : TransitionAction
{
    public override void DoAction(Transition trans)
    {
        var lord = trans.target.lord;
        var targetDestination = ((LordJob_MiningCoBase)lord.LordJob).targetDestination;
        var cannotReach = false;
        if (!targetDestination.InBounds(lord.Map) || targetDestination.x != 0 &&
            targetDestination.x != lord.Map.Size.x - 1 && targetDestination.z != 0 &&
            targetDestination.z != lord.Map.Size.z - 1)
        {
            cannotReach = true;
        }
        else
        {
            foreach (var ownedPawn in lord.ownedPawns)
            {
                if (ownedPawn.CanReach(targetDestination, PathEndMode.OnCell, Danger.Some))
                {
                    continue;
                }

                cannotReach = true;
                break;
            }
        }

        var exitSpot = targetDestination;
        if (cannotReach &&
            !Expedition.TryFindRandomExitSpot(lord.Map, lord.ownedPawns.RandomElement().Position, out exitSpot))
        {
            exitSpot = CellFinder.RandomEdgeCell(lord.Map);
        }

        ((LordJob_MiningCoBase)lord.LordJob).targetDestination = exitSpot;
        switch (trans.target)
        {
            case LordToil_Travel lordToilTravel:
                lordToilTravel.SetDestination(exitSpot);
                break;
            case LordToil_EscortDownedPawn lordToilEscortDownedPawn:
                lordToilEscortDownedPawn.SetDestination(exitSpot);
                break;
        }
    }
}