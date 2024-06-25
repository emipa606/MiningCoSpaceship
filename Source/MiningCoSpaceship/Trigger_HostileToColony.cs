using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Spaceship;

public class Trigger_HostileToColony : Trigger
{
    public const int checkInterval = 62;

    public override bool ActivateOn(Lord lord, TriggerSignal signal)
    {
        return signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame % checkInterval == 0 &&
               lord.faction.HostileTo(Faction.OfPlayer);
    }
}