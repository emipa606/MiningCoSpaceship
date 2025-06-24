using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public abstract class LordJob_MiningCoBase : LordJob
{
    private const int PawnExitedGoodwillImpact = 1;

    private const int PawnLostGoodwillImpact = -3;

    protected LocomotionUrgency locomotionUrgency;

    public IntVec3 targetDestination;

    protected LordJob_MiningCoBase()
    {
    }

    protected LordJob_MiningCoBase(IntVec3 targetDestination,
        LocomotionUrgency locomotionUrgency = LocomotionUrgency.Walk)
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
                Messages.Message("MCS.employeelost".Translate(PawnLostGoodwillImpact),
                    new TargetInfo(p.Position, Map), MessageTypeDefOf.NegativeHealthEvent);
                Util_Faction.AffectGoodwillWith(Util_Faction.MiningCoFaction, Faction.OfPlayer, PawnLostGoodwillImpact);
                break;
            case PawnLostCondition.ChangedFaction:
                Util_Faction.AffectGoodwillWith(Util_Faction.MiningCoFaction, Faction.OfPlayer,
                    PawnExitedGoodwillImpact);
                break;
        }
    }
}