using UnityEngine;
using Verse;

namespace RecoillessRifle
{
    // Token: 0x0200000E RID: 14
    public class TurretTop_CustomSize
    {
        // Token: 0x04000018 RID: 24
        private const float IdleTurnDegreesPerTick = 0.26f;

        // Token: 0x04000019 RID: 25
        private const int IdleTurnDuration = 140;

        // Token: 0x0400001A RID: 26
        private const int IdleTurnIntervalMin = 150;

        // Token: 0x0400001B RID: 27
        private const int IdleTurnIntervalMax = 350;

        // Token: 0x04000013 RID: 19
        private readonly Building_TurretGunCustom parentTurret;

        // Token: 0x04000014 RID: 20
        private float curRotationInt;

        // Token: 0x04000017 RID: 23
        private bool idleTurnClockwise;

        // Token: 0x04000016 RID: 22
        private int idleTurnTicksLeft;

        // Token: 0x04000015 RID: 21
        private int ticksUntilIdleTurn;

        // Token: 0x06000032 RID: 50 RVA: 0x00003689 File Offset: 0x00001889
        public TurretTop_CustomSize(Building_TurretGunCustom ParentTurret)
        {
            parentTurret = ParentTurret;
        }

        // Token: 0x1700000D RID: 13
        // (get) Token: 0x06000030 RID: 48 RVA: 0x00003608 File Offset: 0x00001808
        // (set) Token: 0x06000031 RID: 49 RVA: 0x00003620 File Offset: 0x00001820
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

        // Token: 0x06000033 RID: 51 RVA: 0x0000369C File Offset: 0x0000189C
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

        // Token: 0x06000034 RID: 52 RVA: 0x000037E0 File Offset: 0x000019E0
        public void DrawTurret()
        {
            var matrix4x = default(Matrix4x4);
            matrix4x.SetTRS(parentTurret.DrawPos + Altitudes.AltIncVect, CurRotation.ToQuat(),
                parentTurret.TopSizeComp?.Props.topSize ?? Vector3.one);
            Graphics.DrawMesh(MeshPool.plane20, matrix4x, parentTurret.def.building.turretTopMat, 0);
        }
    }
}