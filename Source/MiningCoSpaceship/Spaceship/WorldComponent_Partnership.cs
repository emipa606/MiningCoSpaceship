using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace Spaceship;

public class WorldComponent_Partnership : WorldComponent
{
    public const int feeInitialCostInSilver = 1000;

    public Dictionary<Map, int> feeInSilver = new Dictionary<Map, int>();

    private List<int> feeInSilverValues = new List<int>();

    public int globalGoodwillFeeInSilver;

    private List<Map> maps = new List<Map>();

    public Dictionary<Map, int> nextAirstrikeMinTick = new Dictionary<Map, int>();

    private List<int> nextAirstrikeMinTickValues = new List<int>();

    public Dictionary<Map, int> nextMedicalSupplyMinTick = new Dictionary<Map, int>();

    private List<int> nextMedicalSupplyMinTickValues = new List<int>();

    public Dictionary<Map, int> nextPeriodicSupplyTick = new Dictionary<Map, int>();

    private List<int> nextPeriodicSupplyTickValues = new List<int>();

    public Dictionary<Map, int> nextRequestedSupplyMinTick = new Dictionary<Map, int>();

    private List<int> nextRequestedSupplyMinTickValues = new List<int>();

    public WorldComponent_Partnership(World world)
        : base(world)
    {
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref globalGoodwillFeeInSilver, "globalGoodwillFeeInSilver");
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            CleanNullMap(ref feeInSilver);
            CleanNullMap(ref nextPeriodicSupplyTick);
            CleanNullMap(ref nextRequestedSupplyMinTick);
            CleanNullMap(ref nextMedicalSupplyMinTick);
            CleanNullMap(ref nextAirstrikeMinTick);
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

    public void CleanNullMap(ref Dictionary<Map, int> dictionary)
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
        if (!feeInSilver.ContainsKey(map))
        {
            feeInSilver.Add(map, feeInitialCostInSilver);
        }
    }

    public void InitializePeriodicSupplyTickIfNeeded(Map map)
    {
        if (!nextPeriodicSupplyTick.ContainsKey(map))
        {
            nextPeriodicSupplyTick.Add(map, 0);
        }
    }

    public void InitializeRequestedSupplyTickIfNeeded(Map map)
    {
        if (!nextRequestedSupplyMinTick.ContainsKey(map))
        {
            nextRequestedSupplyMinTick.Add(map, 0);
        }
    }

    public void InitializeMedicalSupplyTickIfNeeded(Map map)
    {
        if (!nextMedicalSupplyMinTick.ContainsKey(map))
        {
            nextMedicalSupplyMinTick.Add(map, 0);
        }
    }

    public void InitializeAirstrikeTickIfNeeded(Map map)
    {
        if (!nextAirstrikeMinTick.ContainsKey(map))
        {
            nextAirstrikeMinTick.Add(map, 0);
        }
    }
}