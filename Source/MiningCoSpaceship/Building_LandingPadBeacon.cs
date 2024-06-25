using UnityEngine;
using Verse;

namespace Spaceship;

public class Building_LandingPadBeacon : Building
{
    public Color color = Color.white;

    public Thing glower;

    public bool isPoweredOn;
    public Building_LandingPad landingPad;

    public int lightDelayInTicks;

    public int lightDurationInTicks = 10;

    public int lightPeriodInTicks = 900;

    public int nextLightStartTick;

    public int nextLightStopTick;

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
        SwitchOffLight();
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

    public override void Tick()
    {
        base.Tick();
        if (!Settings.landingPadLightIsEnabled || !isPoweredOn)
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
            SwitchOffLight();
        }
    }

    public void SwitchOnLight()
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

    public void SwitchOffLight()
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
        SwitchOffLight();
    }
}