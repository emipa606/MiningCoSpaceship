using Verse;
using Verse.AI.Group;

namespace Spaceship;

public class Trigger_SpaceshipNotFound : Trigger
{
    private const int CheckInterval = 64;

    public override bool ActivateOn(Lord lord, TriggerSignal signal)
    {
        if (signal.type != TriggerSignalType.Tick || Find.TickManager.TicksGame % CheckInterval != 0)
        {
            return false;
        }

        var thingList = (lord.LordJob as LordJob_MiningCoBase)?.targetDestination.GetThingList(lord.Map);
        if (thingList == null)
        {
            return true;
        }

        foreach (var thing in thingList)
        {
            if (thing is Building_Spaceship)
            {
                return false;
            }
        }

        return true;
    }
}