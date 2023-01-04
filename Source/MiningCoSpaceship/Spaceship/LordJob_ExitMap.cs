using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class LordJob_ExitMap : LordJob_MiningCoBase
{
    public bool impactMessageIsSent;

    public LordJob_ExitMap()
    {
    }

    public LordJob_ExitMap(IntVec3 targetDestination)
        : base(targetDestination)
    {
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref impactMessageIsSent, "impactMessageIsSent");
    }

    public override StateGraph CreateGraph()
    {
        var stateGraph = new StateGraph();
        var firstSource = (LordToil_DefendPoint)(stateGraph.StartingToil = new LordToil_DefendPoint());
        var lordToil_Travel = new LordToil_Travel(targetDestination);
        stateGraph.AddToil(lordToil_Travel);
        var lordToil_DefendPoint = new LordToil_DefendPoint(false);
        stateGraph.AddToil(lordToil_DefendPoint);
        var lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.Walk);
        stateGraph.AddToil(lordToil_ExitMap);
        var lordToil_EscortDownedPawn = new LordToil_EscortDownedPawn(targetDestination, LocomotionUrgency.Walk);
        stateGraph.AddToil(lordToil_EscortDownedPawn);
        var lordToil_ExitMap2 = new LordToil_ExitMap(LocomotionUrgency.Jog, true);
        stateGraph.AddToil(lordToil_ExitMap2);
        var lordToil_DefendPoint2 = new LordToil_DefendPoint(false);
        stateGraph.AddToil(lordToil_DefendPoint2);
        var lordToil_EscortDownedPawn2 = new LordToil_EscortDownedPawn(targetDestination, LocomotionUrgency.Jog);
        stateGraph.AddToil(lordToil_EscortDownedPawn2);
        stateGraph.AddTransition(new Transition(firstSource, lordToil_Travel)
        {
            triggers = { (Trigger)new Trigger_TicksPassedWithoutHarm(1) },
            preActions = { (TransitionAction)new TransitionAction_CheckExitSpotIsValid() },
            postActions = { (TransitionAction)new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToil_Travel, lordToil_DefendPoint)
        {
            triggers = { (Trigger)new Trigger_PawnHarmed() },
            preActions = { (TransitionAction)new TransitionAction_SetDefendLocalGroup() },
            postActions = { (TransitionAction)new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToil_DefendPoint, lordToil_Travel)
        {
            triggers = { (Trigger)new Trigger_TicksPassedWithoutHarm(1200) },
            preActions = { (TransitionAction)new TransitionAction_EnsureHaveExitDestination() }
        });
        stateGraph.AddTransition(new Transition(lordToil_Travel, lordToil_EscortDownedPawn)
        {
            sources = { (LordToil)lordToil_ExitMap },
            triggers = { (Trigger)new Trigger_ReachableDownedPawn() },
            preActions = { (TransitionAction)new TransitionAction_CheckExitSpotIsValid() },
            postActions = { (TransitionAction)new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToil_EscortDownedPawn, lordToil_Travel)
        {
            triggers = { (Trigger)new Trigger_Memo("RescueEnded") }
        });
        stateGraph.AddTransition(new Transition(lordToil_Travel, lordToil_ExitMap)
        {
            triggers = { (Trigger)new Trigger_Memo("TravelArrived") },
            postActions = { (TransitionAction)new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToil_Travel, lordToil_ExitMap2)
        {
            sources =
            {
                (LordToil)lordToil_ExitMap,
                (LordToil)lordToil_EscortDownedPawn
            },
            triggers =
            {
                (Trigger)new Trigger_PawnCannotReachTargetDestination(),
                (Trigger)new Trigger_HostileToColony()
            },
            postActions = { (TransitionAction)new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToil_ExitMap2, lordToil_DefendPoint2)
        {
            triggers = { (Trigger)new Trigger_PawnHarmed() },
            preActions = { (TransitionAction)new TransitionAction_SetDefendLocalGroup() },
            postActions = { (TransitionAction)new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToil_DefendPoint2, lordToil_ExitMap2)
        {
            triggers = { (Trigger)new Trigger_TicksPassedWithoutHarm(1200) }
        });
        stateGraph.AddTransition(new Transition(lordToil_ExitMap2, lordToil_EscortDownedPawn2)
        {
            triggers = { (Trigger)new Trigger_ReachableDownedPawn() },
            preActions = { (TransitionAction)new TransitionAction_CheckExitSpotIsValid() },
            postActions = { (TransitionAction)new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToil_EscortDownedPawn2, lordToil_ExitMap2)
        {
            triggers = { (Trigger)new Trigger_Memo("RescueEnded") }
        });
        return stateGraph;
    }
}