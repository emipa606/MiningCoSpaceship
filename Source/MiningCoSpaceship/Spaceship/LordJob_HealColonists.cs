using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class LordJob_HealColonists : LordJob_MiningCoBase
{
    public LordJob_HealColonists()
    {
    }

    public LordJob_HealColonists(IntVec3 targetDestination)
        : base(targetDestination)
    {
    }

    public override StateGraph CreateGraph()
    {
        var stateGraph = new StateGraph();
        var firstSource =
            (LordToil_HealColonists)(stateGraph.StartingToil = new LordToil_HealColonists(targetDestination));
        var startingToil = stateGraph
            .AttachSubgraph(new LordJob_BoardSpaceship(targetDestination, LocomotionUrgency.Jog).CreateGraph())
            .StartingToil;
        stateGraph.AddTransition(new Transition(firstSource, startingToil)
        {
            triggers =
            {
                (Trigger)new Trigger_PawnHarmed(),
                (Trigger)new Trigger_PawnLostViolently(),
                (Trigger)new Trigger_Memo("HealFinished"),
                (Trigger)new Trigger_Memo("TakeOffImminent"),
                (Trigger)new Trigger_MedicalSpaceshipTakeOffImminent()
            },
            postActions =
            {
                (TransitionAction)new TransitionAction_CancelMedicalAssistance(),
                (TransitionAction)new TransitionAction_EndAllJobs()
            }
        });
        return stateGraph;
    }

    public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
    {
        base.Notify_PawnLost(p, condition);
        if (condition != PawnLostCondition.Incapped || p.kindDef != Util_PawnKindDefOf.Medic)
        {
            return;
        }

        Util_Faction.AffectGoodwillWith(Util_Faction.MiningCoFaction, Faction.OfPlayer, -5);
        var text = "MCS.medicinjured".Translate();
        Find.LetterStack.ReceiveLetter("MCS.medicinjuredtitle".Translate(), text, LetterDefOf.NegativeEvent);
    }
}