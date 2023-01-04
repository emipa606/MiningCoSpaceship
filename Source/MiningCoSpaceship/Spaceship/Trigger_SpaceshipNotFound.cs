using Verse;
using Verse.AI.Group;

namespace Spaceship;

public class Trigger_SpaceshipNotFound : Trigger
{
    public const int checkInterval = 64;

    public override bool ActivateOn(Lord lord, TriggerSignal signal)
    {
        if (signal.type != TriggerSignalType.Tick || Find.TickManager.TicksGame % checkInterval != 0)
        {
            return false;
        }

        var thinglist = (lord.LordJob as LordJob_MiningCoBase)?.targetDestination.GetThingList(lord.Map);
        if (thinglist == null)
        {
            return true;
        }

        foreach (var thing in thinglist)
        {
            if (thing is Building_Spaceship)
            {
                return false;
            }
        }

        return true;
    }
}