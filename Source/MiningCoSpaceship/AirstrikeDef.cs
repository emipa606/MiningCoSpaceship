using System.Collections.Generic;
using Verse;

namespace Spaceship;

public class AirstrikeDef : Def
{
    public const int maxWeapons = 3;

    public readonly float ammoResupplyDays = 1f;

    public readonly float cellsTravelledPerTick = 0.25f;

    public readonly int costInSilver = 500;

    public readonly int runsNumber = 1;

    public readonly int ticksAfterOverflightFinalValue = 600;

    public readonly int ticksAfterOverflightReducedSpeed = 0;

    public readonly int ticksBeforeOverflightInitialValue = 600;

    public readonly int ticksBeforeOverflightPlaySound = 240;

    public readonly int ticksBeforeOverflightReducedSpeed = 240;

    public readonly List<WeaponDef> weapons = new List<WeaponDef>();
}