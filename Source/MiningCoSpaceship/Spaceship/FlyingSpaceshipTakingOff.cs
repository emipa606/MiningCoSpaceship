using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Spaceship;

[StaticConstructorOnStartup]
public class FlyingSpaceshipTakingOff : FlyingSpaceship
{
    public const int horizontalTrajectoryDurationInTicks = 480;

    public const float verticalTrajectoryDurationInTicks = 240;

    public static readonly SoundDef takingOffSound = SoundDef.Named("SpaceshipTakingOff");

    public IntVec3 landingPadPosition = IntVec3.Invalid;

    public Rot4 landingPadRotation = Rot4.North;

    public int ticksSinceTakeOff;

    public void InitializeTakingOffParameters(IntVec3 position, Rot4 rotation, SpaceshipKind kind)
    {
        landingPadPosition = position;
        landingPadRotation = rotation;
        spaceshipExactRotation = landingPadRotation.AsAngle;
        spaceshipKind = kind;
        ConfigureShipTexture(spaceshipKind);
        base.Tick();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksSinceTakeOff, "ticksSinceTakeOff");
        Scribe_Values.Look(ref landingPadPosition, "landingPadPosition");
        Scribe_Values.Look(ref landingPadRotation, "landingPadRotation");
    }

    public override void Tick()
    {
        base.Tick();
        ticksSinceTakeOff++;
        if (ticksSinceTakeOff <= verticalTrajectoryDurationInTicks)
        {
            FleckMaker.ThrowDustPuff(
                GenAdj.CellsAdjacentCardinal(landingPadPosition, landingPadRotation, Util_ThingDefOf.LandingPad.Size)
                    .RandomElement(), Map, 3f * (1f - (ticksSinceTakeOff / verticalTrajectoryDurationInTicks)));
        }

        if (ticksSinceTakeOff == 1)
        {
            takingOffSound.PlayOneShot(new TargetInfo(Position, Map));
        }

        if (ticksSinceTakeOff >= 720)
        {
            Destroy();
        }
    }

    public override void ComputeShipExactPosition()
    {
        var vector = landingPadPosition.ToVector3ShiftedWithAltitude(def.altitudeLayer.AltitudeFor());
        if (spaceshipKind != SpaceshipKind.Medical)
        {
            vector += new Vector3(0f, 0f, 0.5f).RotatedBy(landingPadRotation.AsAngle);
        }

        if (ticksSinceTakeOff >= verticalTrajectoryDurationInTicks)
        {
            var num = ticksSinceTakeOff - verticalTrajectoryDurationInTicks;
            var z = num * num * 0.001f * 0.8f;
            vector += new Vector3(0f, 0f, z).RotatedBy(spaceshipExactRotation);
        }

        spaceshipExactPosition = vector;
    }

    public override void ComputeShipShadowExactPosition()
    {
        spaceshipShadowExactPosition = spaceshipExactPosition;
        var num = 2f;
        if (ticksSinceTakeOff < verticalTrajectoryDurationInTicks)
        {
            num *= ticksSinceTakeOff / verticalTrajectoryDurationInTicks;
        }

        var lightSourceInfo = GenCelestial.GetLightSourceInfo(Map, GenCelestial.LightType.Shadow);
        spaceshipShadowExactPosition += new Vector3(lightSourceInfo.vector.x, -0.1f, lightSourceInfo.vector.y) * num;
    }

    public override void ComputeShipExactRotation()
    {
    }

    public override void ComputeShipScale()
    {
        var num = 1.2f;
        var num2 = 0.8f;
        if (ticksSinceTakeOff < verticalTrajectoryDurationInTicks)
        {
            num = 1f + (0.2f * (ticksSinceTakeOff / verticalTrajectoryDurationInTicks));
            num2 = 1f - (0.2f * (ticksSinceTakeOff / verticalTrajectoryDurationInTicks));
        }

        spaceshipScale = baseSpaceshipScale * num;
        spaceshipShadowScale = baseSpaceshipScale * num2;
    }

    public override void SetShipPositionToBeSelectable()
    {
        Position = IsInBounds() ? spaceshipExactPosition.ToIntVec3() : landingPadPosition;
    }
}