using System.Text;
using RimWorld;
using Verse;

namespace Spaceship;

public abstract class Building_SpaceshipDispatcher : Building_Spaceship
{
    private const int TurretsCount = 2;

    private readonly IntVec3[] turretOffsetPositions =
    [
        new(-4, 0, -2),
        new(4, 0, -2)
    ];

    private readonly Rot4[] turretOffsetRotations =
    [
        Rot4.West,
        Rot4.East
    ];

    protected override bool takeOffRequestIsEnabled => true;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (respawningAfterLoad)
        {
            return;
        }

        for (var i = 0; i < TurretsCount; i++)
        {
            var thing = ThingMaker.MakeThing(Util_ThingDefOf.VulcanTurret, ThingDefOf.Plasteel);
            thing.SetFaction(Util_Faction.MiningCoFaction);
            GenSpawn.Spawn(thing, Position + turretOffsetPositions[i].RotatedBy(Rotation), Map,
                new Rot4(Rotation.AsInt + turretOffsetRotations[i].AsInt));
        }
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        for (var i = 0; i < TurretsCount; i++)
        {
            (Position + turretOffsetPositions[i].RotatedBy(Rotation)).GetFirstThing(Map, Util_ThingDefOf.VulcanTurret)
                ?.Destroy();
        }

        base.Destroy(mode);
    }

    protected void SpawnPayment(int pawnsCount)
    {
        var quantity = Util_Spaceship.feePerPawnInSilver * pawnsCount;
        var thing = SpawnItem(ThingDefOf.Silver, null, quantity, Position, Map, 0f);
        Messages.Message("MCS.landingpadpayment".Translate(quantity), thing,
            MessageTypeDefOf.PositiveEvent);
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        if (Find.TickManager.TicksGame >= takeOffTick)
        {
            stringBuilder.Append("MCS.takeoffasap".Translate());
        }
        else
        {
            stringBuilder.Append(
                "MCS.planedtakeoff".Translate((takeOffTick - Find.TickManager.TicksGame)
                    .ToStringTicksToPeriodVerbose()));
        }

        return stringBuilder.ToString();
    }
}