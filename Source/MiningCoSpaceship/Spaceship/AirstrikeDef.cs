using System.Collections.Generic;
using Verse;

namespace Spaceship;

public class AirstrikeDef : Def
{
    public const int maxWeapons = 3;

    public float ammoResupplyDays = 1f;

    public float cellsTravelledPerTick = 0.25f;

    public int costInSilver = 500;

    public int runsNumber = 1;

    public int ticksAfterOverflightFinalValue = 600;

    public int ticksAfterOverflightReducedSpeed = 0;

    public int ticksBeforeOverflightInitialValue = 600;

    public int ticksBeforeOverflightPlaySound = 240;

    public int ticksBeforeOverflightReducedSpeed = 240;

    public List<WeaponDef> weapons = new List<WeaponDef>();
}