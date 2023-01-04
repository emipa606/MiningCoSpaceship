using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Spaceship;

public class WorldComponent_OrbitalHealing : WorldComponent
{
    private List<int> healEndTicks = new List<int>();

    public List<HealingPawn> healingPawns = new List<HealingPawn>();

    public int nextHealUpdateTick;

    private List<Map> originMaps = new List<Map>();

    private List<Pawn> pawns = new List<Pawn>();

    public WorldComponent_OrbitalHealing(World world)
        : base(world)
    {
    }

    public override void ExposeData()
    {
        base.ExposeData();
        ExposeHealingPawns();
    }

    public void ExposeHealingPawns()
    {
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            pawns.Clear();
            originMaps.Clear();
            healEndTicks.Clear();
            foreach (var healingPawn in healingPawns)
            {
                pawns.Add(healingPawn.pawn);
                originMaps.Add(healingPawn.originMap);
                healEndTicks.Add(healingPawn.healEndTick);
            }
        }

        Scribe_Collections.Look(ref pawns, "pawnList", LookMode.Deep);
        Scribe_Collections.Look(ref originMaps, "originMapList", LookMode.Reference);
        Scribe_Collections.Look(ref healEndTicks, "healEndTickList");
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
        {
            return;
        }

        healingPawns.Clear();
        if (pawns == null)
        {
            return;
        }

        for (var i = 0; i < pawns.Count; i++)
        {
            var item = new HealingPawn(pawns[i], originMaps[i], healEndTicks[i]);
            healingPawns.Add(item);
        }
    }

    public override void WorldComponentTick()
    {
        base.WorldComponentTick();
        if (Find.TickManager.TicksGame < nextHealUpdateTick || Util_Faction.MiningCoFaction.HostileTo(Faction.OfPlayer))
        {
            return;
        }

        nextHealUpdateTick = Find.TickManager.TicksGame + 250;
        var list = new List<HealingPawn>();
        foreach (var healingPawn in healingPawns)
        {
            var pawnBackToSurface = false;
            if (Find.TickManager.TicksGame >= healingPawn.healEndTick)
            {
                pawnBackToSurface = TrySendPawnBackToSurface(healingPawn);
            }

            if (!pawnBackToSurface)
            {
                list.Add(healingPawn);
            }
        }

        healingPawns = list.ListFullCopy();
    }

    public void Notify_BecameHostileToColony()
    {
        if (healingPawns.Count <= 0)
        {
            return;
        }

        string text;
        var stringBuilder = new StringBuilder();
        if (healingPawns.Count == 1)
        {
            var possessive = healingPawns.First().pawn.gender.GetPossessive();
            var objective = healingPawns.First().pawn.gender.GetObjective();
            text = "MCS.keeppawninfo".Translate(objective, possessive, healingPawns.First().pawn.Name.ToStringShort);
        }
        else
        {
            text = "MCS.keeppawn".Translate();
            foreach (var healingPawn in healingPawns)
            {
                stringBuilder.AppendWithComma(healingPawn.pawn.Name.ToStringShort);
            }

            stringBuilder.Append(".");
            text += stringBuilder.ToString();
        }

        Find.LetterStack.ReceiveLetter("MCS.keeppawntitle".Translate(), text, LetterDefOf.NeutralEvent);
    }

    public bool HasAnyTreatableHediff(Pawn pawn)
    {
        foreach (var hediff in pawn.health.hediffSet.hediffs)
        {
            if (IsTreatableHediff(hediff))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsTreatableHediff(Hediff hediff)
    {
        return hediff.Visible && !hediff.IsPermanent() && (hediff.def.tendable || hediff.def.makesSickThought ||
                                                           hediff.def == HediffDefOf.Hypothermia ||
                                                           hediff.def == HediffDefOf.Heatstroke ||
                                                           hediff.def == HediffDefOf.ToxicBuildup ||
                                                           hediff.def == HediffDefOf.BloodLoss ||
                                                           hediff.def == HediffDefOf.Malnutrition);
    }

    public void Notify_PawnStartingOrbitalHealing(Pawn pawn, Map originMap)
    {
        var num = HealTreatableHediffs(pawn);
        var val = num * 15000;
        val = Math.Min(val, 300000);
        var item = new HealingPawn(pawn, originMap, Find.TickManager.TicksGame + val);
        healingPawns.Add(item);
    }

    public int HealTreatableHediffs(Pawn pawn)
    {
        var num = 0;
        var list = new List<Hediff>();
        foreach (var hediff in pawn.health.hediffSet.hediffs)
        {
            if (!IsTreatableHediff(hediff))
            {
                continue;
            }

            list.Add(hediff);
            if (hediff.def == HediffDefOf.Carcinoma || hediff.def == HediffDefOf.Plague)
            {
                num += 10;
            }
        }

        num = list.Count;
        foreach (var item in list)
        {
            item.Heal(item.Severity);
        }

        return num;
    }

    public bool TrySendPawnBackToSurface(HealingPawn healedPawn)
    {
        var originMap = healedPawn.originMap;
        if (Find.Maps.Contains(originMap) && originMap.IsPlayerHome && SendPawnBackToMap(healedPawn.pawn, originMap))
        {
            return true;
        }

        foreach (var item in Find.Maps.InRandomOrder())
        {
            if (item.IsPlayerHome && SendPawnBackToMap(healedPawn.pawn, item))
            {
                return true;
            }
        }

        return false;
    }

    public bool SendPawnBackToMap(Pawn pawn, Map map)
    {
        var orbitalRelay = Util_OrbitalRelay.GetOrbitalRelay(map);
        if (orbitalRelay == null)
        {
            return false;
        }

        if (!orbitalRelay.powerComp.PowerOn)
        {
            return false;
        }

        var bestAvailableLandingPad = Util_LandingPad.GetBestAvailableLandingPad(map);
        if (bestAvailableLandingPad == null)
        {
            return false;
        }

        DropCellFinder.TryFindDropSpotNear(bestAvailableLandingPad.Position, map, out var result, false, false);
        if (!result.IsValid)
        {
            return false;
        }

        var possessive = pawn.gender.GetPossessive();
        var objective = pawn.gender.GetObjective();
        pawn.needs.SetInitialLevels();
        pawn.needs.roomsize.CurLevel = 0f;
        var activeDropPodInfo = new ActiveDropPodInfo();
        if (Rand.Value < 0.98f)
        {
            var text = "MCS.healingdone".Translate(pawn.Name.ToStringShort, possessive);
            Find.LetterStack.ReceiveLetter("MCS.healingdonetitle".Translate(), text, LetterDefOf.PositiveEvent,
                new TargetInfo(result, map));
        }
        else
        {
            var text2 = "MCS.healingfail".Translate(pawn.Name.ToStringShort, possessive, objective);
            Find.LetterStack.ReceiveLetter("MCS.healingfailtitle".Translate(), text2, LetterDefOf.NegativeEvent,
                new TargetInfo(result, map));
            pawn.health.AddHediff(HediffDef.Named("HeartAttack"));
            pawn.health.AddHediff(HediffDefOf.Anesthetic);
            var thing = ThingMaker.MakeThing(ThingDefOf.Silver);
            thing.stackCount = Mathf.RoundToInt(125f);
            activeDropPodInfo.innerContainer.TryAdd(thing);
        }

        activeDropPodInfo.innerContainer.TryAdd(pawn);
        activeDropPodInfo.leaveSlag = true;
        DropPodUtility.MakeDropPodAt(result, map, activeDropPodInfo);
        return true;
    }
}