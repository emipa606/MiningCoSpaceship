using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Spaceship;

public class JobDriver_BoardMedicalSpaceship : JobDriver
{
    private readonly TargetIndex medicalSpaceshipIndex = TargetIndex.A;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var medicalSpaceship = TargetThingA as Building_SpaceshipMedical;
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch)
            .FailOn(() => medicalSpaceship.DestroyedOrNull());
        yield return Toils_General.Wait(300).WithProgressBarToilDelay(medicalSpaceshipIndex)
            .FailOn(() => medicalSpaceship.DestroyedOrNull());
        yield return new Toil
        {
            initAction = delegate
            {
                if (medicalSpaceship is { orbitalHealingPawnsAboardCount: >= 6 })
                {
                    Messages.Message(
                        "MCS.cannotboardmedic".Translate(pawn.Name.ToStringShort),
                        pawn, MessageTypeDefOf.RejectInput);
                }
                else if (TradeUtility.ColonyHasEnoughSilver(pawn.Map, Util_Spaceship.medicalSupplyCostInSilver))
                {
                    TradeUtility.LaunchSilver(Map, Util_Spaceship.medicalSupplyCostInSilver);
                    medicalSpaceship?.Notify_PawnBoarding(pawn, false);
                }
                else
                {
                    Messages.Message(
                        "MCS.cannotboardmedicsilver".Translate(pawn.Name.ToStringShort, pawn.gender.GetPossessive()),
                        pawn, MessageTypeDefOf.RejectInput);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}