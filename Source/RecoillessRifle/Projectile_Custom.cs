using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RecoillessRifle
{
	// Token: 0x0200000D RID: 13
	public class Projectile_Custom : Projectile
	{
		// Token: 0x06000026 RID: 38 RVA: 0x00003025 File Offset: 0x00001225
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksToDetonation, "ticksToDetonation", 0, false);
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00003042 File Offset: 0x00001242
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.extraDamageComp = base.GetComp<CompProjectileExtraDamage>();
			this.smokepopComp = base.GetComp<CompProjectileSmoke>();
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00003068 File Offset: 0x00001268
		public override void Tick()
		{
			base.Tick();
			bool flag = this.ticksToDetonation > 0;
			if (flag)
			{
				this.ticksToDetonation--;
				bool flag2 = this.ticksToDetonation <= 0;
				if (flag2)
				{
					this.Explode();
				}
			}
		}

		// Token: 0x06000029 RID: 41 RVA: 0x000030B4 File Offset: 0x000012B4
		protected override void Impact(Thing hitThing)
		{
			Map map = base.Map;
			bool flag = map == null;
			if (flag)
			{
				Log.Error("map is null!");
			}
			bool flag2 = this.smokepopComp != null;
			if (flag2)
			{
				this.ImpactPopSmoke(hitThing);
			}
			bool flag3 = this.def.projectile.explosionRadius > 0f;
			if (flag3)
			{
				bool flag4 = this.extraDamageComp != null;
				if (flag4)
				{
					this.ImpactExtra(hitThing, map);
				}
				this.ImpactExplode(hitThing);
			}
			else
			{
				this.ImpactDirectly(hitThing, map);
				bool flag5 = this.extraDamageComp != null;
				if (flag5)
				{
					this.ImpactExtra(hitThing, map);
				}
			}
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00003158 File Offset: 0x00001358
		protected virtual void ImpactPopSmoke(Thing hitThing)
		{
			IntVec3 position = base.Position;
			Map map = base.Map;
			float num = 1f + this.smokepopComp.Props.smokepopRadius;
			DamageDef smoke = DamageDefOf.Smoke;
			Thing thing = null;
			ThingDef gas_Smoke = ThingDefOf.Gas_Smoke;
			GenExplosion.DoExplosion(position, map, num, smoke, thing, -1, -1, null, null, null, null, gas_Smoke, 1f, 1, false, null, 0f, 1, 0f, false);
		}

		// Token: 0x0600002B RID: 43 RVA: 0x000031C4 File Offset: 0x000013C4
		protected virtual void ImpactExtra(Thing hitThing, Map map)
		{
			bool flag = hitThing != null;
			if (flag)
			{
				int damageAmountBase = this.extraDamageComp.Props.damageAmountBase;
				DamageDef damageDef = this.extraDamageComp.Props.damageDef;
				float y = this.ExactRotation.eulerAngles.y;
				Thing launcher = this.launcher;
				ThingDef equipmentDef = this.equipmentDef;
				DamageInfo damageInfo = new DamageInfo(damageDef, damageAmountBase, 0, y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown);
				hitThing.TakeDamage(damageInfo);
				bool flag2 = hitThing.def.category == ThingCategory.Pawn;
				if (flag2)
				{
					MoteMaker.ThrowText(new Vector3((float)base.Position.x + 1f, (float)base.Position.y, (float)base.Position.z + 1f), map, Translator.Translate(this.extraDamageComp.Props.hitText), this.extraDamageComp.Props.hitTextColor, -1f);
				}
			}
		}

		// Token: 0x0600002C RID: 44 RVA: 0x000032BC File Offset: 0x000014BC
		protected virtual void ImpactDirectly(Thing hitThing, Map map)
		{
			base.Impact(hitThing);
			BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(this.launcher, hitThing, this.intendedTarget.Thing, this.equipmentDef, this.def, this.targetCoverDef);
			Find.BattleLog.Add(battleLogEntry_RangedImpact);
			bool flag = hitThing != null;
			if (flag)
			{
				int damageAmountBase = this.def.projectile.GetDamageAmount(this.launcher);
				DamageDef damageDef = this.def.projectile.damageDef;
				float y = this.ExactRotation.eulerAngles.y;
				Thing launcher = this.launcher;
				ThingDef equipmentDef = this.equipmentDef;
				DamageInfo damageInfo = new DamageInfo(damageDef, damageAmountBase, 0, y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown);
				hitThing.TakeDamage(damageInfo).AssociateWithLog(battleLogEntry_RangedImpact);
			}
			else
			{
				SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(base.Position, map, false));
				MoteMaker.MakeStaticMote(this.ExactPosition, map, ThingDefOf.Mote_ShotHit_Dirt, 1f);
				bool takeSplashes = base.Position.GetTerrain(map).takeSplashes;
				if (takeSplashes)
				{
					MoteMaker.MakeWaterSplash(this.ExactPosition, map, (float)((double)Mathf.Sqrt((float)this.def.projectile.GetDamageAmount(this.launcher)) * 1.0), 4f);
				}
			}
		}

		// Token: 0x0600002D RID: 45 RVA: 0x000033F8 File Offset: 0x000015F8
		protected virtual void ImpactExplode(Thing hitThing)
		{
			bool flag = this.def.projectile.explosionDelay == 0;
			if (flag)
			{
				this.Explode();
			}
			else
			{
				this.landed = true;
				this.ticksToDetonation = this.def.projectile.explosionDelay;
				GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, this.def.projectile.damageDef, this.launcher.Faction);
			}
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00003468 File Offset: 0x00001668
		protected virtual void Explode()
		{
			Map map = base.Map;
			this.Destroy(DestroyMode.Vanish);
			bool flag = this.def.projectile.explosionEffect != null;
			if (flag)
			{
				Effecter effecter = this.def.projectile.explosionEffect.Spawn();
				effecter.Trigger(new TargetInfo(base.Position, map, false), new TargetInfo(base.Position, map, false));
				effecter.Cleanup();
			}
			IntVec3 position = base.Position;
			Map map2 = map;
			float explosionRadius = this.def.projectile.explosionRadius;
			DamageDef damageDef = this.def.projectile.damageDef;
			Thing launcher = this.launcher;
			int damageAmountBase = this.def.projectile.GetDamageAmount(this.launcher);
			SoundDef soundExplode = this.def.projectile.soundExplode;
			ThingDef equipmentDef = this.equipmentDef;
			ThingDef def = this.def;
			ThingDef postExplosionSpawnThingDef = this.def.projectile.postExplosionSpawnThingDef;
			float postExplosionSpawnChance = this.def.projectile.postExplosionSpawnChance;
			int postExplosionSpawnThingCount = this.def.projectile.postExplosionSpawnThingCount;
			ThingDef preExplosionSpawnThingDef = this.def.projectile.preExplosionSpawnThingDef;
			GenExplosion.DoExplosion(position, map2, explosionRadius, damageDef, launcher, damageAmountBase, -1, soundExplode, equipmentDef, def, null, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, this.def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, this.def.projectile.preExplosionSpawnChance, this.def.projectile.preExplosionSpawnThingCount, this.def.projectile.explosionChanceToStartFire, this.def.projectile.explosionDamageFalloff);
		}

		// Token: 0x04000010 RID: 16
		protected CompProjectileExtraDamage extraDamageComp;

		// Token: 0x04000011 RID: 17
		protected CompProjectileSmoke smokepopComp;

		// Token: 0x04000012 RID: 18
		private int ticksToDetonation;
	}
}
