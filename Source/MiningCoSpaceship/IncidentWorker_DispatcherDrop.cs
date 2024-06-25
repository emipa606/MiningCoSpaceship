using RimWorld;
using Verse;

namespace Spaceship;

public class IncidentWorker_DispatcherDrop : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!base.CanFireNowSub(parms))
        {
            return false;
        }

        if (Util_Faction.MiningCoFaction.HostileTo(Faction.OfPlayer))
        {
            return false;
        }

        var map = (Map)parms.target;
        if (!Expedition.IsWeatherValidForExpedition(map))
        {
            return false;
        }

        var orbitalRelay = Util_OrbitalRelay.GetOrbitalRelay(map);
        if (orbitalRelay == null || !orbitalRelay.powerComp.PowerOn)
        {
            return false;
        }

        var bestAvailableLandingPadReachingMapEdge =
            Util_LandingPad.GetBestAvailableLandingPadReachingMapEdge(map);
        return bestAvailableLandingPadReachingMapEdge != null;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var map = (Map)parms.target;
        var bestAvailableLandingPadReachingMapEdge = Util_LandingPad.GetBestAvailableLandingPadReachingMapEdge(map);
        if (bestAvailableLandingPadReachingMapEdge == null)
        {
            return false;
        }

        Util_Spaceship.SpawnLandingSpaceship(bestAvailableLandingPadReachingMapEdge, SpaceshipKind.DispatcherDrop);
        return true;
    }
}