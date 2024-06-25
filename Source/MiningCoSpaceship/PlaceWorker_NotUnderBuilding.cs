using Verse;

namespace Spaceship;

public class PlaceWorker_NotUnderBuilding : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        foreach (var cell in GenAdj.OccupiedRect(loc, rot, checkingDef.Size).Cells)
        {
            foreach (var thing2 in cell.GetThingList(map))
            {
                if (thing2.def.category == ThingCategory.Building && thing2.def.building.isEdifice ||
                    thing2.def == Util_ThingDefOf.LandingPad || thing2.def == Util_ThingDefOf.LandingPad.frameDef ||
                    thing2.def == Util_ThingDefOf.LandingPad.blueprintDef)
                {
                    return new AcceptanceReport("SpaceAlreadyOccupied".Translate());
                }
            }
        }

        return true;
    }
}