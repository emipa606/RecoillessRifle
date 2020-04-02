using System;
using RimWorld;

namespace Verse
{
	// Token: 0x02000003 RID: 3
	internal class RR_Verb_ShootMultiple : RR_Verb_LaunchMultipleProjectile
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000008 RID: 8 RVA: 0x00002968 File Offset: 0x00000B68
		protected override int ShotsPerBurst
		{
			get
			{
				return this.verbProps.burstShotCount;
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002988 File Offset: 0x00000B88
		public override void WarmupComplete()
		{
			base.WarmupComplete();
			bool flag = base.CasterIsPawn && base.CasterPawn.skills != null;
			if (flag)
			{
				float xp = 6f;
				Pawn pawn = this.currentTarget.Thing as Pawn;
				bool flag2 = pawn != null && pawn.HostileTo(this.caster) && !pawn.Downed;
				if (flag2)
				{
					xp = 240f;
				}
				base.CasterPawn.skills.Learn(SkillDefOf.Shooting, xp, false);
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002A14 File Offset: 0x00000C14
		protected override bool TryCastShot()
		{
			bool flag = base.TryCastShot();
			bool flag2 = flag && base.CasterIsPawn;
			if (flag2)
			{
				base.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
			}
			return flag;
		}
	}
}
