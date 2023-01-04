using Verse;
using Verse.AI;

namespace Spaceship;

public static class Util_DownedPawn
{
    public static Pawn GetRandomReachableDownedPawn(Pawn carrier)
    {
        if (carrier.Map == null)
        {
            return null;
        }

        foreach (var item in carrier.Map.mapPawns.FreeHumanlikesSpawnedOfFaction(carrier.Faction))
        {
            if (item.Downed && carrier.CanReserveAndReach(item, PathEndMode.OnCell, Danger.Some))
            {
                return item;
            }
        }

        return null;
    }

    public static Pawn GetNearestReachableDownedPawn(Pawn carrier)
    {
        if (carrier.Map == null)
        {
            return null;
        }

        Pawn result = null;
        var num = 99999f;
        foreach (var item in carrier.Map.mapPawns.FreeHumanlikesSpawnedOfFaction(carrier.Faction))
        {
            if (!item.Downed || !carrier.CanReserveAndReach(item, PathEndMode.OnCell, Danger.Some))
            {
                continue;
            }

            var num2 = carrier.Position.DistanceTo(item.Position);
            if (!(num2 < num))
            {
                continue;
            }

            num = num2;
            result = item;
        }

        return result;
    }
}