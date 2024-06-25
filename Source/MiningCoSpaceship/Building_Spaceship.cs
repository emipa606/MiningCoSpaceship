using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Spaceship;

public abstract class Building_Spaceship : Building, IThingHolder
{
    public const int pilotsNumber = 2;

    public TraderKindDef cargoKind;

    public List<Pawn> pawnsAboard = new List<Pawn>();

    public SpaceshipKind spaceshipKind = SpaceshipKind.CargoPeriodic;

    public int takeOffTick;

    public ThingOwner things;

    public virtual bool takeOffRequestIsEnabled => true;

    public ThingOwner GetDirectlyHeldThings()
    {
        return things;
    }

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (respawningAfterLoad)
        {
            return;
        }

        if (things == null)
        {
            things = new ThingOwner<Thing>(this);
            GenerateThings();
            DestroyRoof();
        }

        for (var i = 0; i < pilotsNumber; i++)
        {
            var pawn = MiningCoPawnGenerator.GeneratePawn(Util_PawnKindDefOf.Pilot, Map);
            if (pawn != null)
            {
                pawnsAboard.Add(pawn);
            }
        }
    }

    protected void InitializeData(Faction faction, int hitPoints, int landingDuration, SpaceshipKind kind)
    {
        SetFaction(faction);
        HitPoints = hitPoints;
        takeOffTick = Find.TickManager.TicksGame + landingDuration;
        spaceshipKind = kind;
        cargoKind = GetCargoKind(spaceshipKind);
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        var firstThing = Position.GetFirstThing(Map, Util_ThingDefOf.LandingPad);
        (firstThing as Building_LandingPad)?.Notify_ShipTakingOff();

        if (mode == DestroyMode.KillFinalize)
        {
            SpawnSurvivingPawns();
            SpawnExplosions();
            SpawnFuelPuddleAndFire();
            Util_Misc.Partnership.feeInSilver[Map] += Mathf.RoundToInt(def.BaseMarketValue * 0.5f);
            var num = SpawnCargoContent(0.5f);
            Util_Misc.Partnership.feeInSilver[Map] += Mathf.RoundToInt(num * 0.5f);
            Util_Faction.AffectGoodwillWith(Util_Faction.MiningCoFaction, Faction.OfPlayer, -30);
            var text = "MCS.lostship".Translate();
            Find.LetterStack.ReceiveLetter("MCS.lostshiptitle".Translate(), text, LetterDefOf.ThreatSmall,
                new TargetInfo(Position, Map));
        }
        else
        {
            foreach (var item in pawnsAboard)
            {
                item.Destroy();
            }

            pawnsAboard.Clear();
            var flyingSpaceshipTakingOff =
                ThingMaker.MakeThing(Util_Spaceship.SpaceshipTakingOff) as FlyingSpaceshipTakingOff;
            GenSpawn.Spawn(flyingSpaceshipTakingOff, Position, Map, Rotation);
            flyingSpaceshipTakingOff?.InitializeTakingOffParameters(Position, Rotation, spaceshipKind);
            if (flyingSpaceshipTakingOff != null)
            {
                flyingSpaceshipTakingOff.HitPoints = HitPoints;
                if (Util_OrbitalRelay.GetOrbitalRelay(Map) != null)
                {
                    Messages.Message("MCS.shiptakingoff".Translate(), flyingSpaceshipTakingOff,
                        MessageTypeDefOf.NeutralEvent);
                }
            }

            DestroyRoof();
        }

        things.ClearAndDestroyContentsOrPassToWorld();
        base.Destroy(mode);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref takeOffTick, "takeOffTick");
        Scribe_Values.Look(ref spaceshipKind, "spaceshipKind");
        Scribe_Defs.Look(ref cargoKind, "cargoKind");
        Scribe_Deep.Look(ref things, "things");
        Scribe_Collections.Look(ref pawnsAboard, "pawnsAboard", LookMode.Deep);
    }

    public void DestroyRoof()
    {
        foreach (var cell in this.OccupiedRect().Cells)
        {
            if (!cell.Roofed(Map))
            {
                continue;
            }

            var roofDef = Map.roofGrid.RoofAt(cell);
            if (roofDef.filthLeaving != null)
            {
                FilthMaker.TryMakeFilth(cell, Map, roofDef.filthLeaving, Rand.RangeInclusive(1, 3));
            }

            Map.roofGrid.SetRoof(cell, null);
        }
    }

    public override void Tick()
    {
        base.Tick();
        if (!this.DestroyedOrNull() && Find.TickManager.TicksGame >= takeOffTick &&
            !Map.GameConditionManager.ConditionIsActive(Util_GameConditionDefOf.SolarFlare))
        {
            Destroy();
        }
    }

    public virtual void RequestTakeOff()
    {
        takeOffTick = Find.TickManager.TicksGame;
    }

    public TraderKindDef GetCargoKind(SpaceshipKind kind)
    {
        var result = Util_TraderKindDefOf.SpaceshipCargoPeriodicSupply;
        switch (kind)
        {
            case SpaceshipKind.CargoPeriodic:
                result = Util_TraderKindDefOf.SpaceshipCargoPeriodicSupply;
                break;
            case SpaceshipKind.CargoRequested:
                result = Util_TraderKindDefOf.SpaceshipCargoRequestedSupply;
                break;
            case SpaceshipKind.Damaged:
                result = Util_TraderKindDefOf.SpaceshipCargoDamaged;
                break;
            case SpaceshipKind.DispatcherDrop:
            case SpaceshipKind.DispatcherPick:
                result = Util_TraderKindDefOf.SpaceshipCargoDispatcher;
                break;
            case SpaceshipKind.Medical:
                result = Util_TraderKindDefOf.SpaceshipCargoMedical;
                break;
            default:
                Log.ErrorOnce(
                    $"MiningCo. Spaceship: unhandled SpaceshipKind ({spaceshipKind}) in Building_Spaceship.GetCargoKind.",
                    123456781);
                break;
        }

        return result;
    }

    public void SpawnSurvivingPawns()
    {
        var list = new List<Pawn>();
        foreach (var item in pawnsAboard)
        {
            GenSpawn.Spawn(item, this.OccupiedRect().Cells.RandomElement(), Map);
            Expedition.RandomlyDamagePawn(item, Rand.RangeInclusive(1, 4), Rand.RangeInclusive(4, 16));
            if (!item.Dead)
            {
                list.Add(item);
            }
        }

        pawnsAboard.Clear();
        if (list.Count <= 0)
        {
            return;
        }

        if (RCellFinder.TryFindBestExitSpot(list.First(), out var invalid, TraverseMode.PassAllDestroyableThings))
        {
            LordMaker.MakeNewLord(Util_Faction.MiningCoFaction, new LordJob_ExitMap(invalid), Map, list);
        }
    }

    public void SpawnExplosions()
    {
        for (var i = 0; i < 5; i++)
        {
            GenExplosion.DoExplosion(Position + IntVec3Utility.RandomHorizontalOffset(5f), Map, Rand.Range(3f, 7f),
                DamageDefOf.Bomb, this, Rand.Range(8, 45));
        }
    }

    public void SpawnFuelPuddleAndFire()
    {
        for (var i = 0; i < 150; i++)
        {
            var loc = Position + IntVec3Utility.RandomHorizontalOffset(12f);
            GenSpawn.Spawn(ThingDefOf.Filth_Fuel, loc, Map);
            if (GenSpawn.Spawn(ThingDefOf.Fire, loc, Map) is Fire fire)
            {
                fire.fireSize = Rand.Range(0.25f, 1.25f);
            }
        }
    }

    protected int SpawnCargoContent(float integrity)
    {
        var num = 0;
        foreach (var thing in things)
        {
            var num2 = Mathf.RoundToInt(thing.stackCount * integrity);
            SpawnItem(thing.def, thing.Stuff, num2, Position, Map, 12f);
            num += Mathf.RoundToInt(thing.def.BaseMarketValue * num2);
        }

        return num;
    }

    protected static Thing SpawnItem(ThingDef itemDef, ThingDef stuff, int quantity, IntVec3 position, Map map,
        float radius)
    {
        Thing thing = null;
        var num = quantity;
        while (num > 0)
        {
            var num2 = num <= itemDef.stackLimit ? num : itemDef.stackLimit;
            num -= num2;
            thing = ThingMaker.MakeThing(itemDef, stuff);
            thing.stackCount = num2;
            GenDrop.TryDropSpawn(thing, position + IntVec3Utility.RandomHorizontalOffset(radius), map,
                ThingPlaceMode.Near, out _);
        }

        return thing;
    }

    public virtual void Notify_PawnBoarding(Pawn pawn, bool isLastLordPawn)
    {
        pawnsAboard.Add(pawn);
        pawn.DeSpawn();
    }

    public bool IsTakeOffImminent(int marginTimeInTicks)
    {
        var num = Math.Max(0, takeOffTick - marginTimeInTicks);
        return Find.TickManager.TicksGame >= num;
    }

    public void GenerateThings()
    {
        var parms = default(ThingSetMakerParams);
        parms.traderDef = cargoKind;
        parms.tile = Map.Tile;
        things.TryAddRangeOrTransfer(ThingSetMakerDefOf.TraderStock.root.Generate(parms));
    }

    protected IEnumerable<FloatMenuOption> GetFloatMenuOptionsCannotReach(Pawn selPawn)
    {
        var list = new List<FloatMenuOption>();
        var item = new FloatMenuOption("CannotUseNoPath".Translate(), null);
        list.Add(item);
        return list;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        IList<Gizmo> list = new List<Gizmo>();
        var num = 700000112;
        if (takeOffRequestIsEnabled)
        {
            var command_Action = new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip"),
                defaultLabel = "MCS.requesttakeoff".Translate(),
                defaultDesc = "MCS.requesttakeoffTT".Translate(),
                activateSound = SoundDefOf.Click,
                action = RequestTakeOff,
                groupKey = num + 1
            };
            list.Add(command_Action);
        }

        var gizmos = base.GetGizmos();
        return gizmos != null ? gizmos.Concat(list) : list;
    }
}