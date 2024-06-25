using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public abstract class LordJob_MiningCoBase : LordJob
{
    public const int pawnExitedGoodwillImpact = 1;

    public const int pawnLostGoodwillImpact = -3;

    public LocomotionUrgency locomotionUrgency;

    public IntVec3 targetDestination;

    public LordJob_MiningCoBase()
    {
    }

    public LordJob_MiningCoBase(IntVec3 targetDestination, LocomotionUrgency locomotionUrgency = LocomotionUrgency.Walk)
    {
        this.targetDestination = targetDestination;
        this.locomotionUrgency = locomotionUrgency;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref targetDestination, "targetDestination");
        Scribe_Values.Look(ref locomotionUrgency, "locomotionUrgency");
    }

    public override StateGraph CreateGraph()
    {
        return null;
    }

    public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
    {
        base.Notify_PawnLost(p, condition);
        switch (condition)
        {
            case PawnLostCondition.Incapped:
                Messages.Message("MCS.employeelost".Translate(pawnLostGoodwillImpact),
                    new TargetInfo(p.Position, Map), MessageTypeDefOf.NegativeHealthEvent);
                Util_Faction.AffectGoodwillWith(Util_Faction.MiningCoFaction, Faction.OfPlayer, pawnLostGoodwillImpact);
                break;
            case PawnLostCondition.ChangedFaction:
                Util_Faction.AffectGoodwillWith(Util_Faction.MiningCoFaction, Faction.OfPlayer,
                    pawnExitedGoodwillImpact);
                break;
        }
    }
}