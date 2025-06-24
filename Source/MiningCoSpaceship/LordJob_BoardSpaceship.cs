using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class LordJob_BoardSpaceship : LordJob_MiningCoBase
{
    public LordJob_BoardSpaceship()
    {
    }

    public LordJob_BoardSpaceship(IntVec3 targetDestination,
        LocomotionUrgency locomotionUrgency = LocomotionUrgency.Walk)
        : base(targetDestination, locomotionUrgency)
    {
    }

    public override StateGraph CreateGraph()
    {
        var stateGraph = new StateGraph();
        var lordToilTravel = (LordToil_Travel)(stateGraph.StartingToil = new LordToil_Travel(targetDestination));
        var lordToilDefendPoint = new LordToil_DefendPoint(false);
        stateGraph.AddToil(lordToilDefendPoint);
        var lordToilBoardSpaceship = new LordToil_BoardSpaceship(targetDestination, locomotionUrgency);
        stateGraph.AddToil(lordToilBoardSpaceship);
        var lordToilEscortDownedPawn = new LordToil_EscortDownedPawn(targetDestination, locomotionUrgency);
        stateGraph.AddToil(lordToilEscortDownedPawn);
        var startingToil = stateGraph.AttachSubgraph(new LordJob_ExitMap(IntVec3.Invalid).CreateGraph()).StartingToil;
        stateGraph.AddTransition(new Transition(lordToilTravel, lordToilDefendPoint)
        {
            triggers = { new Trigger_PawnHarmed() },
            preActions = { new TransitionAction_SetDefendLocalGroup() },
            postActions = { new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToilDefendPoint, lordToilTravel)
        {
            triggers = { new Trigger_TicksPassedWithoutHarm(1200) }
        });
        stateGraph.AddTransition(new Transition(lordToilTravel, lordToilEscortDownedPawn)
        {
            triggers = { new Trigger_ReachableDownedPawn() },
            postActions = { new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToilEscortDownedPawn, lordToilTravel)
        {
            triggers = { new Trigger_Memo("RescueEnded") }
        });
        stateGraph.AddTransition(new Transition(lordToilTravel, lordToilBoardSpaceship)
        {
            triggers = { new Trigger_Memo("TravelArrived") },
            postActions = { new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToilTravel, startingToil)
        {
            sources =
            {
                lordToilEscortDownedPawn,
                lordToilBoardSpaceship
            },
            triggers = { new Trigger_PawnCannotReachTargetDestination() },
            postActions =
            {
                new TransitionAction_CancelDispatcherPick(),
                new TransitionAction_EndAllJobs()
            }
        });
        stateGraph.AddTransition(new Transition(lordToilEscortDownedPawn, startingToil)
        {
            sources = { lordToilBoardSpaceship },
            triggers = { new Trigger_SpaceshipNotFound() },
            postActions =
            {
                new TransitionAction_CancelDispatcherPick(),
                new TransitionAction_EndAllJobs()
            }
        });
        return stateGraph;
    }
}