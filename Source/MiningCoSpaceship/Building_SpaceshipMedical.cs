using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceship;

public class Building_SpaceshipMedical : Building_SpaceshipCargo
{
    public const int medicsNumber = 4;

    public const int tendableColonistCheckPeriodInTicks = 240;

    public const int staffAboardHealPeriodInTicks = 2500;

    public const int orbitalHealingPawnsAboardMaxCount = 6;

    public int availableMedikitsCount = 40;

    private List<Pawn> medics = new List<Pawn>();

    public int nextStaffAboardHealTick;

    public int nextTendableColonistCheckTick;

    public int orbitalHealingPawnsAboardCount;

    public void InitializeData_Medical(Faction faction, int hitPoints, int landingDuration, SpaceshipKind kind)
    {
        InitializeData_Cargo(faction, hitPoints, landingDuration, kind);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (respawningAfterLoad)
        {
            return;
        }

        for (var i = 0; i < medicsNumber; i++)
        {
            var item = MiningCoPawnGenerator.GeneratePawn(Util_PawnKindDefOf.Medic, Map);
            medics.Add(item);
            pawnsAboard.Add(item);
        }
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        if (mode != DestroyMode.KillFinalize)
        {
            var num = HitPoints / (float)MaxHitPoints;
            if (num < 1f)
            {
                var num2 = Mathf.RoundToInt(600000f * (1f - num));
                Util_Misc.Partnership.nextMedicalSupplyMinTick[Map] += num2;
                Find.LetterStack.ReceiveLetter("MCS.damagedmedictitle".Translate(), "MCS.damagedmedic".Translate(),
                    LetterDefOf.NegativeEvent,
                    new TargetInfo(Position, Map));
            }

            if (Util_Faction.MiningCoFaction.HostileTo(Faction.OfPlayer))
            {
                EjectPlayerPawns();
            }

            var list = new List<Pawn>();
            foreach (var item in pawnsAboard)
            {
                if (item.Faction == Faction.OfPlayer)
                {
                    Util_Misc.OrbitalHealing.Notify_PawnStartingOrbitalHealing(item, Map);
                }
                else
                {
                    list.Add(item);
                }
            }

            pawnsAboard = list.ListFullCopy();
        }

        base.Destroy(mode);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref medics, "medics", LookMode.Reference);
        Scribe_Values.Look(ref availableMedikitsCount, "availableMedikitsCount");
        Scribe_Values.Look(ref nextStaffAboardHealTick, "nextStaffAboardHealTick");
        Scribe_Values.Look(ref orbitalHealingPawnsAboardCount, "orbitalHealingPawnsAboardCount");
    }

    public override void Tick()
    {
        base.Tick();
        HealStaffAboard();
        SendMedicIfNeeded();
    }

    public void HealStaffAboard()
    {
        if (Find.TickManager.TicksGame < nextStaffAboardHealTick)
        {
            return;
        }

        nextStaffAboardHealTick = Find.TickManager.TicksGame + staffAboardHealPeriodInTicks;
        foreach (var item in pawnsAboard)
        {
            if (item.Faction != Util_Faction.MiningCoFaction || !item.health.HasHediffsNeedingTend())
            {
                continue;
            }

            var hediff = item.health.hediffSet.GetHediffsTendable().RandomElement();
            item.health.RemoveHediff(hediff);
        }
    }

    public void SendMedicIfNeeded()
    {
        if (Find.TickManager.TicksGame < nextTendableColonistCheckTick)
        {
            return;
        }

        nextTendableColonistCheckTick = Find.TickManager.TicksGame + tendableColonistCheckPeriodInTicks;
        if (IsTakeOffImminent(30000))
        {
            return;
        }

        Pawn pawn = null;
        foreach (var item in medics.InRandomOrder())
        {
            if (!pawnsAboard.Contains(item) || item.health.HasHediffsNeedingTend())
            {
                continue;
            }

            pawn = item;
            break;
        }

        if (pawn == null)
        {
            return;
        }

        var tendableColonist = JobGiver_HealColonists.GetTendableColonist(Position, Map);
        if (tendableColonist == null)
        {
            return;
        }

        if (availableMedikitsCount > 0)
        {
            var medicineCountToFullyHeal = Medicine.GetMedicineCountToFullyHeal(tendableColonist);
            var num = Math.Min(availableMedikitsCount, medicineCountToFullyHeal);
            if (num > 0)
            {
                SpawnItem(ThingDefOf.MedicineIndustrial, null, num, Position, Map, 0f);
                availableMedikitsCount -= num;
            }
        }

        GenSpawn.Spawn(pawn, Position, Map);
        pawnsAboard.Remove(pawn);
        var lord = LordMaker.MakeNewLord(Util_Faction.MiningCoFaction, new LordJob_HealColonists(Position), Map);
        lord.AddPawn(pawn);
    }

    public override void Notify_PawnBoarding(Pawn pawn, bool isLastLordPawn)
    {
        base.Notify_PawnBoarding(pawn, isLastLordPawn);
        if (pawn.kindDef == Util_PawnKindDefOf.Medic)
        {
            medics.Add(pawn);
        }

        if (pawn.Faction == Faction.OfPlayer)
        {
            orbitalHealingPawnsAboardCount++;
        }
    }

    public override void RequestTakeOff()
    {
        base.RequestTakeOff();
        foreach (var medic in medics)
        {
            if (medic.Dead || pawnsAboard.Contains(medic))
            {
                continue;
            }

            {
                foreach (var lord in Map.lordManager.lords)
                {
                    if (lord.LordJob is not LordJob_HealColonists)
                    {
                        continue;
                    }

                    takeOffTick = Find.TickManager.TicksGame + Util_Spaceship.medicsRecallBeforeTakeOffMarginInTicks;
                    lord.ReceiveMemo("TakeOffImminent");
                }

                break;
            }
        }
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
    {
        var list = new List<FloatMenuOption>();
        if (!selPawn.CanReach(this, PathEndMode.ClosestTouch, Danger.Some))
        {
            return GetFloatMenuOptionsCannotReach(selPawn);
        }

        foreach (var floatMenuOption2 in base.GetFloatMenuOptions(selPawn))
        {
            list.Add(floatMenuOption2);
        }

        if (this.IsBurning())
        {
            var item = new FloatMenuOption("CannotUseReason".Translate("BurningLower".Translate()), null);
            list.Add(item);
        }
        else
        {
            if (!Util_Misc.OrbitalHealing.HasAnyTreatableHediff(selPawn))
            {
                return list;
            }

            FloatMenuOption floatMenuOption;
            if (orbitalHealingPawnsAboardCount >= orbitalHealingPawnsAboardMaxCount)
            {
                var label = "MCS.boardmedicnoroom".Translate();
                floatMenuOption = new FloatMenuOption(label, null);
            }
            else if (TradeUtility.ColonyHasEnoughSilver(Map, Util_Spaceship.orbitalHealingCost))
            {
                void Action()
                {
                    var job = JobMaker.MakeJob(Util_JobDefOf.JobDef_BoardMedicalSpaceship, this);
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }

                var label2 = "MCS.boardmedic".Translate(Util_Spaceship.orbitalHealingCost);
                floatMenuOption = new FloatMenuOption(label2, Action);
            }
            else
            {
                var label3 = "MCS.boardmedicnosilver".Translate(Util_Spaceship.orbitalHealingCost);
                floatMenuOption = new FloatMenuOption(label3, null);
            }

            list.Add(floatMenuOption);
        }

        return list;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        IList<Gizmo> list = new List<Gizmo>();
        var num = 700000113;
        var command_Action = new Command_Action
        {
            icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject"),
            defaultLabel = "MCS.disembark".Translate(),
            defaultDesc = "MCS.disembarkTT".Translate(),
            activateSound = SoundDefOf.Click,
            action = EjectPlayerPawns,
            groupKey = num + 1
        };
        list.Add(command_Action);
        var gizmos = base.GetGizmos();
        return gizmos != null ? gizmos.Concat(list) : list;
    }

    public void EjectPlayerPawns()
    {
        var list = new List<Pawn>();
        foreach (var item in pawnsAboard)
        {
            if (item.Faction == Faction.OfPlayer)
            {
                orbitalHealingPawnsAboardCount--;
                GenSpawn.Spawn(item, Position + IntVec3Utility.RandomHorizontalOffset(5f), Map);
                if (!Util_Faction.MiningCoFaction.HostileTo(Faction.OfPlayer))
                {
                    SpawnItem(ThingDefOf.Silver, null, Util_Spaceship.orbitalHealingCost, Position, Map, 5f);
                }
            }
            else
            {
                list.Add(item);
            }
        }

        pawnsAboard = list;
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        stringBuilder.AppendLine();
        if (orbitalHealingPawnsAboardCount > 0)
        {
            stringBuilder.Append("MCS.pawnsaboardmedic".Translate());
            var stringBuilder2 = new StringBuilder();
            foreach (var item in pawnsAboard)
            {
                if (item.Faction == Faction.OfPlayer)
                {
                    stringBuilder2.AppendWithComma(item.Name.ToStringShort);
                }
            }

            stringBuilder.Append(stringBuilder2);
        }
        else
        {
            stringBuilder.Append("MCS.pawnsaboardmedicnone".Translate());
        }

        return stringBuilder.ToString();
    }
}