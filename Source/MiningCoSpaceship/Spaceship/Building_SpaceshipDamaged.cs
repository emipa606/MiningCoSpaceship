using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Spaceship;

public class Building_SpaceshipDamaged : Building_Spaceship
{
    public const int availableMaterialsUpdatePeriodInTicks = 62;

    public Dictionary<ThingDef, int> availableMaterials = new Dictionary<ThingDef, int>();

    public int initialHitPoints;

    public Dictionary<ThingDef, int> neededMaterials = new Dictionary<ThingDef, int>();

    public int nextAvailableMaterialsUpdateTick;
    public bool repairsAreStarted;

    public override bool takeOffRequestIsEnabled => true;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (respawningAfterLoad)
        {
            return;
        }

        neededMaterials.Add(ThingDefOf.ComponentIndustrial, Rand.RangeInclusive(2, 15));
        neededMaterials.Add(ThingDefOf.Steel, Rand.RangeInclusive(50, 250));
        foreach (var key in neededMaterials.Keys)
        {
            availableMaterials.Add(key, 0);
        }
    }

    public void InitializeData_Damaged(Faction faction, int hitPoints, int landingDuration, SpaceshipKind kind,
        int startingHitPoints)
    {
        InitializeData(faction, hitPoints, landingDuration, kind);
        initialHitPoints = startingHitPoints;
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        if (mode != DestroyMode.KillFinalize)
        {
            DetermineConsequences();
        }

        base.Destroy(mode);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref repairsAreStarted, "repairsAreStarted");
        Scribe_Values.Look(ref initialHitPoints, "initialHitPoints");
        Scribe_Collections.Look(ref neededMaterials, "neededMaterials");
    }

    public override void Tick()
    {
        base.Tick();
        if (this.DestroyedOrNull())
        {
            return;
        }

        if (!repairsAreStarted && Find.TickManager.TicksGame >= nextAvailableMaterialsUpdateTick)
        {
            nextAvailableMaterialsUpdateTick = Find.TickManager.TicksGame + availableMaterialsUpdatePeriodInTicks;
            UpdateAvailableMaterials();
        }

        if (HitPoints >= MaxHitPoints)
        {
            RequestTakeOff();
        }
    }

    public void UpdateAvailableMaterials()
    {
        var dictionary = new Dictionary<ThingDef, int>();
        foreach (var key in neededMaterials.Keys)
        {
            dictionary.Add(key, 0);
        }

        foreach (var item in GetNeededMaterialsAround())
        {
            dictionary[item.def] += item.stackCount;
        }

        availableMaterials = dictionary;
    }

    public List<Thing> GetNeededMaterialsAround()
    {
        var list = new List<Thing>();
        foreach (var item in GenRadial.RadialCellsAround(Position, def.specialDisplayRadius, true))
        {
            foreach (var thing in item.GetThingList(Map))
            {
                if (neededMaterials.Keys.Contains(thing.def))
                {
                    list.Add(thing);
                }
            }
        }

        return list;
    }

    public bool AreNeededMaterialsAvailable()
    {
        foreach (var key in neededMaterials.Keys)
        {
            if (availableMaterials[key] < neededMaterials[key])
            {
                return false;
            }
        }

        return true;
    }

    public void TryStartRepairs()
    {
        UpdateAvailableMaterials();
        if (!AreNeededMaterialsAvailable())
        {
            Messages.Message("MCS.nomaterials".Translate(),
                this, MessageTypeDefOf.RejectInput);
            return;
        }

        repairsAreStarted = true;
        var dictionary = new Dictionary<ThingDef, int>(neededMaterials);
        foreach (var key in neededMaterials.Keys)
        {
            foreach (var item in GetNeededMaterialsAround())
            {
                if (item.def != key)
                {
                    continue;
                }

                if (item.stackCount > dictionary[key])
                {
                    item.stackCount -= dictionary[key];
                    dictionary[key] = 0;
                }
                else
                {
                    dictionary[key] -= item.stackCount;
                    item.Destroy();
                }

                if (dictionary[key] == 0)
                {
                    break;
                }
            }
        }

        SetFaction(Faction.OfPlayer);
        Messages.Message("MCS.materialsok".Translate(), this, MessageTypeDefOf.PositiveEvent);
    }

    public void DetermineConsequences()
    {
        if (repairsAreStarted)
        {
            if (HitPoints == MaxHitPoints)
            {
                var text = "MCS.repairscomplete".Translate();
                Find.LetterStack.ReceiveLetter("MCS.repairscompletetitle".Translate(), text, LetterDefOf.PositiveEvent,
                    new TargetInfo(Position, Map));
                SpawnCargoContent(1f);
                Util_Faction.AffectGoodwillWith(Util_Faction.MiningCoFaction, Faction.OfPlayer, 10);
            }
            else if (HitPoints >= initialHitPoints)
            {
                var text2 = "MCS.repairscompletesoso".Translate();
                Find.LetterStack.ReceiveLetter("MCS.repairscompletesosotitle".Translate(), text2,
                    LetterDefOf.PositiveEvent,
                    new TargetInfo(Position, Map));
                SpawnCargoContent(0.5f);
                Util_Faction.AffectGoodwillWith(Util_Faction.MiningCoFaction, Faction.OfPlayer, 5);
            }
            else
            {
                var text3 = "MCS.repairscompletebad".Translate();
                Find.LetterStack.ReceiveLetter("MCS.repairscompletebadtitle".Translate(), text3,
                    LetterDefOf.NeutralEvent,
                    new TargetInfo(Position, Map));
            }
        }
        else if (HitPoints >= initialHitPoints)
        {
            var text4 = "MCS.repairscompletenone".Translate();
            Find.LetterStack.ReceiveLetter("MCS.repairscompletenonetitle".Translate(), text4, LetterDefOf.NegativeEvent,
                new TargetInfo(Position, Map));
            Util_Misc.Partnership.nextPeriodicSupplyTick[Map] = Find.TickManager.TicksGame + 1200000;
            Util_Misc.Partnership.nextRequestedSupplyMinTick[Map] = Find.TickManager.TicksGame + 600000;
            Util_Misc.Partnership.nextAirstrikeMinTick[Map] = Find.TickManager.TicksGame + 1200000;
            Util_Faction.AffectGoodwillWith(Util_Faction.MiningCoFaction, Faction.OfPlayer, -10);
        }
        else
        {
            var text5 = "MCS.repairscompleteworst".Translate();
            Find.LetterStack.ReceiveLetter("MCS.repairscompletenonetitle".Translate(), text5, LetterDefOf.NegativeEvent,
                new TargetInfo(Position, Map));
            Util_Misc.Partnership.nextPeriodicSupplyTick[Map] = Find.TickManager.TicksGame + 2400000;
            Util_Misc.Partnership.nextRequestedSupplyMinTick[Map] = Find.TickManager.TicksGame + 1200000;
            Util_Misc.Partnership.nextAirstrikeMinTick[Map] = Find.TickManager.TicksGame + 2400000;
            Util_Faction.AffectGoodwillWith(Util_Faction.MiningCoFaction, Faction.OfPlayer, -20);
        }
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        IList<Gizmo> list = new List<Gizmo>();
        var num = 700000106;
        if (!repairsAreStarted)
        {
            var command_Action = new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get(
                    "Things/Item/Resource/Medicine/MedicineUltratech/MedicineUltratech_a"),
                defaultLabel = "MCS.startrepairs".Translate(),
                defaultDesc = "MCS.startrepairsTT".Translate(),
                activateSound = SoundDefOf.Click,
                action = TryStartRepairs,
                groupKey = num + 1
            };
            list.Add(command_Action);
        }

        var gizmos = base.GetGizmos();
        return gizmos != null ? gizmos.Concat(list) : list;
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        if (!repairsAreStarted)
        {
            stringBuilder.Append("MCS.materialsnearby".Translate());
            foreach (var key in neededMaterials.Keys)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append($"{key.LabelCap + ": "}{availableMaterials[key]}/{neededMaterials[key]}");
            }
        }
        else
        {
            stringBuilder.Append("MCS.repairprogress".Translate((HitPoints / (float)MaxHitPoints).ToStringPercent()));
        }

        return stringBuilder.ToString();
    }
}