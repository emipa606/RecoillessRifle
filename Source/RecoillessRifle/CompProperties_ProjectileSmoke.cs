using Verse;

namespace RecoillessRifle;

public class CompProperties_ProjectileSmoke : CompProperties
{
    public readonly float smokepopRadius = 1f;

    public CompProperties_ProjectileSmoke()
    {
        compClass = typeof(CompProjectileSmoke);
    }
}