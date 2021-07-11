using RimWorld;

namespace Verse
{
    // Token: 0x02000003 RID: 3
    internal class RR_Verb_ShootMultiple : RR_Verb_LaunchMultipleProjectile
    {
        // Token: 0x17000002 RID: 2
        // (get) Token: 0x06000008 RID: 8 RVA: 0x00002968 File Offset: 0x00000B68
        protected override int ShotsPerBurst => verbProps.burstShotCount;

        // Token: 0x06000009 RID: 9 RVA: 0x00002988 File Offset: 0x00000B88
        public override void WarmupComplete()
        {
            base.WarmupComplete();
            if (!base.CasterIsPawn || base.CasterPawn.skills == null)
            {
                return;
            }

            var xp = 6f;
            if (currentTarget.Thing is Pawn pawn && pawn.HostileTo(caster) && !pawn.Downed)
            {
                xp = 240f;
            }

            base.CasterPawn.skills.Learn(SkillDefOf.Shooting, xp);
        }

        // Token: 0x0600000A RID: 10 RVA: 0x00002A14 File Offset: 0x00000C14
        protected override bool TryCastShot()
        {
            if (base.TryCastShot() && base.CasterIsPawn)
            {
                base.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
            }

            return base.TryCastShot();
        }
    }
}