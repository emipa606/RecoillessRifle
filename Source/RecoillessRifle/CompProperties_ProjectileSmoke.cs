using Verse;

namespace RecoillessRifle
{
    // Token: 0x02000007 RID: 7
    public class CompProperties_ProjectileSmoke : CompProperties
    {
        // Token: 0x04000007 RID: 7
        public float smokepopRadius = 1f;

        // Token: 0x0600001E RID: 30 RVA: 0x00002F01 File Offset: 0x00001101
        public CompProperties_ProjectileSmoke()
        {
            compClass = typeof(CompProjectileSmoke);
        }
    }
}