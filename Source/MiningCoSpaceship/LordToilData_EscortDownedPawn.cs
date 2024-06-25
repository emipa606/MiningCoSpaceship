using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class LordToilData_EscortDownedPawn : LordToilData
{
    public Pawn carrier;

    public LocomotionUrgency locomotion;
    public IntVec3 targetDestination;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref targetDestination, "targetDestination");
        Scribe_Values.Look(ref locomotion, "locomotion");
        Scribe_References.Look(ref carrier, "carrier");
    }
}