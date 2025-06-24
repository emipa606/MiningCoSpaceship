using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Spaceship;

public class Trigger_HostileToColony : Trigger
{
    private const int CheckInterval = 62;

    public override bool ActivateOn(Lord lord, TriggerSignal signal)
    {
        return signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame % CheckInterval == 0 &&
               lord.faction.HostileTo(Faction.OfPlayer);
    }
}