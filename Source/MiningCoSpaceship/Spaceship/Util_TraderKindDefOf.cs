using RimWorld;
using Verse;

namespace Spaceship;

public static class Util_TraderKindDefOf
{
    public static TraderKindDef spaceshipCargoPeriodicSupply =>
        DefDatabase<TraderKindDef>.GetNamed("SpaceshipCargoPeriodicSupply");

    public static TraderKindDef spaceshipCargoRequestedSupply =>
        DefDatabase<TraderKindDef>.GetNamed("SpaceshipCargoRequestedSupply");

    public static TraderKindDef spaceshipCargoDamaged => DefDatabase<TraderKindDef>.GetNamed("SpaceshipCargoDamaged");

    public static TraderKindDef spaceshipCargoDispatcher =>
        DefDatabase<TraderKindDef>.GetNamed("SpaceshipCargoDispatcher");

    public static TraderKindDef spaceshipCargoMedical => DefDatabase<TraderKindDef>.GetNamed("SpaceshipCargoMedical");
}