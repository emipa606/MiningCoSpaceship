using UnityEngine;
using Verse;

namespace Spaceship;

public class Building_LandingPadBeacon : Building
{
    private Color color = Color.white;

    private Thing glower;

    private bool isPoweredOn;
    public Building_LandingPad landingPad;

    private int lightDelayInTicks;

    private int lightDurationInTicks = 10;

    private int lightPeriodInTicks = 900;

    private int nextLightStartTick;

    private int nextLightStopTick;

    public void InitializeParameters(Building_LandingPad pad, Color padColor, int periodInTicks,
        int durationInTicks, int delayInTicks)
    {
        landingPad = pad;
        SetFaction(pad.Faction);
        color = padColor;
        lightPeriodInTicks = periodInTicks;
        lightDurationInTicks = durationInTicks;
        lightDelayInTicks = delayInTicks;
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        switchOffLight();
        base.Destroy(mode);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref landingPad, "landingPad");
        Scribe_Values.Look(ref isPoweredOn, "isPoweredOn");
        Scribe_Values.Look(ref color, "color");
        Scribe_Values.Look(ref lightPeriodInTicks, "lightPeriodInTicks");
        Scribe_Values.Look(ref lightDurationInTicks, "lightDurationInTicks");
        Scribe_Values.Look(ref lightDelayInTicks, "lightDelayInTicks");
        Scribe_Values.Look(ref nextLightStartTick, "nextLightStartTick");
        Scribe_Values.Look(ref nextLightStopTick, "nextLightStopTick");
        Scribe_References.Look(ref glower, "glower");
    }

    protected override void Tick()
    {
        base.Tick();
        if (!Settings.LandingPadLightIsEnabled || !isPoweredOn)
        {
            return;
        }

        if (Find.TickManager.TicksGame >= nextLightStartTick)
        {
            nextLightStopTick = Find.TickManager.TicksGame + lightDurationInTicks;
            nextLightStartTick = Find.TickManager.TicksGame + lightPeriodInTicks;
            SwitchOnLight();
        }

        if (Find.TickManager.TicksGame >= nextLightStopTick)
        {
            switchOffLight();
        }
    }

    private void SwitchOnLight()
    {
        if (!glower.DestroyedOrNull())
        {
            return;
        }

        if (color == Color.white)
        {
            glower = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeaconGlowerWhite, Position, Map);
        }
        else if (color == Color.red)
        {
            glower = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeaconGlowerRed, Position, Map);
        }
        else if (color == Color.green)
        {
            glower = GenSpawn.Spawn(Util_ThingDefOf.LandingPadBeaconGlowerGreen, Position, Map);
        }
        else
        {
            Log.ErrorOnce($"MiningCo. spaceship: beacon color ({color}) not handled!", 123456789);
        }
    }

    private void switchOffLight()
    {
        if (glower.DestroyedOrNull())
        {
            return;
        }

        glower.Destroy();
        glower = null;
    }

    public void Notify_PowerStarted()
    {
        isPoweredOn = true;
        nextLightStartTick = Find.TickManager.TicksGame + lightDelayInTicks;
    }

    public void Notify_PowerStopped()
    {
        isPoweredOn = false;
        switchOffLight();
    }
}