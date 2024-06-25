using Verse;

namespace Spaceship;

public class WeaponDef : Def
{
    public readonly ThingDef ammoDef = null;

    public readonly int ammoQuantity = 0;

    public readonly float ammoTravelDistance = 0f;

    public readonly int disableRainDurationInTicks = 0;

    public readonly float horizontalPositionOffset = 0f;

    public readonly bool isTwinGun = true;

    public readonly SoundDef soundCastDef = null;

    public readonly float startShootingDistance = 0f;

    public readonly float targetAcquireRange = 0f;

    public readonly int ticksBetweenShots = 0;

    public readonly float verticalPositionOffset = 0f;

    public float ammoDispersion = 0f;
}