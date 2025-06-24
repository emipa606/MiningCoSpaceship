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

    private LordToilData_BoardSpaceship Data => (LordToilData_BoardSpaceship)data;

    public override void UpdateAllDuties()
    {
        var lordToilDataBoardSpaceship = Data;
        foreach (var pawn in lord.ownedPawns)
        {
            var pawnDuty = new PawnDuty(Util_DutyDefOf.DutyDef_BoardSpaceship)
            {
                locomotion = lordToilDataBoardSpaceship.locomotion,
                focus = lordToilDataBoardSpaceship.boardCell
            };
            pawn.mindState.duty = pawnDuty;
        }
    }
}