using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class LordToil_EscortDownedPawn : LordToil
{
    public LordToil_EscortDownedPawn(IntVec3 travelDest, LocomotionUrgency locomotion)
    {
        data = new LordToilData_EscortDownedPawn();
        Data.targetDestination = travelDest;
        Data.locomotion = locomotion;
    }

    public LordToilData_EscortDownedPawn Data => (LordToilData_EscortDownedPawn)data;

    public override bool AllowSatisfyLongNeeds => false;

    public void SetDestination(IntVec3 newTargetDestination)
    {
        Data.targetDestination = newTargetDestination;
    }

    public override void UpdateAllDuties()
    {
        if (Data.carrier == null)
        {
            Data.carrier = lord.ownedPawns.RandomElement();
        }

        foreach (var pawn in lord.ownedPawns)
        {
            if (pawn == Data.carrier)
            {
                var duty = new PawnDuty(Util_DutyDefOf.CarryDownedPawn, Data.targetDestination);
                pawn.mindState.duty = duty;
            }
            else
            {
                var duty2 = new PawnDuty(Util_DutyDefOf.EscortCarrier, Data.carrier, 5f);
                pawn.mindState.duty = duty2;
            }
        }
    }

    public override void LordToilTick()
    {
        base.LordToilTick();
        if (Find.TickManager.TicksGame % 120 != 0)
        {
            return;
        }

        var carrier = Data.carrier;
        if (carrier.DestroyedOrNull() || carrier.Dead || carrier.Downed)
        {
            Notify_RescueEnded();
        }
    }

    public void Notify_RescueEnded()
    {
        Data.carrier = null;
        lord.ReceiveMemo("RescueEnded");
    }
}