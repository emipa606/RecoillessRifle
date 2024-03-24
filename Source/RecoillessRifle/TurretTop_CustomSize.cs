using UnityEngine;
using Verse;

namespace RecoillessRifle;

public class TurretTop_CustomSize(Building_TurretGunCustom ParentTurret)
{
    private const float IdleTurnDegreesPerTick = 0.26f;

    private const int IdleTurnDuration = 140;

    private const int IdleTurnIntervalMin = 150;

    private const int IdleTurnIntervalMax = 350;

    private float curRotationInt;

    private bool idleTurnClockwise;

    private int idleTurnTicksLeft;

    private int ticksUntilIdleTurn;

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
        var currentTarget = ParentTurret.CurrentTarget;
        var isValid = currentTarget.IsValid;
        if (isValid)
        {
            CurRotation = (currentTarget.Cell.ToVector3Shifted() - ParentTurret.DrawPos).AngleFlat();
            ticksUntilIdleTurn = Rand.RangeInclusive(IdleTurnIntervalMin, IdleTurnIntervalMax);
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

                idleTurnTicksLeft = IdleTurnDuration;
            }
            else
            {
                if (idleTurnClockwise)
                {
                    CurRotation += IdleTurnDegreesPerTick;
                }
                else
                {
                    CurRotation -= IdleTurnDegreesPerTick;
                }

                idleTurnTicksLeft--;
                if (idleTurnTicksLeft <= 0)
                {
                    ticksUntilIdleTurn = Rand.RangeInclusive(IdleTurnIntervalMin, IdleTurnIntervalMax);
                }
            }
        }
    }

    public void DrawTurret()
    {
        var matrix4x = default(Matrix4x4);
        matrix4x.SetTRS(ParentTurret.DrawPos + Altitudes.AltIncVect, CurRotation.ToQuat(),
            ParentTurret.TopSizeComp?.Props.topSize ?? Vector3.one);
        Graphics.DrawMesh(MeshPool.plane20, matrix4x, ParentTurret.def.building.turretTopMat, 0);
    }
}