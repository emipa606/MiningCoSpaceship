using RimWorld;
using UnityEngine;
using Verse;

namespace Spaceship;

[StaticConstructorOnStartup]
public abstract class FlyingSpaceship : Thing
{
    public static Vector3 supplySpaceshipScale = new Vector3(11f, 1f, 20f);

    public static Vector3 medicalSpaceshipScale = new Vector3(7f, 1f, 11f);

    public static readonly Material supplySpaceshipTexture =
        MaterialPool.MatFrom("Things/SupplySpaceship/SupplySpaceship");

    public static readonly Material dispatcherTexture = MaterialPool.MatFrom("Things/Dispatcher/DispatcherFlying");

    public static readonly Material medicalSpaceshipTexture =
        MaterialPool.MatFrom("Things/MedicalSpaceship/MedicalSpaceship");

    public static readonly Material strikeshipTexture = MaterialPool.MatFrom("Things/StrikeShip/StrikeShip");

    public static readonly Material supplySpaceshipShadowTexture =
        MaterialPool.MatFrom("Things/SupplySpaceship/SupplySpaceshipShadow", ShaderDatabase.Transparent);

    public static readonly Material medicalSpaceshipShadowTexture =
        MaterialPool.MatFrom("Things/MedicalSpaceship/MedicalSpaceshipShadow", ShaderDatabase.Transparent);

    public Vector3 baseSpaceshipScale = new Vector3(1f, 1f, 1f);
    public Vector3 spaceshipExactPosition = Vector3.zero;

    public float spaceshipExactRotation;

    public SpaceshipKind spaceshipKind = SpaceshipKind.CargoPeriodic;

    public Matrix4x4 spaceshipMatrix;

    public Vector3 spaceshipScale = new Vector3(11f, 1f, 20f);

    public Vector3 spaceshipShadowExactPosition = Vector3.zero;

    public Matrix4x4 spaceshipShadowMatrix;

    public Vector3 spaceshipShadowScale = new Vector3(11f, 1f, 20f);

    public Material spaceshipShadowTexture;

    public Material spaceshipTexture;

    public override Vector3 DrawPos => spaceshipExactPosition;

    public Vector3 ShadowDrawPos => spaceshipShadowExactPosition;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (respawningAfterLoad)
        {
            ConfigureShipTexture(spaceshipKind);
        }
    }

    protected void ConfigureShipTexture(SpaceshipKind kind)
    {
        switch (kind)
        {
            case SpaceshipKind.CargoPeriodic:
            case SpaceshipKind.CargoRequested:
            case SpaceshipKind.Damaged:
                spaceshipTexture = supplySpaceshipTexture;
                spaceshipShadowTexture = supplySpaceshipShadowTexture;
                baseSpaceshipScale = supplySpaceshipScale;
                break;
            case SpaceshipKind.DispatcherDrop:
            case SpaceshipKind.DispatcherPick:
                spaceshipTexture = dispatcherTexture;
                spaceshipShadowTexture = supplySpaceshipShadowTexture;
                baseSpaceshipScale = supplySpaceshipScale;
                break;
            case SpaceshipKind.Medical:
                spaceshipTexture = medicalSpaceshipTexture;
                spaceshipShadowTexture = medicalSpaceshipShadowTexture;
                baseSpaceshipScale = medicalSpaceshipScale;
                break;
            case SpaceshipKind.Airstrike:
                spaceshipTexture = strikeshipTexture;
                spaceshipShadowTexture = supplySpaceshipShadowTexture;
                baseSpaceshipScale = supplySpaceshipScale;
                break;
            default:
                Log.ErrorOnce($"MiningCo. Spaceship: unhandled SpaceshipKind ({spaceshipKind}).", 123456784);
                break;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref spaceshipExactPosition, "spaceshipExactPosition");
        Scribe_Values.Look(ref spaceshipShadowExactPosition, "spaceshipShadowExactPosition");
        Scribe_Values.Look(ref spaceshipExactRotation, "spaceshipExactRotation");
        Scribe_Values.Look(ref spaceshipKind, "spaceshipKind");
        Scribe_Values.Look(ref spaceshipScale, "spaceshipScale");
        Scribe_Values.Look(ref spaceshipShadowScale, "spaceshipShadowScale");
    }

    public override void Tick()
    {
        ComputeShipExactPosition();
        ComputeShipShadowExactPosition();
        ComputeShipExactRotation();
        ComputeShipScale();
        SetShipPositionToBeSelectable();
    }

    public abstract void ComputeShipExactPosition();

    public abstract void ComputeShipShadowExactPosition();

    public abstract void ComputeShipExactRotation();

    public abstract void ComputeShipScale();

    public abstract void SetShipPositionToBeSelectable();

    public bool IsInBounds()
    {
        return DrawPos.ToIntVec3().InBounds(Map) && DrawPos.ToIntVec3().x >= 10 &&
               DrawPos.ToIntVec3().x < Map.Size.x - 10 && DrawPos.ToIntVec3().z >= 10 &&
               DrawPos.ToIntVec3().z < Map.Size.z - 10;
    }

    public override void Draw()
    {
        spaceshipMatrix.SetTRS(DrawPos + Altitudes.AltIncVect, spaceshipExactRotation.ToQuat(), spaceshipScale);
        Graphics.DrawMesh(MeshPool.plane10, spaceshipMatrix, spaceshipTexture, 0);
        spaceshipShadowMatrix.SetTRS(ShadowDrawPos + Altitudes.AltIncVect, spaceshipExactRotation.ToQuat(),
            spaceshipShadowScale);
        Graphics.DrawMesh(MeshPool.plane10, spaceshipShadowMatrix,
            FadedMaterialPool.FadedVersionOf(spaceshipShadowTexture, 0.4f * GenCelestial.CurShadowStrength(Map)), 0);
    }
}