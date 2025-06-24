using RimWorld;
using Verse;

namespace Spaceship;

public static class Util_Spaceship
{
    public const int cargoSupplyCostInSilver = 1500;

    public const int medicalSupplyCostInSilver = 1000;

    public const int orbitalHealingCost = 250;

    public const int feePerPawnInSilver = 40;

    public const int medicsRecallBeforeTakeOffMarginInTicks = 15000;

    private const int CargoPeriodicSupplyLandingDuration = 30000;

    private const int CargoRequestedSupplyLandingDuration = 60000;

    private const int DispatcherDropDurationInTicks = 5000;

    private const int DispatcherPickDurationInTicks = 120000;

    private const int MedicalSupplyLandingDuration = 60000;

    private static IntRange damagedSpaceshipLandingDuration = new(480000, 720000);

    private static ThingDef SpaceshipLanding => ThingDef.Named("FlyingSpaceshipLanding");

    public static ThingDef SpaceshipTakingOff => ThingDef.Named("FlyingSpaceshipTakingOff");

    public static ThingDef SpaceshipCargo => ThingDef.Named("SpaceshipCargo");

    public static ThingDef SpaceshipDamaged => ThingDef.Named("SpaceshipDamaged");

    public static ThingDef SpaceshipDispatcherDrop => ThingDef.Named("SpaceshipDispatcherDrop");

    public static ThingDef SpaceshipDispatcherPick => ThingDef.Named("SpaceshipDispatcherPick");

    public static ThingDef SpaceshipMedical => ThingDef.Named("SpaceshipMedical");

    private static ThingDef SpaceshipAirstrike => ThingDef.Named("FlyingSpaceshipAirstrike");

    public static FlyingSpaceshipLanding SpawnLandingSpaceship(Building_LandingPad landingPad,
        SpaceshipKind spaceshipKind)
    {
        var orbitalRelay = Util_OrbitalRelay.GetOrbitalRelay(landingPad.Map);
        var landingDuration = 0;
        switch (spaceshipKind)
        {
            case SpaceshipKind.CargoPeriodic:
                landingDuration = CargoPeriodicSupplyLandingDuration;
                orbitalRelay?.Notify_CargoSpaceshipPeriodicLanding();
                Util_Misc.Partnership.nextPeriodicSupplyTick[landingPad.Map] = Find.TickManager.TicksGame + 600000;
                Messages.Message("MCS.cargoshiplanding".Translate(),
                    new TargetInfo(landingPad.Position, landingPad.Map), MessageTypeDefOf.NeutralEvent);
                break;
            case SpaceshipKind.CargoRequested:
                landingDuration = CargoRequestedSupplyLandingDuration;
                orbitalRelay?.Notify_CargoSpaceshipRequestedLanding();
                Util_Misc.Partnership.nextRequestedSupplyMinTick[landingPad.Map] = Find.TickManager.TicksGame + 300000;
                Messages.Message("MCS.cargoshiplanding".Translate(),
                    new TargetInfo(landingPad.Position, landingPad.Map), MessageTypeDefOf.NeutralEvent);
                break;
            case SpaceshipKind.Damaged:
                landingDuration = damagedSpaceshipLandingDuration.RandomInRange;
                break;
            case SpaceshipKind.DispatcherDrop:
                landingDuration = DispatcherDropDurationInTicks;
                Messages.Message("MCS.dispatchdrop".Translate(),
                    new TargetInfo(landingPad.Position, landingPad.Map), MessageTypeDefOf.NeutralEvent);
                break;
            case SpaceshipKind.DispatcherPick:
                landingDuration = DispatcherPickDurationInTicks;
                Messages.Message("MCS.dispatchdrop".Translate(),
                    new TargetInfo(landingPad.Position, landingPad.Map), MessageTypeDefOf.NeutralEvent);
                break;
            case SpaceshipKind.Medical:
                landingDuration = MedicalSupplyLandingDuration;
                orbitalRelay?.Notify_MedicalSpaceshipLanding();
                Util_Misc.Partnership.nextMedicalSupplyMinTick[landingPad.Map] = Find.TickManager.TicksGame + 300000;
                Messages.Message("MCS.medicshiplanding".Translate(),
                    new TargetInfo(landingPad.Position, landingPad.Map), MessageTypeDefOf.NeutralEvent);
                break;
            default:
                Log.ErrorOnce($"MiningCo. Spaceship: unhandled SpaceshipKind ({spaceshipKind}).", 123456780);
                break;
        }

        var flyingSpaceshipLanding = ThingMaker.MakeThing(SpaceshipLanding) as FlyingSpaceshipLanding;
        GenSpawn.Spawn(flyingSpaceshipLanding, landingPad.Position, landingPad.Map, landingPad.Rotation);
        flyingSpaceshipLanding?.InitializeLandingParameters(landingPad, landingDuration, spaceshipKind);
        return flyingSpaceshipLanding;
    }

    public static void SpawnStrikeShip(Map map, IntVec3 targetPosition, AirstrikeDef airStrikeDef, Faction faction)
    {
        var flyingSpaceshipAirstrike = ThingMaker.MakeThing(SpaceshipAirstrike) as FlyingSpaceshipAirstrike;
        GenSpawn.Spawn(flyingSpaceshipAirstrike, targetPosition, map);
        flyingSpaceshipAirstrike?.InitializeAirstrikeData(targetPosition, airStrikeDef, faction);
    }
}