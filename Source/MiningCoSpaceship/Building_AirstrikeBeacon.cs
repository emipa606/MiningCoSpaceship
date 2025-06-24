using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Spaceship;

public class Building_AirstrikeBeacon : Building
{
    private const int TicksBetweenRuns = 900;

    private const int FireStartCheckPeriodInTicks = 60;
    private AirstrikeDef airStrikeDef;

    private int nextStrikeTick;

    private int remainingRuns;

    public void InitializeAirstrike(IntVec3 targetPosition, AirstrikeDef airStrikeDef)
    {
        Position = targetPosition;
        this.airStrikeDef = airStrikeDef;
        remainingRuns = this.airStrikeDef.runsNumber;
        nextStrikeTick = Find.TickManager.TicksGame + 900;
        GetComp<CompOrbitalBeam>().StartAnimation(remainingRuns * TicksBetweenRuns, 10, Rand.Range(-12f, 12f));
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref airStrikeDef, "airStrikeDef");
        Scribe_Values.Look(ref nextStrikeTick, "nextStrikeTick");
        Scribe_Values.Look(ref remainingRuns, "runNumber");
    }

    protected override void Tick()
    {
        base.Tick();
        if (Find.TickManager.TicksGame % FireStartCheckPeriodInTicks == 0)
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
            nextStrikeTick = Find.TickManager.TicksGame + TicksBetweenRuns;
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
        const int num = 700000104;
        var commandAction = new Command_Action
        {
            icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack"),
            defaultLabel = "MCS.setnewtarget".Translate(),
            defaultDesc = "MCS.setnewtargetTT".Translate(),
            activateSound = SoundDefOf.Click,
            action = selectNewAirstrikeTarget,
            groupKey = num + 1
        };
        list.Add(commandAction);
        var gizmos = base.GetGizmos();
        return gizmos != null ? gizmos.Concat(list) : list;
    }

    private void selectNewAirstrikeTarget()
    {
        Util_Misc.SelectAirstrikeTarget(Map, setNewAirstrikeTarget);
    }

    private void setNewAirstrikeTarget(LocalTargetInfo targetPosition)
    {
        Position = targetPosition.Cell;
        nextStrikeTick = Find.TickManager.TicksGame + TicksBetweenRuns;
        Messages.Message("MCS.newairstrikedesignated".Translate(), this, MessageTypeDefOf.CautionInput);
        GetComp<CompOrbitalBeam>().StartAnimation(remainingRuns * TicksBetweenRuns, 10, Rand.Range(-12f, 12f));
    }
}