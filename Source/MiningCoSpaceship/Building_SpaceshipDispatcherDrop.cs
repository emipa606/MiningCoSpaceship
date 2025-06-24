using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Spaceship;

public class Building_SpaceshipDispatcherDrop : Building_SpaceshipDispatcher
{
    private int teamDropTick;
    private bool teamIsDropped;

    protected override bool takeOffRequestIsEnabled => false;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (!respawningAfterLoad)
        {
            pawnsAboard.AddRange(Expedition.GenerateExpeditionPawns(Map));
        }
    }

    public void InitializeData_DispatcherDrop(Faction faction, int hitPoints, int landingDuration,
        SpaceshipKind kind)
    {
        InitializeData(faction, hitPoints, landingDuration, kind);
        teamDropTick = Find.TickManager.TicksGame + 300;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref teamIsDropped, "teamIsDropped");
        Scribe_Values.Look(ref teamDropTick, "teamDropTick");
    }

    protected override void Tick()
    {
        base.Tick();
        if (teamIsDropped || Find.TickManager.TicksGame < teamDropTick ||
            Util_Faction.MiningCoFaction.HostileTo(Faction.OfPlayer))
        {
            return;
        }

        dropTeam();
        teamIsDropped = true;
        takeOffTick = Find.TickManager.TicksGame + 600;
    }

    private void dropTeam()
    {
        if (!Expedition.TryFindRandomExitSpot(Map, Position, out var exitSpot))
        {
            return;
        }

        var list = new List<Pawn>();
        var list2 = new List<Pawn>();
        foreach (var item in pawnsAboard)
        {
            if (item.kindDef == Util_PawnKindDefOf.Pilot)
            {
                list.Add(item);
                continue;
            }

            list2.Add(item);
            GenSpawn.Spawn(item, Position + IntVec3Utility.RandomHorizontalOffset(3f), Map);
        }

        pawnsAboard = list;
        LordMaker.MakeNewLord(Util_Faction.MiningCoFaction, new LordJob_ExitMap(exitSpot), Map, list2);
        SpawnPayment(list2.Count);
    }
}