using Verse;

namespace Spaceship;

public class WeaponDef : Def
{
    public ThingDef ammoDef = null;

    public float ammoDispersion = 0f;

    public int ammoQuantity = 0;

    public float ammoTravelDistance = 0f;

    public int disableRainDurationInTicks = 0;

    public float horizontalPositionOffset = 0f;

    public bool isTwinGun = true;

    public SoundDef soundCastDef = null;

    public float startShootingDistance = 0f;

    public float targetAcquireRange = 0f;

    public int ticksBetweenShots = 0;

    public float verticalPositionOffset = 0f;
}