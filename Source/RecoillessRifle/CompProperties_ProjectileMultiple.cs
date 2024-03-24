using Verse;

namespace RecoillessRifle;

internal class CompProperties_ProjectileMultiple : CompProperties
{
    public readonly float forsedScatterRadius = 0f;

    public readonly int pellets = 1;

    public readonly float scatterRadiusAt10tilesAway = 0f;

    public CompProperties_ProjectileMultiple()
    {
        compClass = typeof(CompProjectileMultiple);
    }
}