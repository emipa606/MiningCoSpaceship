using Verse;

namespace Spaceship;

public static class Util_PawnKindDefOf
{
    public static PawnKindDef Technician => PawnKindDef.Named("MiningCoTechnician");

    public static PawnKindDef Miner => PawnKindDef.Named("MiningCoMiner");

    public static PawnKindDef Geologist => PawnKindDef.Named("MiningCoGeologist");

    public static PawnKindDef Medic => PawnKindDef.Named("MiningCoMedic");

    public static PawnKindDef Pilot => PawnKindDef.Named("MiningCoPilot");

    public static PawnKindDef Scout => PawnKindDef.Named("MiningCoScout");

    public static PawnKindDef Guard => PawnKindDef.Named("MiningCoGuard");

    public static PawnKindDef ShockTrooper => PawnKindDef.Named("MiningCoShockTrooper");

    public static PawnKindDef HeavyGuard => PawnKindDef.Named("MiningCoHeavyGuard");

    public static PawnKindDef Officer => PawnKindDef.Named("MiningCoOfficer");
}