using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Spaceship;

public class Building_LandingPad : Building
{
    private const int BlockCheckPeriodInTicks = 60;

    private const int LightPeriodInTicks = 960;

    private const int LightInternalCrossDelayInTicks = 30;

    private const int LightInternalCrossDurationInTicks = 30;

    public const int lightExternalFrameDelayInTicks = 15;

    private const int LightExternalFrameDurationInTicks = 105;

    private string blockingReasons = "";
    public bool isPrimary = true;

    private bool isReserved;

    private int nextBlockCheckTick;

    private CompPowerTrader powerComp;

    public bool IsFree => !isReserved && blockingReasons.Length == 0;

    public bool IsFreeAndPowered => IsFree && powerComp.PowerOn;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        powerComp = GetComp<CompPowerTrader>();
        powerComp.powerStartedAction = Notify_PowerStarted;
        powerComp.powerStoppedAction = Notify_PowerStopped;
        if (Map.listerBuildings.ColonistsHaveBuilding(Util_ThingDefOf.LandingPad))
        {
            foreach (var item in Map.listerBuildings.AllBuildingsColonistOfDef(Util_ThingDefOf.LandingPad))
            {
                if (item is Building_LandingPad building_LandingPad &&
                    (building_LandingPad == this || !building_LandingPad.isPrimary))
                {
                    continue;
                }

                isPrimary = false;
                break;
            }
        }

        if (!respawningAfterLoad)
        {
            SpawnBeacons();
        }
    }

    private void SpawnBeacons()
    {
        var buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(0, 0, 3).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.white, LightPeriodInTicks,
            LightInternalCrossDurationInTicks, 0);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(3, 0, 0).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.white, LightPeriodInTicks,
            LightInternalCrossDurationInTicks, 0);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(0, 0, -3).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.white, LightPeriodInTicks,
            LightInternalCrossDurationInTicks, 0);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(-3, 0, 0).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.white, LightPeriodInTicks,
            LightInternalCrossDurationInTicks, 0);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(0, 0, 2).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.white, LightPeriodInTicks,
            LightInternalCrossDurationInTicks, LightInternalCrossDelayInTicks);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(2, 0, 0).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.white, LightPeriodInTicks,
            LightInternalCrossDurationInTicks, LightInternalCrossDelayInTicks);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(0, 0, -2).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.white, LightPeriodInTicks,
            LightInternalCrossDurationInTicks, LightInternalCrossDelayInTicks);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(-2, 0, 0).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.white, LightPeriodInTicks,
            LightInternalCrossDurationInTicks, LightInternalCrossDelayInTicks);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(0, 0, 1).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.white, LightPeriodInTicks,
            LightInternalCrossDurationInTicks, 60);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(1, 0, 0).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.white, LightPeriodInTicks,
            LightInternalCrossDurationInTicks, 60);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(0, 0, -1).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.white, LightPeriodInTicks,
            LightInternalCrossDurationInTicks, 60);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(-1, 0, 0).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.white, LightPeriodInTicks,
            LightInternalCrossDurationInTicks, 60);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position, Map) as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.green, LightPeriodInTicks, 60, 90);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(-2, 0, -8).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.red, LightPeriodInTicks,
            LightExternalFrameDurationInTicks, 480);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(2, 0, -8).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.red, LightPeriodInTicks,
            LightExternalFrameDurationInTicks, 480);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(-4, 0, -6).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.red, LightPeriodInTicks,
            LightExternalFrameDurationInTicks, 495);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(4, 0, -6).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.red, LightPeriodInTicks,
            LightExternalFrameDurationInTicks, 495);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(-4, 0, -3).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.red, LightPeriodInTicks,
            LightExternalFrameDurationInTicks, 510);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(4, 0, -3).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.red, LightPeriodInTicks,
            LightExternalFrameDurationInTicks, 510);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(-4, 0, 0).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.red, LightPeriodInTicks,
            LightExternalFrameDurationInTicks, 525);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(4, 0, 0).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.red, LightPeriodInTicks,
            LightExternalFrameDurationInTicks, 525);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(-4, 0, 3).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.red, LightPeriodInTicks,
            LightExternalFrameDurationInTicks, 540);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(4, 0, 3).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.red, LightPeriodInTicks,
            LightExternalFrameDurationInTicks, 540);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(-4, 0, 6).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.red, LightPeriodInTicks,
            LightExternalFrameDurationInTicks, 555);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(4, 0, 6).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.red, LightPeriodInTicks,
            LightExternalFrameDurationInTicks, 555);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(-1, 0, 9).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.red, LightPeriodInTicks,
            LightExternalFrameDurationInTicks, 570);
        buildingLandingPadBeacon =
            GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeacon, Position + new IntVec3(1, 0, 9).RotatedBy(Rotation), Map)
                as Building_LandingPadBeacon;
        buildingLandingPadBeacon?.InitializeParameters(this, Color.red, LightPeriodInTicks,
            LightExternalFrameDurationInTicks, 570);
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        var unused = Map;
        foreach (var cell in this.OccupiedRect().Cells)
        {
            cell.GetFirstThing(Map, Util_ThingDefOf.LandingPadBeacon)?.Destroy();
        }

        base.DeSpawn(mode);
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        var map = Map;
        foreach (var cell in this.OccupiedRect().Cells)
        {
            cell.GetFirstThing(Map, Util_ThingDefOf.LandingPadBeacon)?.Destroy();
        }

        base.Destroy(mode);
        Util_OrbitalRelay.TryUpdateLandingPadAvailability(map);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref nextBlockCheckTick, "nextBlockCheckTick");
        Scribe_Values.Look(ref isPrimary, "isPrimary");
        Scribe_Values.Look(ref isReserved, "isReserved");
        Scribe_Values.Look(ref blockingReasons, "blockingReasons");
    }

    protected override void Tick()
    {
        base.Tick();
        if (Find.TickManager.TicksGame < nextBlockCheckTick)
        {
            return;
        }

        nextBlockCheckTick = Find.TickManager.TicksGame + BlockCheckPeriodInTicks;
        checkForBlockingThing();
    }

    private void checkForBlockingThing()
    {
        var foundRoof = true;
        blockingReasons = "";
        var stringBuilder = new StringBuilder();
        foreach (var cell in this.OccupiedRect().Cells)
        {
            if (foundRoof && cell.Roofed(Map))
            {
                foundRoof = false;
                stringBuilder.AppendWithComma("MCS.roof".Translate());
            }

            var edifice = cell.GetEdifice(Map);
            if (edifice != null && edifice.def != Util_ThingDefOf.VulcanTurret)
            {
                stringBuilder.AppendWithComma(edifice.Label);
            }

            var plant = cell.GetPlant(Map);
            if (plant != null && plant.def.plant.IsTree)
            {
                stringBuilder.AppendWithComma(plant.Label);
            }
        }

        if (stringBuilder.Length > 0)
        {
            blockingReasons = stringBuilder.ToString();
        }
    }

    private void Notify_PowerStarted()
    {
        Util_OrbitalRelay.TryUpdateLandingPadAvailability(Map);
        foreach (var item in Map.listerBuildings.AllBuildingsColonistOfDef(Util_ThingDefOf.LandingPadBeacon))
        {
            var building_LandingPadBeacon = item as Building_LandingPadBeacon;
            if (building_LandingPadBeacon?.landingPad == this)
            {
                building_LandingPadBeacon.Notify_PowerStarted();
            }
        }
    }

    private void Notify_PowerStopped()
    {
        Util_OrbitalRelay.TryUpdateLandingPadAvailability(Map);
        foreach (var item in Map.listerBuildings.AllBuildingsColonistOfDef(Util_ThingDefOf.LandingPadBeacon))
        {
            var building_LandingPadBeacon = item as Building_LandingPadBeacon;
            if (building_LandingPadBeacon?.landingPad == this)
            {
                building_LandingPadBeacon.Notify_PowerStopped();
            }
        }
    }

    public void Notify_ShipLanding()
    {
        isReserved = true;
        Util_OrbitalRelay.TryUpdateLandingPadAvailability(Map);
    }

    public void Notify_ShipTakingOff()
    {
        isReserved = false;
        Util_OrbitalRelay.TryUpdateLandingPadAvailability(Map);
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        if (isPrimary)
        {
            stringBuilder.AppendLine();
            stringBuilder.Append("MCS.primarypad".Translate());
        }

        if (blockingReasons.Length <= 0)
        {
            return stringBuilder.ToString();
        }

        stringBuilder.AppendLine();
        stringBuilder.Append("MCS.blockedby".Translate(blockingReasons));

        return stringBuilder.ToString();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        IList<Gizmo> list = new List<Gizmo>();
        var num = 700000105;
        var command_Action = new Command_Action();
        if (isPrimary)
        {
            command_Action.icon = ContentFinder<Texture2D>.Get("Ui/Commands/Commands_Primary");
            command_Action.defaultLabel = "MCS.primary".Translate();
            command_Action.defaultDesc = "MCS.primaryTT".Translate();
        }
        else
        {
            command_Action.icon = ContentFinder<Texture2D>.Get("Ui/Commands/Commands_Ancillary");
            command_Action.defaultLabel = "MCS.ancillary".Translate();
            command_Action.defaultDesc = "MCS.ancillaryTT".Translate();
        }

        command_Action.activateSound = SoundDefOf.Click;
        command_Action.action = setAsPrimary;
        command_Action.groupKey = num + 1;
        list.Add(command_Action);
        var gizmos = base.GetGizmos();
        return gizmos != null ? gizmos.Concat(list) : list;
    }

    private void setAsPrimary()
    {
        foreach (var item in Map.listerBuildings.AllBuildingsColonistOfDef(Util_ThingDefOf.LandingPad))
        {
            ((Building_LandingPad)item).isPrimary = false;
        }

        isPrimary = true;
    }
}