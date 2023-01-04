using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Spaceship;

[StaticConstructorOnStartup]
public class FlyingSpaceshipLanding : FlyingSpaceship
{
    public const int horizontalTrajectoryDurationInTicks = 480;

    public const float verticalTrajectoryDurationInTicks = 240;

    public static readonly SoundDef preLandingSound = SoundDef.Named("DropPodFall");

    public static readonly SoundDef landingSound = SoundDef.Named("SpaceshipLanding");

    public int landingDuration;

    public IntVec3 landingPadPosition = IntVec3.Invalid;

    public Rot4 landingPadRotation = Rot4.North;

    public int ticksToLanding = 720;

    public void InitializeLandingParameters(Building_LandingPad landingPad, int duration,
        SpaceshipKind kind)
    {
        landingPad.Notify_ShipLanding();
        landingPadPosition = landingPad.Position;
        landingPadRotation = landingPad.Rotation;
        spaceshipExactRotation = landingPadRotation.AsAngle;
        landingDuration = duration;
        spaceshipKind = kind;
        ConfigureShipTexture(spaceshipKind);
        Tick();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksToLanding, "ticksToLanding");
        Scribe_Values.Look(ref landingPadPosition, "landingPadPosition");
        Scribe_Values.Look(ref landingPadRotation, "landingPadRotation");
        Scribe_Values.Look(ref landingDuration, "landingDuration");
    }

    public override void Tick()
    {
        base.Tick();
        if (ticksToLanding == 720)
        {
            preLandingSound.PlayOneShot(new TargetInfo(Position, Map));
        }

        ticksToLanding--;
        if (ticksToLanding == verticalTrajectoryDurationInTicks)
        {
            landingSound.PlayOneShot(new TargetInfo(Position, Map));
        }

        if (ticksToLanding <= verticalTrajectoryDurationInTicks)
        {
            FleckMaker.ThrowDustPuff(
                GenAdj.CellsAdjacentCardinal(landingPadPosition, landingPadRotation, Util_ThingDefOf.LandingPad.Size)
                    .RandomElement(), Map, 3f * (1f - (ticksToLanding / verticalTrajectoryDurationInTicks)));
        }

        if (ticksToLanding != 0)
        {
            return;
        }

        switch (spaceshipKind)
        {
            case SpaceshipKind.CargoPeriodic:
            case SpaceshipKind.CargoRequested:
            {
                var building_SpaceshipCargo =
                    ThingMaker.MakeThing(Util_Spaceship.SpaceshipCargo) as Building_SpaceshipCargo;
                building_SpaceshipCargo?.InitializeData_Cargo(Util_Faction.MiningCoFaction, HitPoints,
                    landingDuration, spaceshipKind);
                GenSpawn.Spawn(building_SpaceshipCargo, landingPadPosition, Map, landingPadRotation);
                break;
            }
            case SpaceshipKind.Damaged:
            {
                var building_SpaceshipDamaged =
                    ThingMaker.MakeThing(Util_Spaceship.SpaceshipDamaged) as Building_SpaceshipDamaged;
                building_SpaceshipDamaged?.InitializeData_Damaged(Util_Faction.MiningCoFaction, HitPoints,
                    landingDuration, spaceshipKind, HitPoints);
                GenSpawn.Spawn(building_SpaceshipDamaged, landingPadPosition, Map, landingPadRotation);
                break;
            }
            case SpaceshipKind.DispatcherDrop:
            {
                var building_SpaceshipDispatcherDrop =
                    ThingMaker.MakeThing(Util_Spaceship
                        .SpaceshipDispatcherDrop) as Building_SpaceshipDispatcherDrop;
                building_SpaceshipDispatcherDrop?.InitializeData_DispatcherDrop(Util_Faction.MiningCoFaction, HitPoints,
                    landingDuration, spaceshipKind);
                GenSpawn.Spawn(building_SpaceshipDispatcherDrop, landingPadPosition, Map, landingPadRotation);
                break;
            }
            case SpaceshipKind.DispatcherPick:
            {
                var building_SpaceshipDispatcherPick =
                    ThingMaker.MakeThing(Util_Spaceship
                        .SpaceshipDispatcherPick) as Building_SpaceshipDispatcherPick;
                building_SpaceshipDispatcherPick?.InitializeData_DispatcherPick(Util_Faction.MiningCoFaction, HitPoints,
                    landingDuration, spaceshipKind);
                GenSpawn.Spawn(building_SpaceshipDispatcherPick, landingPadPosition, Map, landingPadRotation);
                break;
            }
            case SpaceshipKind.Medical:
            {
                var building_SpaceshipMedical =
                    ThingMaker.MakeThing(Util_Spaceship.SpaceshipMedical) as Building_SpaceshipMedical;
                building_SpaceshipMedical?.InitializeData_Medical(Util_Faction.MiningCoFaction, HitPoints,
                    landingDuration, spaceshipKind);
                GenSpawn.Spawn(building_SpaceshipMedical, landingPadPosition, Map, landingPadRotation);
                break;
            }
            default:
                Log.ErrorOnce($"MiningCo. Spaceship: unhandled SpaceshipKind ({spaceshipKind}).", 123456783);
                break;
        }

        Destroy();
    }

    public override void ComputeShipExactPosition()
    {
        var vector = landingPadPosition.ToVector3ShiftedWithAltitude(def.altitudeLayer.AltitudeFor());
        if (spaceshipKind != SpaceshipKind.Medical)
        {
            vector += new Vector3(0f, 0f, 0.5f).RotatedBy(landingPadRotation.AsAngle);
        }

        if (ticksToLanding > verticalTrajectoryDurationInTicks)
        {
            var num = ticksToLanding - verticalTrajectoryDurationInTicks;
            var z = num * num * 0.001f * 0.8f;
            vector -= new Vector3(0f, 0f, z).RotatedBy(spaceshipExactRotation);
        }

        spaceshipExactPosition = vector;
    }

    public override void ComputeShipShadowExactPosition()
    {
        spaceshipShadowExactPosition = spaceshipExactPosition;
        var num = 2f;
        if (ticksToLanding < verticalTrajectoryDurationInTicks)
        {
            num *= ticksToLanding / verticalTrajectoryDurationInTicks;
        }

        var lightSourceInfo = GenCelestial.GetLightSourceInfo(Map, GenCelestial.LightType.Shadow);
        spaceshipShadowExactPosition += new Vector3(lightSourceInfo.vector.x, -0.01f, lightSourceInfo.vector.y) * num;
    }

    public override void ComputeShipExactRotation()
    {
    }

    public override void ComputeShipScale()
    {
        var num = 1.2f;
        var num2 = 0.9f;
        if (ticksToLanding <= verticalTrajectoryDurationInTicks)
        {
            num = 1f + (0.2f * (ticksToLanding / verticalTrajectoryDurationInTicks));
            num2 = 1f - (0.1f * (ticksToLanding / verticalTrajectoryDurationInTicks));
        }

        spaceshipScale = baseSpaceshipScale * num;
        spaceshipShadowScale = baseSpaceshipScale * num2;
    }

    public override void SetShipPositionToBeSelectable()
    {
        Position = IsInBounds() ? spaceshipExactPosition.ToIntVec3() : landingPadPosition;
    }
}