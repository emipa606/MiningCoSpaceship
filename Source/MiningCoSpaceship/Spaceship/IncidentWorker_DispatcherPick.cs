using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Spaceship;

public class IncidentWorker_DispatcherPick : IncidentWorker
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

        Util_Spaceship.SpawnLandingSpaceship(bestAvailableLandingPadReachingMapEdge, SpaceshipKind.DispatcherPick);
        var teamPawns = Expedition.GenerateExpeditionPawns(map);
        if (Rand.Value < 0.2f)
        {
            ApplyInjuriesOrIllnessToTeam(map, teamPawns);
        }

        return SpawnTeamOnMapEdge(bestAvailableLandingPadReachingMapEdge.Position, map, teamPawns);
    }

    public void ApplyInjuriesOrIllnessToTeam(Map map, List<Pawn> teamPawns)
    {
        if (map.Biome == BiomeDefOf.TropicalRainforest || map.Biome == BiomeDef.Named("TropicalSwamp"))
        {
            var hediffDef = !(Rand.Value < 0.5f) ? HediffDefOf.Malaria : HediffDef.Named("SleepingSickness");
            {
                foreach (var teamPawn in teamPawns)
                {
                    teamPawn.health.AddHediff(hediffDef);
                }

                return;
            }
        }

        var value = Mathf.RoundToInt(Rand.Range(0.25f, 0.5f) * teamPawns.Count);
        value = Mathf.Clamp(value, 1, teamPawns.Count - 1);
        for (var i = 0; i < value; i++)
        {
            Expedition.RandomlyDamagePawn(teamPawns[i], Rand.Range(1, 2), Rand.Range(12, 20));
        }
    }

    public bool SpawnTeamOnMapEdge(IntVec3 targetDestination, Map map, List<Pawn> teamPawns)
    {
        if (!Expedition.TryFindRandomExitSpot(map, targetDestination, out var exitSpot))
        {
            return Expedition.TryFindRandomExitSpot(map, targetDestination, out _);
        }

        foreach (var teamPawn in teamPawns)
        {
            var loc = CellFinder.RandomSpawnCellForPawnNear(exitSpot, map, 5);
            GenSpawn.Spawn(teamPawn, loc, map);
        }

        LordMaker.MakeNewLord(Util_Faction.MiningCoFaction, new LordJob_BoardSpaceship(targetDestination), map,
            teamPawns);

        return Expedition.TryFindRandomExitSpot(map, targetDestination, out _);
    }
}