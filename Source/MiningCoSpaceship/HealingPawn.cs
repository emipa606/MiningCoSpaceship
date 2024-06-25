using Verse;

namespace Spaceship;

public class HealingPawn(Pawn pawn, Map originMap, int healEndTicks)
{
    public readonly int healEndTick = healEndTicks;

    public readonly Map originMap = originMap;
    public readonly Pawn pawn = pawn;
}