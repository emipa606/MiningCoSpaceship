using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class Trigger_ReachableDownedPawn : Trigger
{
    private const int CheckInterval = 61;

    public override bool ActivateOn(Lord lord, TriggerSignal signal)
    {
        if (signal.type != TriggerSignalType.Tick || Find.TickManager.TicksGame % CheckInterval != 0)
        {
            return false;
        }

        var targetDestination = ((LordJob_MiningCoBase)lord.LordJob).targetDestination;
        if (lord.ownedPawns.NullOrEmpty())
        {
            lord.Cleanup();
            return false;
        }

        var randomReachableDownedPawn =
            Util_DownedPawn.GetRandomReachableDownedPawn(lord.ownedPawns.RandomElement());
        if (randomReachableDownedPawn == null)
        {
            return false;
        }

        foreach (var ownedPawn in lord.ownedPawns)
        {
            if (!ownedPawn.CanReserveAndReach(randomReachableDownedPawn, PathEndMode.OnCell, Danger.Some))
            {
                return false;
            }
        }

        var canReach = true;
        foreach (var ownedPawn2 in lord.ownedPawns)
        {
            if (ownedPawn2.CanReach(targetDestination, PathEndMode.OnCell, Danger.Some))
            {
                continue;
            }

            canReach = false;
            break;
        }

        if (canReach)
        {
            return true;
        }

        if (!Expedition.TryFindRandomExitSpot(lord.Map, lord.ownedPawns.RandomElement().Position, out var exitSpot))
        {
            return false;
        }

        canReach = true;
        foreach (var ownedPawn3 in lord.ownedPawns)
        {
            if (ownedPawn3.CanReach(exitSpot, PathEndMode.OnCell, Danger.Some))
            {
                continue;
            }

            canReach = false;
            break;
        }

        return canReach;
    }
}