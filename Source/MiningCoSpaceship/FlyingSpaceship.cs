using RimWorld;
using UnityEngine;
using Verse;

namespace Spaceship;

[StaticConstructorOnStartup]
public abstract class FlyingSpaceship : Thing
{
    private static readonly Vector3 supplySpaceshipScale = new(11f, 1f, 20f);

    private static readonly Vector3 medicalSpaceshipScale = new(7f, 1f, 11f);

    private static readonly Material supplySpaceshipTexture =
        MaterialPool.MatFrom("Things/SupplySpaceship/SupplySpaceship");

    private static readonly Material dispatcherTexture = MaterialPool.MatFrom("Things/Dispatcher/DispatcherFlying");

    private static readonly Material medicalSpaceshipTexture =
        MaterialPool.MatFrom("Things/MedicalSpaceship/MedicalSpaceship");

    private static readonly Material strikeshipTexture = MaterialPool.MatFrom("Things/StrikeShip/StrikeShip");

    private static readonly Material supplySpaceshipShadowTexture =
        MaterialPool.MatFrom("Things/SupplySpaceship/SupplySpaceshipShadow", ShaderDatabase.Transparent);

    private static readonly Material medicalSpaceshipShadowTexture =
        MaterialPool.MatFrom("Things/MedicalSpaceship/MedicalSpaceshipShadow", ShaderDatabase.Transparent);

    protected Vector3 baseSpaceshipScale = new(1f, 1f, 1f);
    protected Vector3 spaceshipExactPosition = Vector3.zero;

    protected float spaceshipExactRotation;

    protected SpaceshipKind spaceshipKind = SpaceshipKind.CargoPeriodic;

    private Matrix4x4 spaceshipMatrix;

    protected Vector3 spaceshipScale = new(11f, 1f, 20f);

    protected Vector3 spaceshipShadowExactPosition = Vector3.zero;

    private Matrix4x4 spaceshipShadowMatrix;

    protected Vector3 spaceshipShadowScale = new(11f, 1f, 20f);

    private Material spaceshipShadowTexture;

    private Material spaceshipTexture;

    public override Vector3 DrawPos => spaceshipExactPosition;

    private Vector3 ShadowDrawPos => spaceshipShadowExactPosition;

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

    protected override void Tick()
    {
        ComputeShipExactPosition();
        ComputeShipShadowExactPosition();
        ComputeShipExactRotation();
        ComputeShipScale();
        SetShipPositionToBeSelectable();
    }

    protected abstract void ComputeShipExactPosition();

    protected abstract void ComputeShipShadowExactPosition();

    protected abstract void ComputeShipExactRotation();

    protected abstract void ComputeShipScale();

    protected abstract void SetShipPositionToBeSelectable();

    protected bool IsInBounds()
    {
        return DrawPos.ToIntVec3().InBounds(Map) && DrawPos.ToIntVec3().x >= 10 &&
               DrawPos.ToIntVec3().x < Map.Size.x - 10 && DrawPos.ToIntVec3().z >= 10 &&
               DrawPos.ToIntVec3().z < Map.Size.z - 10;
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        spaceshipMatrix.SetTRS(drawLoc + Altitudes.AltIncVect, spaceshipExactRotation.ToQuat(), spaceshipScale);
        Graphics.DrawMesh(MeshPool.plane10, spaceshipMatrix, spaceshipTexture, 0);
        spaceshipShadowMatrix.SetTRS(ShadowDrawPos + Altitudes.AltIncVect, spaceshipExactRotation.ToQuat(),
            spaceshipShadowScale);
        Graphics.DrawMesh(MeshPool.plane10, spaceshipShadowMatrix,
            FadedMaterialPool.FadedVersionOf(spaceshipShadowTexture, 0.4f * GenCelestial.CurShadowStrength(Map)), 0);
    }
}