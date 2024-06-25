using Verse;

namespace Spaceship;

public class PlaceWorker_TotallyNotUnderRoof : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        foreach (var cell in GenAdj.OccupiedRect(loc, rot, checkingDef.Size).Cells)
        {
            if (cell.Roofed(map))
            {
                return new AcceptanceReport("MustPlaceUnroofed".Translate());
            }
        }

        return true;
    }
}