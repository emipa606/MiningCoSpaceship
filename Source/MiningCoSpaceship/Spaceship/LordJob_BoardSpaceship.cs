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
        var lordToil_Travel = (LordToil_Travel)(stateGraph.StartingToil = new LordToil_Travel(targetDestination));
        var lordToil_DefendPoint = new LordToil_DefendPoint(false);
        stateGraph.AddToil(lordToil_DefendPoint);
        var lordToil_BoardSpaceship = new LordToil_BoardSpaceship(targetDestination, locomotionUrgency);
        stateGraph.AddToil(lordToil_BoardSpaceship);
        var lordToil_EscortDownedPawn = new LordToil_EscortDownedPawn(targetDestination, locomotionUrgency);
        stateGraph.AddToil(lordToil_EscortDownedPawn);
        var startingToil = stateGraph.AttachSubgraph(new LordJob_ExitMap(IntVec3.Invalid).CreateGraph()).StartingToil;
        stateGraph.AddTransition(new Transition(lordToil_Travel, lordToil_DefendPoint)
        {
            triggers = { (Trigger)new Trigger_PawnHarmed() },
            preActions = { (TransitionAction)new TransitionAction_SetDefendLocalGroup() },
            postActions = { (TransitionAction)new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToil_DefendPoint, lordToil_Travel)
        {
            triggers = { (Trigger)new Trigger_TicksPassedWithoutHarm(1200) }
        });
        stateGraph.AddTransition(new Transition(lordToil_Travel, lordToil_EscortDownedPawn)
        {
            triggers = { (Trigger)new Trigger_ReachableDownedPawn() },
            postActions = { (TransitionAction)new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToil_EscortDownedPawn, lordToil_Travel)
        {
            triggers = { (Trigger)new Trigger_Memo("RescueEnded") }
        });
        stateGraph.AddTransition(new Transition(lordToil_Travel, lordToil_BoardSpaceship)
        {
            triggers = { (Trigger)new Trigger_Memo("TravelArrived") },
            postActions = { (TransitionAction)new TransitionAction_EndAllJobs() }
        });
        stateGraph.AddTransition(new Transition(lordToil_Travel, startingToil)
        {
            sources =
            {
                (LordToil)lordToil_EscortDownedPawn,
                (LordToil)lordToil_BoardSpaceship
            },
            triggers = { (Trigger)new Trigger_PawnCannotReachTargetDestination() },
            postActions =
            {
                (TransitionAction)new TransitionAction_CancelDispatcherPick(),
                (TransitionAction)new TransitionAction_EndAllJobs()
            }
        });
        stateGraph.AddTransition(new Transition(lordToil_EscortDownedPawn, startingToil)
        {
            sources = { (LordToil)lordToil_BoardSpaceship },
            triggers = { (Trigger)new Trigger_SpaceshipNotFound() },
            postActions =
            {
                (TransitionAction)new TransitionAction_CancelDispatcherPick(),
                (TransitionAction)new TransitionAction_EndAllJobs()
            }
        });
        return stateGraph;
    }
}