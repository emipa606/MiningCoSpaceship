using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Spaceship;

public class JobDriver_TransferToMedibay : JobDriver
{
    private const TargetIndex DownedPawnIndex = TargetIndex.A;

    private const TargetIndex MedicalSpaceshipCellIndex = TargetIndex.B;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(TargetThingA, job);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var downedPawn = TargetThingA as Pawn;
        yield return Toils_Goto.GotoCell(DownedPawnIndex, PathEndMode.OnCell)
            .FailOn(() => downedPawn.DestroyedOrNull() || downedPawn?.Downed == false);
        yield return Toils_Haul.StartCarryThing(DownedPawnIndex);
        yield return Toils_Haul.CarryHauledThingToCell(MedicalSpaceshipCellIndex).FailOn(() =>
            pawn.carryTracker.CarriedThing.DestroyedOrNull() ||
            !pawn.CanReach(pawn.jobs.curJob.targetB.Cell, PathEndMode.OnCell, Danger.Some));
        yield return new Toil
        {
            initAction = delegate
            {
                pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var resultingThing);
                if (pawn.Position.GetFirstThing(pawn.Map, Util_Spaceship.SpaceshipMedical) is not
                    Building_SpaceshipMedical
                    buildingSpaceshipMedical)
                {
                    return;
                }

                if (buildingSpaceshipMedical.orbitalHealingPawnsAboardCount >= 6)
                {
                    Messages.Message("MCS.cannotboardmedic".Translate((resultingThing as Pawn)?.Name.ToStringShort),
                        resultingThing, MessageTypeDefOf.RejectInput);
                }
                else if (TradeUtility.ColonyHasEnoughSilver(pawn.Map, Util_Spaceship.medicalSupplyCostInSilver))
                {
                    TradeUtility.LaunchSilver(pawn.Map, Util_Spaceship.medicalSupplyCostInSilver);
                    buildingSpaceshipMedical.Notify_PawnBoarding(resultingThing as Pawn, false);
                }
                else
                {
                    Messages.Message(
                        "MCS.cannotboardmedicsilver".Translate((resultingThing as Pawn)?.Name.ToStringShort,
                            pawn.gender.GetPossessive()),
                        resultingThing, MessageTypeDefOf.RejectInput);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}