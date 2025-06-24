using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Spaceship;

[StaticConstructorOnStartup]
public class FlyingSpaceshipLanding : FlyingSpaceship
{
    public const int horizontalTrajectoryDurationInTicks = 480;

    private const float VerticalTrajectoryDurationInTicks = 240;

    private static readonly SoundDef preLandingSound = SoundDef.Named("DropPodFall");

    private static readonly SoundDef landingSound = SoundDef.Named("SpaceshipLanding");

    private int landingDuration;

    private IntVec3 landingPadPosition = IntVec3.Invalid;

    private Rot4 landingPadRotation = Rot4.North;

    private int ticksToLanding = 720;

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

    protected override void Tick()
    {
        base.Tick();
        if (ticksToLanding == 720)
        {
            preLandingSound.PlayOneShot(new TargetInfo(Position, Map));
        }

        ticksToLanding--;
        if (ticksToLanding == VerticalTrajectoryDurationInTicks)
        {
            landingSound.PlayOneShot(new TargetInfo(Position, Map));
        }

        if (ticksToLanding <= VerticalTrajectoryDurationInTicks)
        {
            FleckMaker.ThrowDustPuff(
                GenAdj.CellsAdjacentCardinal(landingPadPosition, landingPadRotation, Util_ThingDefOf.LandingPad.Size)
                    .RandomElement(), Map, 3f * (1f - (ticksToLanding / VerticalTrajectoryDurationInTicks)));
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
                var buildingSpaceshipCargo =
                    ThingMaker.MakeThing(Util_Spaceship.SpaceshipCargo) as Building_SpaceshipCargo;
                buildingSpaceshipCargo?.InitializeData_Cargo(Util_Faction.MiningCoFaction, HitPoints,
                    landingDuration, spaceshipKind);
                GenSpawn.Spawn(buildingSpaceshipCargo, landingPadPosition, Map, landingPadRotation);
                break;
            }
            case SpaceshipKind.Damaged:
            {
                var buildingSpaceshipDamaged =
                    ThingMaker.MakeThing(Util_Spaceship.SpaceshipDamaged) as Building_SpaceshipDamaged;
                buildingSpaceshipDamaged?.InitializeData_Damaged(Util_Faction.MiningCoFaction, HitPoints,
                    landingDuration, spaceshipKind, HitPoints);
                GenSpawn.Spawn(buildingSpaceshipDamaged, landingPadPosition, Map, landingPadRotation);
                break;
            }
            case SpaceshipKind.DispatcherDrop:
            {
                var buildingSpaceshipDispatcherDrop =
                    ThingMaker.MakeThing(Util_Spaceship
                        .SpaceshipDispatcherDrop) as Building_SpaceshipDispatcherDrop;
                buildingSpaceshipDispatcherDrop?.InitializeData_DispatcherDrop(Util_Faction.MiningCoFaction, HitPoints,
                    landingDuration, spaceshipKind);
                GenSpawn.Spawn(buildingSpaceshipDispatcherDrop, landingPadPosition, Map, landingPadRotation);
                break;
            }
            case SpaceshipKind.DispatcherPick:
            {
                var buildingSpaceshipDispatcherPick =
                    ThingMaker.MakeThing(Util_Spaceship
                        .SpaceshipDispatcherPick) as Building_SpaceshipDispatcherPick;
                buildingSpaceshipDispatcherPick?.InitializeData_DispatcherPick(Util_Faction.MiningCoFaction, HitPoints,
                    landingDuration, spaceshipKind);
                GenSpawn.Spawn(buildingSpaceshipDispatcherPick, landingPadPosition, Map, landingPadRotation);
                break;
            }
            case SpaceshipKind.Medical:
            {
                var buildingSpaceshipMedical =
                    ThingMaker.MakeThing(Util_Spaceship.SpaceshipMedical) as Building_SpaceshipMedical;
                buildingSpaceshipMedical?.InitializeData_Medical(Util_Faction.MiningCoFaction, HitPoints,
                    landingDuration, spaceshipKind);
                GenSpawn.Spawn(buildingSpaceshipMedical, landingPadPosition, Map, landingPadRotation);
                break;
            }
            default:
                Log.ErrorOnce($"MiningCo. Spaceship: unhandled SpaceshipKind ({spaceshipKind}).", 123456783);
                break;
        }

        Destroy();
    }

    protected override void ComputeShipExactPosition()
    {
        var vector = landingPadPosition.ToVector3ShiftedWithAltitude(def.altitudeLayer.AltitudeFor());
        if (spaceshipKind != SpaceshipKind.Medical)
        {
            vector += new Vector3(0f, 0f, 0.5f).RotatedBy(landingPadRotation.AsAngle);
        }

        if (ticksToLanding > VerticalTrajectoryDurationInTicks)
        {
            var num = ticksToLanding - VerticalTrajectoryDurationInTicks;
            var z = num * num * 0.001f * 0.8f;
            vector -= new Vector3(0f, 0f, z).RotatedBy(spaceshipExactRotation);
        }

        spaceshipExactPosition = vector;
    }

    protected override void ComputeShipShadowExactPosition()
    {
        spaceshipShadowExactPosition = spaceshipExactPosition;
        var num = 2f;
        if (ticksToLanding < VerticalTrajectoryDurationInTicks)
        {
            num *= ticksToLanding / VerticalTrajectoryDurationInTicks;
        }

        var lightSourceInfo = GenCelestial.GetLightSourceInfo(Map, GenCelestial.LightType.Shadow);
        spaceshipShadowExactPosition += new Vector3(lightSourceInfo.vector.x, -0.01f, lightSourceInfo.vector.y) * num;
    }

    protected override void ComputeShipExactRotation()
    {
    }

    protected override void ComputeShipScale()
    {
        var num = 1.2f;
        var num2 = 0.9f;
        if (ticksToLanding <= VerticalTrajectoryDurationInTicks)
        {
            num = 1f + (0.2f * (ticksToLanding / VerticalTrajectoryDurationInTicks));
            num2 = 1f - (0.1f * (ticksToLanding / VerticalTrajectoryDurationInTicks));
        }

        spaceshipScale = baseSpaceshipScale * num;
        spaceshipShadowScale = baseSpaceshipScale * num2;
    }

    protected override void SetShipPositionToBeSelectable()
    {
        Position = IsInBounds() ? spaceshipExactPosition.ToIntVec3() : landingPadPosition;
    }
}