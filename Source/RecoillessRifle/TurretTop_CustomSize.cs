using UnityEngine;
using Verse;

namespace RecoillessRifle;

public class TurretTop_CustomSize
{
    private const float IdleTurnDegreesPerTick = 0.26f;

    private const int IdleTurnDuration = 140;

    private const int IdleTurnIntervalMin = 150;

    private const int IdleTurnIntervalMax = 350;

    private readonly Building_TurretGunCustom parentTurret;

    private float curRotationInt;

    private bool idleTurnClockwise;

    private int idleTurnTicksLeft;

    private int ticksUntilIdleTurn;

    public TurretTop_CustomSize(Building_TurretGunCustom ParentTurret)
    {
        parentTurret = ParentTurret;
    }

    private float CurRotation
    {
        get => curRotationInt;
        set
        {
            curRotationInt = value;
            if (curRotationInt > 360.0)
            {
                curRotationInt -= 360f;
            }

            if (curRotationInt < 0.0)
            {
                curRotationInt += 360f;
            }
        }
    }

    public void TurretTopTick()
    {
        var currentTarget = parentTurret.CurrentTarget;
        var isValid = currentTarget.IsValid;
        if (isValid)
        {
            CurRotation = (currentTarget.Cell.ToVector3Shifted() - parentTurret.DrawPos).AngleFlat();
            ticksUntilIdleTurn = Rand.RangeInclusive(150, 350);
        }
        else
        {
            if (ticksUntilIdleTurn > 0)
            {
                ticksUntilIdleTurn--;
                if (ticksUntilIdleTurn != 0)
                {
                    return;
                }

                idleTurnClockwise = Rand.Value < 0.5;

                idleTurnTicksLeft = 140;
            }
            else
            {
                if (idleTurnClockwise)
                {
                    CurRotation += 0.26f;
                }
                else
                {
                    CurRotation -= 0.26f;
                }

                idleTurnTicksLeft--;
                if (idleTurnTicksLeft <= 0)
                {
                    ticksUntilIdleTurn = Rand.RangeInclusive(150, 350);
                }
            }
        }
    }

    public void DrawTurret()
    {
        var matrix4x = default(Matrix4x4);
        matrix4x.SetTRS(parentTurret.DrawPos + Altitudes.AltIncVect, CurRotation.ToQuat(),
            parentTurret.TopSizeComp?.Props.topSize ?? Vector3.one);
        Graphics.DrawMesh(MeshPool.plane20, matrix4x, parentTurret.def.building.turretTopMat, 0);
    }
}