using Verse;

namespace RecoillessRifle;

internal class CompProperties_ProjectileMultiple : CompProperties
{
    public float forsedScatterRadius = 0f;

    public int pellets = 1;

    public float scatterRadiusAt10tilesAway = 0f;

    public CompProperties_ProjectileMultiple()
    {
        compClass = typeof(CompProjectileMultiple);
    }
}