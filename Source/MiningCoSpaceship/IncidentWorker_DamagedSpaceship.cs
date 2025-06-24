using RimWorld;
using UnityEngine;
using Verse;

namespace Spaceship;

public class IncidentWorker_DamagedSpaceship : IncidentWorker
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
        var orbitalRelay = Util_OrbitalRelay.GetOrbitalRelay(map);
        if (orbitalRelay == null)
        {
            return false;
        }

        var allFreeLandingPads = Util_LandingPad.GetAllFreeLandingPads(map);
        return allFreeLandingPads != null;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var map = (Map)parms.target;
        var allFreeLandingPads = Util_LandingPad.GetAllFreeLandingPads(map);
        if (allFreeLandingPads == null)
        {
            return false;
        }

        var buildingLandingPad = allFreeLandingPads.RandomElement();
        var flyingSpaceshipLanding = Util_Spaceship.SpawnLandingSpaceship(buildingLandingPad, SpaceshipKind.Damaged);
        flyingSpaceshipLanding.HitPoints =
            Mathf.RoundToInt(Rand.Range(0.15f, 0.45f) * flyingSpaceshipLanding.HitPoints);
        var text = "MCS.repairsequest".Translate();
        Find.LetterStack.ReceiveLetter("MCS.repairsequesttitle".Translate(), text, LetterDefOf.NeutralEvent,
            new TargetInfo(buildingLandingPad.Position, buildingLandingPad.Map));
        return true;
    }
}