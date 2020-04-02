using System;
using UnityEngine;
using Verse;

namespace RecoillessRifle
{
	// Token: 0x0200000B RID: 11
	public class CompProperties_TurretTopSize : CompProperties
	{
		// Token: 0x06000023 RID: 35 RVA: 0x00002FE1 File Offset: 0x000011E1
		public CompProperties_TurretTopSize()
		{
			this.compClass = typeof(CompTurretTopSize);
		}

		// Token: 0x0400000F RID: 15
		public Vector3 topSize = Vector3.one;
	}
}
