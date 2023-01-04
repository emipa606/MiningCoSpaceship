using System.Collections.Generic;
using Verse;

namespace Spaceship;

public static class Util_LandingPad
{
    public static Building_LandingPad GetBestAvailableLandingPad(Map map)
    {
        var allFreeAndPoweredLandingPads = GetAllFreeAndPoweredLandingPads(map);
        if (allFreeAndPoweredLandingPads == null)
        {
            return null;
        }

        foreach (var item in allFreeAndPoweredLandingPads)
        {
            if (item.isPrimary)
            {
                return item;
            }
        }

        return allFreeAndPoweredLandingPads.RandomElement();
    }

    public static Building_LandingPad GetBestAvailableLandingPadReachingMapEdge(Map map)
    {
        var bestAvailableLandingPad = GetBestAvailableLandingPad(map);
        if (bestAvailableLandingPad != null &&
            Expedition.TryFindRandomExitSpot(map, bestAvailableLandingPad.Position, out _))
        {
            return bestAvailableLandingPad;
        }

        var allFreeAndPoweredLandingPads = GetAllFreeAndPoweredLandingPads(map);
        if (allFreeAndPoweredLandingPads == null)
        {
            return null;
        }

        foreach (var item in allFreeAndPoweredLandingPads.InRandomOrder())
        {
            if (Expedition.TryFindRandomExitSpot(map, item.Position, out _))
            {
                return item;
            }
        }

        return null;
    }

    public static List<Building_LandingPad> GetAllFreeAndPoweredLandingPads(Map map)
    {
        if (!map.listerBuildings.ColonistsHaveBuilding(Util_ThingDefOf.LandingPad))
        {
            return null;
        }

        var list = new List<Building_LandingPad>();
        foreach (var item in map.listerBuildings.AllBuildingsColonistOfDef(Util_ThingDefOf.LandingPad))
        {
            if (item is Building_LandingPad { isFreeAndPowered: true } building_LandingPad)
            {
                list.Add(building_LandingPad);
            }
        }

        return list.Count > 0 ? list : null;
    }

    public static List<Building_LandingPad> GetAllFreeLandingPads(Map map)
    {
        if (!map.listerBuildings.ColonistsHaveBuilding(Util_ThingDefOf.LandingPad))
        {
            return null;
        }

        var list = new List<Building_LandingPad>();
        foreach (var item in map.listerBuildings.AllBuildingsColonistOfDef(Util_ThingDefOf.LandingPad))
        {
            if (item is Building_LandingPad { isFree: true } building_LandingPad)
            {
                list.Add(building_LandingPad);
            }
        }

        return list.Count > 0 ? list : null;
    }
}