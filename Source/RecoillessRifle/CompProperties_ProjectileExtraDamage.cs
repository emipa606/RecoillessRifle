using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RecoillessRifle
{
	// Token: 0x02000008 RID: 8
	public class CompProperties_ProjectileExtraDamage : CompProperties
	{
		// Token: 0x0600001F RID: 31 RVA: 0x00002F28 File Offset: 0x00001128
		public CompProperties_ProjectileExtraDamage()
		{
			this.compClass = typeof(CompProjectileExtraDamage);
		}

		// Token: 0x04000008 RID: 8
		public string hitText = "RR_Hit";

		// Token: 0x04000009 RID: 9
		public Color hitTextColor = new Color32(byte.MaxValue, 153, 102, byte.MaxValue);

		// Token: 0x0400000A RID: 10
		public int damageAmountBase = 1;

		// Token: 0x0400000B RID: 11
		public DamageDef damageDef = DamageDefOf.Bullet;
	}
}
