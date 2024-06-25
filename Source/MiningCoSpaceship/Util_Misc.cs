using System;
using RimWorld;
using Verse;

namespace Spaceship;

public static class Util_Misc
{
    public static WorldComponent_Partnership Partnership
    {
        get
        {
            if (Find.World.GetComponent(typeof(WorldComponent_Partnership)) is WorldComponent_Partnership result)
            {
                return result;
            }

            Log.ErrorOnce("MiningCo. Spaceship: did not found WorldComponent_Partnership.", 123456788);
            return null;
        }
    }

    public static WorldComponent_OrbitalHealing OrbitalHealing
    {
        get
        {
            if (Find.World.GetComponent(typeof(WorldComponent_OrbitalHealing)) is WorldComponent_OrbitalHealing result)
            {
                return result;
            }

            Log.ErrorOnce("MiningCo. Spaceship: did not found WorldComponent_OrbitalHealing.", 123456787);
            return null;
        }
    }

    public static void SelectAirstrikeTarget(Map map, Action<LocalTargetInfo> actionOnValidTarget)
    {
        var targetingParameters = new TargetingParameters
        {
            canTargetPawns = false,
            canTargetBuildings = true,
            canTargetLocations = true,
            validator = targ => !map.fogGrid.IsFogged(targ.Cell)
        };
        Find.Targeter.BeginTargeting(targetingParameters, actionOnValidTarget);
    }

    public static bool IsModActive(string packageId)
    {
        foreach (var allInstalledMod in ModLister.AllInstalledMods)
        {
            if (allInstalledMod.Active && allInstalledMod.PackageId == packageId)
            {
                return true;
            }
        }

        return false;
    }
}