using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Spaceship;

public class Building_SpaceshipCargo : Building_Spaceship, ITrader
{
    public override bool takeOffRequestIsEnabled => true;

    public TradeCurrency TradeCurrency => TradeCurrency.Silver;

    public bool CanTradeNow => !this.DestroyedOrNull() && !this.IsBurning();

    public IEnumerable<Thing> Goods => things;

    public int RandomPriceFactorSeed { get; } = -1;

    public float TradePriceImprovementOffsetForPlayer => 0f;

    public TraderKindDef TraderKind => cargoKind;

    public string TraderName => "MCS.shiptitle".Translate();

    public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
    {
        var list = new List<Thing>();
        foreach (var item in TradeUtility.AllLaunchableThingsForTrade(Map))
        {
            list.Add(item);
        }

        foreach (var cell in this.OccupiedRect().Cells)
        {
            foreach (var thing in cell.GetThingList(Map))
            {
                if (TradeUtility.EverPlayerSellable(thing.def) && TraderKind.WillTrade(thing.def) &&
                    !list.Contains(thing))
                {
                    list.Add(thing);
                }
            }
        }

        return list;
    }

    public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
    {
        var thing = toGive.SplitOff(countToGive);
        thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
        SpawnItem(thing.def, thing.Stuff, thing.stackCount, Position, Map, 5f);
        thing.Destroy();
    }

    public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
    {
        var thing = toGive.SplitOff(countToGive);
        thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this);
        var thing2 = TradeUtility.ThingFromStockToMergeWith(this, thing);
        if (thing2 != null)
        {
            if (!thing2.TryAbsorbStack(thing, false))
            {
                thing.Destroy();
            }
        }
        else
        {
            things.TryAdd(thing, false);
        }
    }

    public void InitializeData_Cargo(Faction faction, int hitPoints, int landingDuration, SpaceshipKind kind)
    {
        InitializeData(faction, hitPoints, landingDuration, kind);
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        if (mode != DestroyMode.KillFinalize)
        {
            var num = HitPoints / (float)MaxHitPoints;
            if (num < 1f)
            {
                int num2;
                if (cargoKind == Util_TraderKindDefOf.spaceshipCargoPeriodicSupply)
                {
                    num2 = Mathf.RoundToInt(1200000f * (1f - num));
                    Util_Misc.Partnership.nextPeriodicSupplyTick[Map] += num2;
                }
                else
                {
                    num2 = Mathf.RoundToInt(600000f * (1f - num));
                    Util_Misc.Partnership.nextRequestedSupplyMinTick[Map] += num2;
                }

                var text = "MCS.damagedcargo".Translate();
                Find.LetterStack.ReceiveLetter("MCS.damagedcargotitle".Translate(), text, LetterDefOf.NegativeEvent,
                    new TargetInfo(Position, Map));
            }
        }

        base.Destroy(mode);
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
    {
        var list = new List<FloatMenuOption>();
        if (!selPawn.CanReach(this, PathEndMode.ClosestTouch, Danger.Some))
        {
            return GetFloatMenuOptionsCannotReach(selPawn);
        }

        foreach (var floatMenuOption in base.GetFloatMenuOptions(selPawn))
        {
            list.Add(floatMenuOption);
        }

        if (this.IsBurning())
        {
            var item = new FloatMenuOption("CannotUseReason".Translate("BurningLower".Translate()), null);
            list.Add(item);
        }
        else if (selPawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
        {
            var item2 = new FloatMenuOption("CannotPrioritizeWorkTypeDisabled".Translate(SkillDefOf.Social.LabelCap),
                null);
            list.Add(item2);
        }
        else
        {
            void Action()
            {
                var job = JobMaker.MakeJob(Util_JobDefOf.TradeWithCargoSpaceship, this);
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }

            var item3 = new FloatMenuOption("MCS.tradewithcargo".Translate(), Action);
            list.Add(item3);
        }

        return list;
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        if (Find.TickManager.TicksGame >= takeOffTick)
        {
            stringBuilder.Append("MCS.takeoffasap".Translate());
        }
        else
        {
            stringBuilder.Append(
                "MCS.planedtakeoff".Translate((takeOffTick - Find.TickManager.TicksGame)
                    .ToStringTicksToPeriodVerbose()));
        }

        return stringBuilder.ToString();
    }
}