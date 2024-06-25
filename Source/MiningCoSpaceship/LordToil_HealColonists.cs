using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class LordToil_HealColonists : LordToil
{
    public LordToil_HealColonists(IntVec3 spaceshipPosition)
    {
        data = new LordToilData_HealColonists();
        Data.spaceshipPosition = spaceshipPosition;
    }

    public LordToilData_HealColonists Data => (LordToilData_HealColonists)data;

    public override bool AllowSatisfyLongNeeds => false;

    public override void UpdateAllDuties()
    {
        foreach (var ownedPawn in lord.ownedPawns)
        {
            var duty = new PawnDuty(Util_DutyDefOf.DutyDef_HealColonists);
            ownedPawn.mindState.duty = duty;
            ownedPawn.mindState.duty.focus = Data.spaceshipPosition;
        }
    }
}