using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class LordJob_ExitMap : LordJob_MiningCoBase
{
    private bool impactMessageIsSent;

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
        var lordToilTravel = new LordToil_Travel(targetDestination);
        stateGraph.AddToil(lordToilTravel);
        var lordToilDefendPoint = new LordToil_DefendPoint(false);
        stateGraph.AddToil(lordToilDefendPoint);
        var lordToilExitMap = new LordToil_ExitMap(LocomotionUrgency.Walk);
        stateGraph.AddToil(lordToilExitMap);
        var lordToilEscortDownedPawn = new LordToil_EscortDownedPawn(targetDestination, LocomotionUrgency.Walk);
        stateGraph.AddToil(lordToilEscortDownedPawn);
        var lordToilExitMap2 = new LordToil_ExitMap(LocomotionUrgency.Jog, true);
        stateGraph.AddToil(lordToilExitMap2);
        var lordToilDefendPoint2 = new LordToil_DefendPoint(false);
        stateGraph.AddToil(lordToilDefendPoint2);
        var lordToilEscortDownedPawn2 = new LordToil_EscortDownedPawn(targetDestination, LocomotionUrgency.Jog);
        stateGraph.AddToil(lordToilEscortDownedPawn2);
        stateGraph.AddTransition(new Transition(firstSource, lordToilTravel)
        {
            triggers = { new Trigger_TicksPassedWithoutHarm(1) },
            preActions = { new TransitionAction_CheckExitSpotIsValid() },
            postActions = { new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToilTravel, lordToilDefendPoint)
        {
            triggers = { new Trigger_PawnHarmed() },
            preActions = { new TransitionAction_SetDefendLocalGroup() },
            postActions = { new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToilDefendPoint, lordToilTravel)
        {
            triggers = { new Trigger_TicksPassedWithoutHarm(1200) },
            preActions = { new TransitionAction_EnsureHaveExitDestination() }
        });
        stateGraph.AddTransition(new Transition(lordToilTravel, lordToilEscortDownedPawn)
        {
            sources = { lordToilExitMap },
            triggers = { new Trigger_ReachableDownedPawn() },
            preActions = { new TransitionAction_CheckExitSpotIsValid() },
            postActions = { new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToilEscortDownedPawn, lordToilTravel)
        {
            triggers = { new Trigger_Memo("RescueEnded") }
        });
        stateGraph.AddTransition(new Transition(lordToilTravel, lordToilExitMap)
        {
            triggers = { new Trigger_Memo("TravelArrived") },
            postActions = { new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToilTravel, lordToilExitMap2)
        {
            sources =
            {
                lordToilExitMap,
                lordToilEscortDownedPawn
            },
            triggers =
            {
                new Trigger_PawnCannotReachTargetDestination(),
                new Trigger_HostileToColony()
            },
            postActions = { new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToilExitMap2, lordToilDefendPoint2)
        {
            triggers = { new Trigger_PawnHarmed() },
            preActions = { new TransitionAction_SetDefendLocalGroup() },
            postActions = { new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToilDefendPoint2, lordToilExitMap2)
        {
            triggers = { new Trigger_TicksPassedWithoutHarm(1200) }
        });
        stateGraph.AddTransition(new Transition(lordToilExitMap2, lordToilEscortDownedPawn2)
        {
            triggers = { new Trigger_ReachableDownedPawn() },
            preActions = { new TransitionAction_CheckExitSpotIsValid() },
            postActions = { new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToilEscortDownedPawn2, lordToilExitMap2)
        {
            triggers = { new Trigger_Memo("RescueEnded") }
        });
        return stateGraph;
    }
}