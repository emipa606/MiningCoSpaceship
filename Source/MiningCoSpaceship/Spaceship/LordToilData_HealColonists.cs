using Verse;
using Verse.AI.Group;

namespace Spaceship;

public class LordToilData_HealColonists : LordToilData
{
    public IntVec3 spaceshipPosition;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref spaceshipPosition, "spaceshipPosition");
    }
}