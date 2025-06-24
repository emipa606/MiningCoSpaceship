using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class Trigger_PawnCannotReachTargetDestination : Trigger
{
    private const int CheckInterval = 63;

    public override bool ActivateOn(Lord lord, TriggerSignal signal)
    {
        if (signal.type != TriggerSignalType.Tick || Find.TickManager.TicksGame % CheckInterval != 0)
        {
            return false;
        }

        var targetDestination = ((LordJob_MiningCoBase)lord.LordJob).targetDestination;
        foreach (var ownedPawn in lord.ownedPawns)
        {
            if (ownedPawn.Map != null && !ownedPawn.CanReach(targetDestination, PathEndMode.OnCell, Danger.Some))
            {
                return true;
            }
        }

        return false;
    }
}