using Verse;
using Verse.AI;

namespace Spaceship;

public static class Util_DutyDefOf
{
    public static DutyDef DutyBoardSpaceship => DefDatabase<DutyDef>.GetNamed("DutyDef_BoardSpaceship");

    public static DutyDef CarryDownedPawn => DefDatabase<DutyDef>.GetNamed("DutyDef_CarryDownedPawn");

    public static DutyDef EscortCarrier => DefDatabase<DutyDef>.GetNamed("DutyDef_EscortCarrier");

    public static DutyDef HealColonists => DefDatabase<DutyDef>.GetNamed("DutyDef_HealColonists");
}