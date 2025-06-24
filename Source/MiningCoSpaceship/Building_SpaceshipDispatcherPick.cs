using RimWorld;
using Verse;

namespace Spaceship;

public class Building_SpaceshipDispatcherPick : Building_SpaceshipDispatcher
{
    protected override bool takeOffRequestIsEnabled => true;

    public void InitializeData_DispatcherPick(Faction faction, int hitPoints, int landingDuration,
        SpaceshipKind kind)
    {
        InitializeData(faction, hitPoints, landingDuration, kind);
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        if (mode != DestroyMode.KillFinalize && pawnsAboard.Count > 2)
        {
            var num = pawnsAboard.Count - 2;
            SpawnPayment(num);
            Util_Faction.AffectGoodwillWith(Util_Faction.MiningCoFaction, Faction.OfPlayer, num);
        }

        base.Destroy(mode);
    }

    public override void Notify_PawnBoarding(Pawn pawn, bool isLastLordPawn)
    {
        base.Notify_PawnBoarding(pawn, isLastLordPawn);
        if (isLastLordPawn)
        {
            takeOffTick = Find.TickManager.TicksGame + 600;
        }
    }
}