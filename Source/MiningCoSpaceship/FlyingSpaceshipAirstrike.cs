using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Spaceship;

[StaticConstructorOnStartup]
public class FlyingSpaceshipAirstrike : FlyingSpaceship
{
    public static readonly SoundDef airStrikeSound = SoundDef.Named("Airstrike");

    public readonly List<bool> weaponShootRight = [true, true, true];

    public AirstrikeDef airStrikeDef;

    private Faction clientFaction;

    public float shipToTargetDistance;

    public IntVec3 targetPosition = IntVec3.Invalid;

    public int ticksAfterOverflight;
    public int ticksBeforeOverflight;

    public List<int> weaponNextShotTick = [0, 0, 0];

    public List<int> weaponRemainingRounds = [-1, -1, -1];

    public void InitializeAirstrikeData(IntVec3 position, AirstrikeDef strikeDef, Faction faction)
    {
        targetPosition = position;
        airStrikeDef = strikeDef;
        clientFaction = faction;
        ticksBeforeOverflight = airStrikeDef.ticksBeforeOverflightInitialValue;
        ticksAfterOverflight = 0;
        spaceshipKind = SpaceshipKind.Airstrike;
        ComputeAirstrikeRotation(targetPosition);
        ConfigureShipTexture(spaceshipKind);
        base.Tick();
    }

    public void ComputeAirstrikeRotation(IntVec3 position)
    {
        var num = Map.Size.x / 2;
        var num2 = Map.Size.z / 2;
        if (position.x <= 50 || Map.Size.x - position.x <= 50 || position.z <= 50 ||
            Map.Size.z - position.z <= 50)
        {
            if (position.x <= num && position.z >= num2)
            {
                spaceshipExactRotation = Rand.RangeInclusive(280, 350);
            }
            else if (position.x >= num && position.z >= num2)
            {
                spaceshipExactRotation = Rand.RangeInclusive(10, 80);
            }
            else if (position.x >= num && position.z <= num2)
            {
                spaceshipExactRotation = Rand.RangeInclusive(100, 170);
            }
            else
            {
                spaceshipExactRotation = Rand.RangeInclusive(190, 260);
            }
        }
        else
        {
            spaceshipExactRotation = Rand.Range(0f, 360f);
        }

        Rotation = new Rot4(Mathf.RoundToInt(spaceshipExactRotation) / 90);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksBeforeOverflight, "ticksBeforeOverflight");
        Scribe_Values.Look(ref ticksAfterOverflight, "ticksAfterOverflight");
        Scribe_Values.Look(ref targetPosition, "targetPosition");
        Scribe_Defs.Look(ref airStrikeDef, "airStrikeDef");
        Scribe_Values.Look(ref spaceshipExactRotation, "shipRotation");
        Scribe_Collections.Look(ref weaponRemainingRounds, "weaponRemainingRounds");
        Scribe_Collections.Look(ref weaponNextShotTick, "weaponNextShotTick");
    }

    public override void Tick()
    {
        base.Tick();
        if (ticksBeforeOverflight == airStrikeDef.ticksBeforeOverflightPlaySound)
        {
            airStrikeSound.PlayOneShot(new TargetInfo(targetPosition, Map));
        }

        for (var i = 0; i < airStrikeDef.weapons.Count; i++)
        {
            WeaponTick(i, airStrikeDef.weapons[i]);
        }

        if (ticksBeforeOverflight > 0)
        {
            ticksBeforeOverflight--;
            return;
        }

        ticksAfterOverflight++;
        if (ticksAfterOverflight >= airStrikeDef.ticksAfterOverflightFinalValue ||
            !spaceshipExactPosition.InBounds(Map))
        {
            Destroy();
        }
    }

    public override void ComputeShipExactPosition()
    {
        var vector = new Vector3(0f, 0f, 1f).RotatedBy(spaceshipExactRotation);
        spaceshipExactPosition = targetPosition.ToVector3ShiftedWithAltitude(def.altitudeLayer);
        if (ticksBeforeOverflight > 0)
        {
            if (ticksBeforeOverflight > airStrikeDef.ticksBeforeOverflightReducedSpeed)
            {
                float num = ticksBeforeOverflight - airStrikeDef.ticksBeforeOverflightReducedSpeed;
                var num2 = num * num * 0.01f;
                shipToTargetDistance = (num2 + ticksBeforeOverflight) * airStrikeDef.cellsTravelledPerTick;
            }
            else
            {
                shipToTargetDistance = ticksBeforeOverflight * airStrikeDef.cellsTravelledPerTick;
            }

            spaceshipExactPosition -= vector * shipToTargetDistance;
        }
        else
        {
            if (ticksAfterOverflight > airStrikeDef.ticksAfterOverflightReducedSpeed)
            {
                float num3 = ticksAfterOverflight - airStrikeDef.ticksAfterOverflightReducedSpeed;
                var num4 = num3 * num3 * 0.01f;
                shipToTargetDistance = (num4 + ticksAfterOverflight) * airStrikeDef.cellsTravelledPerTick;
            }
            else
            {
                shipToTargetDistance = ticksAfterOverflight * airStrikeDef.cellsTravelledPerTick;
            }

            spaceshipExactPosition += vector * shipToTargetDistance;
        }
    }

    public override void ComputeShipShadowExactPosition()
    {
        var lightSourceInfo = GenCelestial.GetLightSourceInfo(Map, GenCelestial.LightType.Shadow);
        spaceshipShadowExactPosition = spaceshipExactPosition +
                                       (2f * new Vector3(lightSourceInfo.vector.x, -0.01f, lightSourceInfo.vector.y));
    }

    public override void ComputeShipExactRotation()
    {
    }

    public override void ComputeShipScale()
    {
    }

    public override void SetShipPositionToBeSelectable()
    {
        Position = IsInBounds() ? spaceshipExactPosition.ToIntVec3() : targetPosition;
    }

    public void WeaponTick(int weaponIndex, WeaponDef weaponDef)
    {
        if (weaponDef.ammoDef != null && weaponRemainingRounds[weaponIndex] == -1 &&
            shipToTargetDistance <= weaponDef.startShootingDistance)
        {
            weaponRemainingRounds[weaponIndex] = weaponDef.ammoQuantity;
            weaponNextShotTick[weaponIndex] = Find.TickManager.TicksGame;
            var num = Rand.RangeInclusive(0, 1);
            if (num == 1)
            {
                weaponShootRight[weaponIndex] = true;
            }
            else
            {
                weaponShootRight[weaponIndex] = false;
            }

            if (weaponDef.disableRainDurationInTicks > 0)
            {
                Map.weatherDecider.DisableRainFor(weaponDef.disableRainDurationInTicks);
            }
        }

        if (weaponRemainingRounds[weaponIndex] <= 0 || Find.TickManager.TicksGame < weaponNextShotTick[weaponIndex])
        {
            return;
        }

        var num2 = weaponDef.isTwinGun && !weaponShootRight[weaponIndex] ? -1f : 1f;
        var vector = spaceshipExactPosition +
                     new Vector3(num2 * weaponDef.horizontalPositionOffset, 0f, weaponDef.verticalPositionOffset)
                         .RotatedBy(spaceshipExactRotation);
        var vector2 = vector + new Vector3(0f, 0f, weaponDef.ammoTravelDistance).RotatedBy(spaceshipExactRotation);
        if (vector.InBounds(Map) && vector2.InBounds(Map))
        {
            var projectile = GenSpawn.Spawn(weaponDef.ammoDef, vector.ToIntVec3(), Map) as Projectile;
            weaponDef.soundCastDef?.PlayOneShot(new TargetInfo(vector.ToIntVec3(), Map));

            FleckMaker.Static(vector, Map, FleckDefOf.ShotFlash, 10f);
            Pawn pawn = null;
            if (weaponDef.targetAcquireRange > 0f)
            {
                pawn = GetRandomeHostilePawnAround(vector2, weaponDef.targetAcquireRange);
            }

            if (pawn != null)
            {
                projectile?.Launch(this, vector, pawn, pawn, ProjectileHitFlags.IntendedTarget);
            }
            else
            {
                vector2 += new Vector3(Rand.Range(0f - weaponDef.targetAcquireRange, weaponDef.targetAcquireRange), 0f,
                    0f).RotatedBy(spaceshipExactRotation);
                projectile?.Launch(this, vector, vector2.ToIntVec3(), vector2.ToIntVec3(), ProjectileHitFlags.None);
            }
        }

        weaponRemainingRounds[weaponIndex]--;
        weaponNextShotTick[weaponIndex] = Find.TickManager.TicksGame + weaponDef.ticksBetweenShots;
        weaponShootRight[weaponIndex] = !weaponShootRight[weaponIndex];
    }

    public Pawn GetRandomeHostilePawnAround(Vector3 center, float radius)
    {
        var list = new List<Pawn>();
        foreach (var item in GenRadial.RadialCellsAround(center.ToIntVec3(), radius, true))
        {
            if (!item.InBounds(Map))
            {
                continue;
            }

            var firstPawn = item.GetFirstPawn(Map);
            if (firstPawn != null && firstPawn.HostileTo(clientFaction) && !firstPawn.health.Downed)
            {
                list.Add(firstPawn);
            }
        }

        return list.Count > 0 ? list.RandomElement() : null;
    }
}