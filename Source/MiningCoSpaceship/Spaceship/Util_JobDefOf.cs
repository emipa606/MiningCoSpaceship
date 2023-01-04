using Verse;

namespace Spaceship;

public static class Util_JobDefOf
{
    public static JobDef UseOrbitalRelayConsole => DefDatabase<JobDef>.GetNamed("JobDef_UseOrbitalRelayConsole");

    public static JobDef TradeWithCargoSpaceship => DefDatabase<JobDef>.GetNamed("JobDef_TradeWithCargoSpaceship");

    public static JobDef RequestSpaceshipTakeOff => DefDatabase<JobDef>.GetNamed("JobDef_RequestSpaceshipTakeOff");

    public static JobDef BoardSpaceship => DefDatabase<JobDef>.GetNamed("JobDef_BoardSpaceship");

    public static JobDef CarryDownedPawn => DefDatabase<JobDef>.GetNamed("JobDef_CarryDownedPawn");

    public static JobDef BoardMedicalSpaceship => DefDatabase<JobDef>.GetNamed("JobDef_BoardMedicalSpaceship");

    public static JobDef TransferToMedibay => DefDatabase<JobDef>.GetNamed("JobDef_TransferToMedibay");
}