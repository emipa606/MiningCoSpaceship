using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Spaceship;

public class JobDriver_TradeWithCargoSpaceship : JobDriver
{
    public readonly TargetIndex cargoSpaceshipIndex = TargetIndex.A;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(TargetThingA, job);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var cargoSpaceship = TargetThingA as Building_SpaceshipCargo;
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch)
            .FailOn(() => cargoSpaceship.DestroyedOrNull() || cargoSpaceship?.CanTradeNow == false);
        yield return new Toil
        {
            initAction = delegate
            {
                if (cargoSpaceship != null)
                {
                    GetActor().rotationTracker.FaceCell(cargoSpaceship.Position);
                }
            }
        };
        yield return new Toil
        {
            initAction = delegate { Find.WindowStack.Add(new Dialog_Trade(GetActor(), cargoSpaceship)); },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        yield return Toils_Reserve.Release(cargoSpaceshipIndex);
    }
}