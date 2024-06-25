using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Spaceship;

public class Building_AirstrikeBeacon : Building
{
    public const int ticksBetweenRuns = 900;

    public const int fireStartCheckPeriodInTicks = 60;
    public AirstrikeDef airStrikeDef;

    public int nextStrikeTick;

    public int remainingRuns;

    public void InitializeAirstrike(IntVec3 targetPosition, AirstrikeDef airStrikeDef)
    {
        Position = targetPosition;
        this.airStrikeDef = airStrikeDef;
        remainingRuns = this.airStrikeDef.runsNumber;
        nextStrikeTick = Find.TickManager.TicksGame + 900;
        GetComp<CompOrbitalBeam>().StartAnimation(remainingRuns * ticksBetweenRuns, 10, Rand.Range(-12f, 12f));
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref airStrikeDef, "airStrikeDef");
        Scribe_Values.Look(ref nextStrikeTick, "nextStrikeTick");
        Scribe_Values.Look(ref remainingRuns, "runNumber");
    }

    public override void Tick()
    {
        base.Tick();
        if (Find.TickManager.TicksGame % fireStartCheckPeriodInTicks == 0)
        {
            var c = (Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1.5f)).ToIntVec3();
            if (c.InBounds(Map))
            {
                FireUtility.TryStartFireIn(Position, Map, 0.1f, this);
            }
        }

        if (remainingRuns > 0 && Find.TickManager.TicksGame >= nextStrikeTick)
        {
            Util_Spaceship.SpawnStrikeShip(Map, Position, airStrikeDef, Faction);
            remainingRuns--;
            nextStrikeTick = Find.TickManager.TicksGame + ticksBetweenRuns;
        }

        if (remainingRuns == 0)
        {
            Destroy();
        }
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        var num = nextStrikeTick - Find.TickManager.TicksGame;
        if (num > 0)
        {
            stringBuilder.Append("MCS.nextstrike".Translate(num.ToStringSecondsFromTicks()));
        }

        return stringBuilder.ToString();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        IList<Gizmo> list = new List<Gizmo>();
        var num = 700000104;
        var command_Action = new Command_Action
        {
            icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack"),
            defaultLabel = "MCS.setnewtarget".Translate(),
            defaultDesc = "MCS.setnewtargetTT".Translate(),
            activateSound = SoundDefOf.Click,
            action = SelectNewAirstrikeTarget,
            groupKey = num + 1
        };
        list.Add(command_Action);
        var gizmos = base.GetGizmos();
        return gizmos != null ? gizmos.Concat(list) : list;
    }

    public void SelectNewAirstrikeTarget()
    {
        Util_Misc.SelectAirstrikeTarget(Map, SetNewAirstrikeTarget);
    }

    public void SetNewAirstrikeTarget(LocalTargetInfo targetPosition)
    {
        Position = targetPosition.Cell;
        nextStrikeTick = Find.TickManager.TicksGame + ticksBetweenRuns;
        Messages.Message("MCS.newairstrikedesignated".Translate(), this, MessageTypeDefOf.CautionInput);
        GetComp<CompOrbitalBeam>().StartAnimation(remainingRuns * ticksBetweenRuns, 10, Rand.Range(-12f, 12f));
    }
}