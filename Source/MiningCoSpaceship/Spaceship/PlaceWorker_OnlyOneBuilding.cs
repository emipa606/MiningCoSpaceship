using Verse;

namespace Spaceship;

public class PlaceWorker_OnlyOneBuilding : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        var list = map.listerThings.ThingsOfDef(checkingDef.blueprintDef);
        var list2 = map.listerThings.ThingsOfDef(checkingDef.frameDef);
        if (list is { Count: > 0 } || list2 is { Count: > 0 } ||
            map.listerBuildings.ColonistsHaveBuilding(ThingDef.Named(checkingDef.defName)))
        {
            return "MCS.onepermap".Translate(checkingDef.defName);
        }

        return true;
    }
}