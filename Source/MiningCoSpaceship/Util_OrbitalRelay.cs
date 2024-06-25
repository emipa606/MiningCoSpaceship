using System.Linq;
using Verse;

namespace Spaceship;

public static class Util_OrbitalRelay
{
    public static Building_OrbitalRelay GetOrbitalRelay(Map map)
    {
        if (!map.listerBuildings.ColonistsHaveBuilding(Util_ThingDefOf.OrbitalRelay))
        {
            return null;
        }

        return map.listerBuildings.AllBuildingsColonistOfDef(Util_ThingDefOf.OrbitalRelay).First() as
            Building_OrbitalRelay;
    }

    public static void TryUpdateLandingPadAvailability(Map map)
    {
        GetOrbitalRelay(map)?.UpdateLandingPadAvailability();
    }
}