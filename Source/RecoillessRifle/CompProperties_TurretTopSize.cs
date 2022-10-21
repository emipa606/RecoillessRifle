using UnityEngine;
using Verse;

namespace RecoillessRifle;

public class CompProperties_TurretTopSize : CompProperties
{
    public Vector3 topSize = Vector3.one;

    public CompProperties_TurretTopSize()
    {
        compClass = typeof(CompTurretTopSize);
    }
}