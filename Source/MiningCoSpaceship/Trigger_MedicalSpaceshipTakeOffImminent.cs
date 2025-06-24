using Verse;
using Verse.AI.Group;

namespace Spaceship;

public class Trigger_MedicalSpaceshipTakeOffImminent : Trigger
{
    private const int CheckInterval = 305;

    public override bool ActivateOn(Lord lord, TriggerSignal signal)
    {
        if (signal.type != TriggerSignalType.Tick || Find.TickManager.TicksGame % CheckInterval != 0)
        {
            return false;
        }

        foreach (var thing in lord.Map.listerThings.ThingsOfDef(Util_Spaceship.SpaceshipMedical))
        {
            var item = (Building)thing;
            if (item is Building_SpaceshipMedical building_SpaceshipMedical &&
                building_SpaceshipMedical.IsTakeOffImminent(Util_Spaceship.medicsRecallBeforeTakeOffMarginInTicks))
            {
                return true;
            }
        }

        return false;
    }
}