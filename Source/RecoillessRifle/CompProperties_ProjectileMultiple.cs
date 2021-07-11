using Verse;

namespace RecoillessRifle
{
    // Token: 0x02000009 RID: 9
    internal class CompProperties_ProjectileMultiple : CompProperties
    {
        // Token: 0x0400000D RID: 13
        public float forsedScatterRadius = 0f;

        // Token: 0x0400000C RID: 12
        public int pellets = 1;

        // Token: 0x0400000E RID: 14
        public float scatterRadiusAt10tilesAway = 0f;

        // Token: 0x06000020 RID: 32 RVA: 0x00002F8B File Offset: 0x0000118B
        public CompProperties_ProjectileMultiple()
        {
            compClass = typeof(CompProjectileMultiple);
        }
    }
}