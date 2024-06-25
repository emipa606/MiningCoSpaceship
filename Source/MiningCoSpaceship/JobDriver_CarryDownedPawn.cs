using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Spaceship;

public class JobDriver_CarryDownedPawn : JobDriver
{
    public readonly TargetIndex downedPawnIndex = TargetIndex.A;

    public readonly TargetIndex travelDestCellIndex = TargetIndex.B;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(TargetThingA, job);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var downedPawn = TargetThingA as Pawn;
        yield return Toils_Goto.GotoCell(downedPawnIndex, PathEndMode.OnCell)
            .FailOn(() => downedPawn.DestroyedOrNull() || downedPawn?.Downed == false);
        yield return Toils_Haul.StartCarryThing(downedPawnIndex);
        yield return Toils_Haul.CarryHauledThingToCell(travelDestCellIndex).FailOn(() =>
            pawn.carryTracker.CarriedThing.DestroyedOrNull() ||
            !pawn.CanReach(pawn.jobs.curJob.targetB.Cell, PathEndMode.OnCell, Danger.Some));
        yield return new Toil
        {
            initAction = delegate
            {
                Building_Spaceship building_Spaceship = null;
                var thingList = pawn.Position.GetThingList(pawn.Map);
                foreach (var item in thingList)
                {
                    if (item is not Building_Spaceship spaceship)
                    {
                        continue;
                    }

                    building_Spaceship = spaceship;
                    break;
                }

                pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var resultingThing);
                if (building_Spaceship != null)
                {
                    building_Spaceship.Notify_PawnBoarding(resultingThing as Pawn, false);
                }
                else if (pawn.Position.CloseToEdge(pawn.Map, 5))
                {
                    resultingThing.Destroy();
                    Util_Faction.AffectGoodwillWith(pawn.Faction, Faction.OfPlayer, 1);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}