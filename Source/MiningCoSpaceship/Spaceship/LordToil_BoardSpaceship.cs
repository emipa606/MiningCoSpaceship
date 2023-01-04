using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class LordToil_BoardSpaceship : LordToil
{
    public LordToil_BoardSpaceship(IntVec3 boardCell, LocomotionUrgency locomotion = LocomotionUrgency.Walk)
    {
        data = new LordToilData_BoardSpaceship();
        Data.boardCell = boardCell;
        Data.locomotion = locomotion;
    }

    public override bool AllowSatisfyLongNeeds => false;

    protected LordToilData_BoardSpaceship Data => (LordToilData_BoardSpaceship)data;

    public override void UpdateAllDuties()
    {
        var lordToilData_BoardSpaceship = Data;
        foreach (var pawn in lord.ownedPawns)
        {
            var pawnDuty = new PawnDuty(Util_DutyDefOf.DutyBoardSpaceship)
            {
                locomotion = lordToilData_BoardSpaceship.locomotion,
                focus = lordToilData_BoardSpaceship.boardCell
            };
            pawn.mindState.duty = pawnDuty;
        }
    }
}