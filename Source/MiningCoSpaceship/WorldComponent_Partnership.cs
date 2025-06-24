using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace Spaceship;

public class WorldComponent_Partnership(World world) : WorldComponent(world)
{
    private const int feeInitialCostInSilver = 1000;

    public Dictionary<Map, int> feeInSilver = new();

    private List<int> feeInSilverValues = [];

    public int globalGoodwillFeeInSilver;

    private List<Map> maps = [];

    public Dictionary<Map, int> nextAirstrikeMinTick = new();

    private List<int> nextAirstrikeMinTickValues = [];

    public Dictionary<Map, int> nextMedicalSupplyMinTick = new();

    private List<int> nextMedicalSupplyMinTickValues = [];

    public Dictionary<Map, int> nextPeriodicSupplyTick = new();

    private List<int> nextPeriodicSupplyTickValues = [];

    public Dictionary<Map, int> nextRequestedSupplyMinTick = new();

    private List<int> nextRequestedSupplyMinTickValues = [];

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref globalGoodwillFeeInSilver, "globalGoodwillFeeInSilver");
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            cleanNullMap(ref feeInSilver);
            cleanNullMap(ref nextPeriodicSupplyTick);
            cleanNullMap(ref nextRequestedSupplyMinTick);
            cleanNullMap(ref nextMedicalSupplyMinTick);
            cleanNullMap(ref nextAirstrikeMinTick);
            maps.Clear();
            feeInSilverValues.Clear();
            nextPeriodicSupplyTickValues.Clear();
            nextRequestedSupplyMinTickValues.Clear();
            nextMedicalSupplyMinTickValues.Clear();
            nextAirstrikeMinTickValues.Clear();
            foreach (var key in feeInSilver.Keys)
            {
                maps.Add(key);
                feeInSilverValues.Add(feeInSilver[key]);
                nextPeriodicSupplyTickValues.Add(nextPeriodicSupplyTick[key]);
                nextRequestedSupplyMinTickValues.Add(nextRequestedSupplyMinTick[key]);
                nextMedicalSupplyMinTickValues.Add(nextMedicalSupplyMinTick[key]);
                nextAirstrikeMinTickValues.Add(nextAirstrikeMinTick[key]);
            }
        }

        Scribe_Collections.Look(ref maps, "maps", LookMode.Reference);
        Scribe_Collections.Look(ref feeInSilverValues, "feeInSilver");
        Scribe_Collections.Look(ref nextPeriodicSupplyTickValues, "nextPeriodicSupplyTick");
        Scribe_Collections.Look(ref nextRequestedSupplyMinTickValues, "nextRequestedSupplyMinTick");
        Scribe_Collections.Look(ref nextMedicalSupplyMinTickValues, "nextMedicalSupplyMinTick");
        Scribe_Collections.Look(ref nextAirstrikeMinTickValues, "nextAirstrikeMinTick");
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
        {
            return;
        }

        feeInSilver.Clear();
        nextPeriodicSupplyTick.Clear();
        nextRequestedSupplyMinTick.Clear();
        nextMedicalSupplyMinTick.Clear();
        nextAirstrikeMinTick.Clear();
        for (var i = 0; i < maps.Count; i++)
        {
            feeInSilver.Add(maps[i], feeInSilverValues[i]);
            nextPeriodicSupplyTick.Add(maps[i], nextPeriodicSupplyTickValues[i]);
            nextRequestedSupplyMinTick.Add(maps[i], nextRequestedSupplyMinTickValues[i]);
            nextMedicalSupplyMinTick.Add(maps[i], nextMedicalSupplyMinTickValues[i]);
            nextAirstrikeMinTick.Add(maps[i], nextAirstrikeMinTickValues[i]);
        }
    }

    private static void cleanNullMap(ref Dictionary<Map, int> dictionary)
    {
        var dictionary2 = new Dictionary<Map, int>();
        foreach (var key in dictionary.Keys)
        {
            if (Find.Maps.Contains(key))
            {
                dictionary2.Add(key, dictionary[key]);
            }
        }

        dictionary = dictionary2;
    }

    public void InitializeFeeIfNeeded(Map map)
    {
        feeInSilver.TryAdd(map, feeInitialCostInSilver);
    }

    public void InitializePeriodicSupplyTickIfNeeded(Map map)
    {
        nextPeriodicSupplyTick.TryAdd(map, 0);
    }

    public void InitializeRequestedSupplyTickIfNeeded(Map map)
    {
        nextRequestedSupplyMinTick.TryAdd(map, 0);
    }

    public void InitializeMedicalSupplyTickIfNeeded(Map map)
    {
        nextMedicalSupplyMinTick.TryAdd(map, 0);
    }

    public void InitializeAirstrikeTickIfNeeded(Map map)
    {
        nextAirstrikeMinTick.TryAdd(map, 0);
    }
}