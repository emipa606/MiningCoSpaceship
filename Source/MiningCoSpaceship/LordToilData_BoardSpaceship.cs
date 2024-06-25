using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class LordToilData_BoardSpaceship : LordToilData
{
    public IntVec3 boardCell;

    public LocomotionUrgency locomotion;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref boardCell, "boardCell");
        Scribe_Values.Look(ref locomotion, "locomotion");
    }
}