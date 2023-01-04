using Verse;

namespace Spaceship;

public class HealingPawn
{
    public int healEndTick;

    public Map originMap;
    public Pawn pawn;

    public HealingPawn(Pawn pawn, Map originMap, int healEndTicks)
    {
        this.pawn = pawn;
        this.originMap = originMap;
        healEndTick = healEndTicks;
    }
}