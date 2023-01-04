using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class JobDriver_BoardSpaceship : JobDriver
{
    public TargetIndex spaceshipIndex = TargetIndex.A;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var spaceship = TargetThingA as Building_Spaceship;
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch).FailOn(() => spaceship.DestroyedOrNull());
        yield return new Toil
        {
            initAction = delegate
            {
                var isLastLordPawn = false;
                var lord = pawn.GetLord();
                if (lord != null)
                {
                    if (lord.ownedPawns.Count == 1)
                    {
                        isLastLordPawn = true;
                    }

                    lord.Notify_PawnLost(pawn, PawnLostCondition.ChangedFaction);
                }

                spaceship?.Notify_PawnBoarding(pawn, isLastLordPawn);
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}