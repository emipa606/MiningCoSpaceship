using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Spaceship;

[StaticConstructorOnStartup]
public class Building_OrbitalRelay : Building
{
    private const int SpaceshipLandingCheckPeriodInTick = 60;

    private const float DishRotationPerTick = 0.06f;

    private const int RotationIntervalMin = 1200;

    private const int RotationIntervalMax = 2400;

    private const int RotationDurationMin = 500;

    private const int RotationDurationMax = 1500;

    private static readonly Material dishTexture = MaterialPool.MatFrom("Things/Building/OrbitalRelay/SatelliteDish");

    private static readonly Vector3 dishScale = new(5f, 0f, 5f);

    private bool clockwiseRotation = true;

    private Matrix4x4 dishMatrix;

    private float dishRotation;

    private bool landingPadIsAvailable;

    private int lastMedicalSupplyTick;

    private int lastPeriodicSupplyTick;

    private int lastRequestedSupplyTick;

    private int nextSpaceshipLandingCheckTick;

    public CompPowerTrader powerComp;

    private Sustainer rotationSoundSustainer;

    private int ticksToNextRotation = 1200;

    private int ticksToRotationEnd;

    public bool CanUseConsoleNow =>
        !Map.gameConditionManager.ConditionIsActive(Util_GameConditionDefOf.SolarFlare) && powerComp.PowerOn;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        powerComp = GetComp<CompPowerTrader>();
        if (respawningAfterLoad)
        {
            return;
        }

        dishRotation = Rotation.AsAngle;
        Util_Misc.Partnership.InitializeFeeIfNeeded(Map);
        Util_Misc.Partnership.InitializePeriodicSupplyTickIfNeeded(Map);
        Util_Misc.Partnership.InitializeRequestedSupplyTickIfNeeded(Map);
        Util_Misc.Partnership.InitializeMedicalSupplyTickIfNeeded(Map);
        Util_Misc.Partnership.InitializeAirstrikeTickIfNeeded(Map);
        if (Util_Misc.Partnership.feeInSilver[Map] > 0)
        {
            Find.LetterStack.ReceiveLetter("MCS.partnershipfee".Translate(),
                "MCS.partnershipfeeTT".Translate(),
                LetterDefOf.NeutralEvent, new TargetInfo(Position, Map));
        }

        UpdateLandingPadAvailability();
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        stopRotationSound();
        base.Destroy(mode);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref lastPeriodicSupplyTick, "lastPeriodicSupplyTick");
        Scribe_Values.Look(ref lastRequestedSupplyTick, "lastRequestedSupplyTick");
        Scribe_Values.Look(ref lastMedicalSupplyTick, "lastMedicalSupplyTick");
        Scribe_Values.Look(ref landingPadIsAvailable, "landingPadIsAvailable");
        Scribe_Values.Look(ref nextSpaceshipLandingCheckTick, "nextSpaceshipLandingCheckTick");
        Scribe_Values.Look(ref ticksToNextRotation, "ticksToNextRotation");
        Scribe_Values.Look(ref dishRotation, "dishRotation");
        Scribe_Values.Look(ref clockwiseRotation, "clockwiseRotation");
        Scribe_Values.Look(ref ticksToRotationEnd, "ticksToRotationEnd");
    }

    protected override void Tick()
    {
        base.Tick();
        if (!powerComp.PowerOn)
        {
            stopRotationSound();
            return;
        }

        if (Find.TickManager.TicksGame >= nextSpaceshipLandingCheckTick)
        {
            nextSpaceshipLandingCheckTick = Find.TickManager.TicksGame + SpaceshipLandingCheckPeriodInTick;
            UpdateLandingPadAvailability();
            if (!Util_Faction.MiningCoFaction.HostileTo(Faction.OfPlayer) &&
                Util_Misc.Partnership.feeInSilver[Map] == 0 &&
                Find.TickManager.TicksGame >= Util_Misc.Partnership.nextPeriodicSupplyTick[Map] &&
                landingPadIsAvailable &&
                !Map.GameConditionManager.ConditionIsActive(Util_GameConditionDefOf.SolarFlare) &&
                !Map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
            {
                var bestAvailableLandingPad = Util_LandingPad.GetBestAvailableLandingPad(Map);
                if (bestAvailableLandingPad != null)
                {
                    Util_Spaceship.SpawnLandingSpaceship(bestAvailableLandingPad, SpaceshipKind.CargoPeriodic);
                }
            }
        }

        updateDishRotation();
    }

    public void UpdateLandingPadAvailability()
    {
        landingPadIsAvailable = Util_LandingPad.GetAllFreeAndPoweredLandingPads(Map) != null;
    }

    public void Notify_CargoSpaceshipPeriodicLanding()
    {
        lastPeriodicSupplyTick = Find.TickManager.TicksGame;
    }

    public void Notify_CargoSpaceshipRequestedLanding()
    {
        lastRequestedSupplyTick = Find.TickManager.TicksGame;
    }

    public void Notify_MedicalSpaceshipLanding()
    {
        lastMedicalSupplyTick = Find.TickManager.TicksGame;
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        if (!powerComp.PowerOn)
        {
            stringBuilder.AppendLine();
            stringBuilder.Append("MCS.orbitaldown".Translate());
            return stringBuilder.ToString();
        }

        stringBuilder.AppendLine();
        stringBuilder.Append("MCS.goodwill".Translate(Util_Faction.MiningCoFaction.GoodwillWith(Faction.OfPlayer)));
        if (Util_Faction.MiningCoFaction.GoodwillWith(Faction.OfPlayer) <= -80)
        {
            stringBuilder.Append("MCS.hostile".Translate());
        }

        if (Util_Misc.Partnership.feeInSilver[Map] > 0 || Util_Misc.Partnership.globalGoodwillFeeInSilver > 0)
        {
            stringBuilder.AppendLine();
            stringBuilder.Append("MCS.feeunpaid".Translate());
            return stringBuilder.ToString();
        }

        stringBuilder.AppendLine();
        stringBuilder.Append("MCS.periodicsupply".Translate());
        if (lastPeriodicSupplyTick > 0 && Find.TickManager.TicksGame >= lastPeriodicSupplyTick &&
            Find.TickManager.TicksGame - lastPeriodicSupplyTick < 720)
        {
            stringBuilder.Append("MCS.inapproach".Translate());
        }
        else if (!landingPadIsAvailable)
        {
            stringBuilder.Append("MCS.nolandingpad".Translate());
        }
        else
        {
            var value = (Util_Misc.Partnership.nextPeriodicSupplyTick[Map] - Find.TickManager.TicksGame)
                .ToStringTicksToPeriodVerbose();
            stringBuilder.Append(value);
        }

        stringBuilder.AppendLine();
        stringBuilder.Append("MCS.requestedsupply".Translate());
        if (lastRequestedSupplyTick > 0 && Find.TickManager.TicksGame >= lastRequestedSupplyTick &&
            Find.TickManager.TicksGame - lastRequestedSupplyTick < 720)
        {
            stringBuilder.Append("MCS.inapproach".Translate());
        }
        else if (!landingPadIsAvailable)
        {
            stringBuilder.Append("MCS.nolandingpad".Translate());
        }
        else if (Find.TickManager.TicksGame >= Util_Misc.Partnership.nextRequestedSupplyMinTick[Map])
        {
            stringBuilder.Append("MCS.available".Translate());
        }
        else
        {
            var text = (Util_Misc.Partnership.nextRequestedSupplyMinTick[Map] - Find.TickManager.TicksGame)
                .ToStringTicksToPeriodVerbose();
            stringBuilder.Append("MCS.availablein".Translate(text));
        }

        stringBuilder.AppendLine();
        stringBuilder.Append("MCS.medicalsupply".Translate());
        if (lastMedicalSupplyTick > 0 && Find.TickManager.TicksGame >= lastMedicalSupplyTick &&
            Find.TickManager.TicksGame - lastMedicalSupplyTick < 720)
        {
            stringBuilder.Append("MCS.inapproach".Translate());
        }
        else if (!landingPadIsAvailable)
        {
            stringBuilder.Append("MCS.nolandingpad".Translate());
        }
        else if (Find.TickManager.TicksGame >= Util_Misc.Partnership.nextMedicalSupplyMinTick[Map])
        {
            stringBuilder.Append("MCS.available".Translate());
        }
        else
        {
            var text2 = (Util_Misc.Partnership.nextMedicalSupplyMinTick[Map] - Find.TickManager.TicksGame)
                .ToStringTicksToPeriodVerbose();
            stringBuilder.Append("MCS.availablein".Translate(text2));
        }

        stringBuilder.AppendLine();
        if (Find.TickManager.TicksGame >= Util_Misc.Partnership.nextAirstrikeMinTick[Map])
        {
            stringBuilder.Append("MCS.airstrikeavailable".Translate());
        }
        else
        {
            var text3 = (Util_Misc.Partnership.nextAirstrikeMinTick[Map] - Find.TickManager.TicksGame)
                .ToStringTicksToPeriodVerbose();
            stringBuilder.Append("MCS.airstrikeavailablein".Translate(text3));
        }

        if (Util_Misc.OrbitalHealing.healingPawns.Count <= 0)
        {
            return stringBuilder.ToString();
        }

        stringBuilder.AppendLine();
        stringBuilder.Append("MCS.colonistaboard".Translate());
        foreach (var healingPawn in Util_Misc.OrbitalHealing.healingPawns)
        {
            stringBuilder.AppendLine();
            stringBuilder.Append($"- {healingPawn.pawn.Name.ToStringShort}: ");
            if (Util_Faction.MiningCoFaction.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Hostile)
            {
                stringBuilder.Append("MCS.guest".Translate());
            }
            else if (healingPawn.healEndTick > Find.TickManager.TicksGame)
            {
                stringBuilder.Append(
                    "MCS.healing".Translate((healingPawn.healEndTick - Find.TickManager.TicksGame)
                        .ToStringTicksToPeriodVerbose()));
            }
            else
            {
                stringBuilder.Append("MCS.needlandingpad".Translate());
            }
        }

        return stringBuilder.ToString();
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
    {
        if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some))
        {
            var item = new FloatMenuOption("CannotUseNoPath".Translate(), null);
            return new List<FloatMenuOption> { item };
        }

        if (Spawned && Map.gameConditionManager.ConditionIsActive(Util_GameConditionDefOf.SolarFlare))
        {
            var item2 = new FloatMenuOption("CannotUseSolarFlare".Translate(), null);
            return new List<FloatMenuOption> { item2 };
        }

        if (!powerComp.PowerOn)
        {
            var item3 = new FloatMenuOption("CannotUseNoPower".Translate(), null);
            return new List<FloatMenuOption> { item3 };
        }

        if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
        {
            var item4 = new FloatMenuOption(
                "CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Talking.label)), null);
            return new List<FloatMenuOption> { item4 };
        }

        var item5 = new FloatMenuOption("MCS.callminingco".Translate(), action);
        return new List<FloatMenuOption> { item5 };

        void action()
        {
            var job = JobMaker.MakeJob(Util_JobDefOf.JobDef_UseOrbitalRelayConsole, this);
            selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }
    }

    private void startRotationSound()
    {
        stopRotationSound();
        rotationSoundSustainer =
            SoundDef.Named("GeothermalPlant_Ambience").TrySpawnSustainer(new TargetInfo(Position, Map));
    }

    private void stopRotationSound()
    {
        if (rotationSoundSustainer == null)
        {
            return;
        }

        rotationSoundSustainer.End();
        rotationSoundSustainer = null;
    }

    private void updateDishRotation()
    {
        if (ticksToNextRotation > 0)
        {
            ticksToNextRotation--;
            if (ticksToNextRotation != 0)
            {
                return;
            }

            ticksToRotationEnd = Rand.RangeInclusive(RotationDurationMin, RotationDurationMax);
            clockwiseRotation = Rand.Value < 0.5f;

            startRotationSound();
        }
        else
        {
            if (clockwiseRotation)
            {
                dishRotation += DishRotationPerTick;
            }
            else
            {
                dishRotation -= DishRotationPerTick;
            }

            ticksToRotationEnd--;
            if (ticksToRotationEnd != 0)
            {
                return;
            }

            ticksToNextRotation = Rand.RangeInclusive(RotationIntervalMin, RotationIntervalMax);
            stopRotationSound();
        }
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        dishMatrix.SetTRS(Position.ToVector3ShiftedWithAltitude(AltitudeLayer.FogOfWar) + Altitudes.AltIncVect,
            dishRotation.ToQuat(), dishScale);
        Graphics.DrawMesh(MeshPool.plane10, dishMatrix, dishTexture, 0);
    }
}