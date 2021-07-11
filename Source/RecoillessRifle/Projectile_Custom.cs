using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RecoillessRifle
{
    // Token: 0x0200000D RID: 13
    public class Projectile_Custom : Projectile
    {
        // Token: 0x04000010 RID: 16
        protected CompProjectileExtraDamage extraDamageComp;

        // Token: 0x04000011 RID: 17
        protected CompProjectileSmoke smokepopComp;

        // Token: 0x04000012 RID: 18
        private int ticksToDetonation;

        // Token: 0x06000026 RID: 38 RVA: 0x00003025 File Offset: 0x00001225
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToDetonation, "ticksToDetonation");
        }

        // Token: 0x06000027 RID: 39 RVA: 0x00003042 File Offset: 0x00001242
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            extraDamageComp = GetComp<CompProjectileExtraDamage>();
            smokepopComp = GetComp<CompProjectileSmoke>();
        }

        // Token: 0x06000028 RID: 40 RVA: 0x00003068 File Offset: 0x00001268
        public override void Tick()
        {
            base.Tick();
            if (ticksToDetonation <= 0)
            {
                return;
            }

            ticksToDetonation--;
            if (ticksToDetonation <= 0)
            {
                Explode();
            }
        }

        // Token: 0x06000029 RID: 41 RVA: 0x000030B4 File Offset: 0x000012B4
        protected override void Impact(Thing hitThing)
        {
            var map = Map;
            if (map == null)
            {
                Log.Error("map is null!");
            }

            if (smokepopComp != null)
            {
                ImpactPopSmoke(hitThing);
            }

            if (def.projectile.explosionRadius > 0f)
            {
                if (extraDamageComp != null)
                {
                    ImpactExtra(hitThing, map);
                }

                ImpactExplode(hitThing);
            }
            else
            {
                ImpactDirectly(hitThing, map);
                if (extraDamageComp != null)
                {
                    ImpactExtra(hitThing, map);
                }
            }
        }

        // Token: 0x0600002A RID: 42 RVA: 0x00003158 File Offset: 0x00001358
        protected virtual void ImpactPopSmoke(Thing hitThing)
        {
            var position = Position;
            var map = Map;
            var num = 1f + smokepopComp.Props.smokepopRadius;
            var smoke = DamageDefOf.Smoke;
            var gas_Smoke = ThingDefOf.Gas_Smoke;
            GenExplosion.DoExplosion(position, map, num, smoke, null, -1, -1, null, null, null, null, gas_Smoke, 1f);
        }

        // Token: 0x0600002B RID: 43 RVA: 0x000031C4 File Offset: 0x000013C4
        protected virtual void ImpactExtra(Thing hitThing, Map map)
        {
            if (hitThing == null)
            {
                return;
            }

            var damageAmountBase = extraDamageComp.Props.damageAmountBase;
            var damageDef = extraDamageComp.Props.damageDef;
            var y = ExactRotation.eulerAngles.y;
            var instigator = launcher;
            var thingDef = equipmentDef;
            var damageInfo = new DamageInfo(damageDef, damageAmountBase, 0, y, instigator, null, thingDef);
            hitThing.TakeDamage(damageInfo);
            if (hitThing.def.category == ThingCategory.Pawn)
            {
                MoteMaker.ThrowText(new Vector3(Position.x + 1f, Position.y, Position.z + 1f), map,
                    extraDamageComp.Props.hitText.Translate(), extraDamageComp.Props.hitTextColor);
            }
        }

        // Token: 0x0600002C RID: 44 RVA: 0x000032BC File Offset: 0x000014BC
        protected virtual void ImpactDirectly(Thing hitThing, Map map)
        {
            base.Impact(hitThing);
            var battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, hitThing,
                intendedTarget.Thing, equipmentDef, def, targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            if (hitThing != null)
            {
                var damageAmountBase = def.projectile.GetDamageAmount(launcher);
                var damageDef = def.projectile.damageDef;
                var y = ExactRotation.eulerAngles.y;
                var instigator = launcher;
                var thingDef = equipmentDef;
                var damageInfo = new DamageInfo(damageDef, damageAmountBase, 0, y, instigator, null, thingDef);
                hitThing.TakeDamage(damageInfo).AssociateWithLog(battleLogEntry_RangedImpact);
            }
            else
            {
                SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(Position, map));
                FleckMaker.Static(ExactPosition, map, FleckDefOf.ShotHit_Dirt);
                var takeSplashes = Position.GetTerrain(map).takeSplashes;
                if (takeSplashes)
                {
                    FleckMaker.WaterSplash(ExactPosition, map,
                        (float) (Mathf.Sqrt(def.projectile.GetDamageAmount(launcher)) * 1.0), 4f);
                }
            }
        }

        // Token: 0x0600002D RID: 45 RVA: 0x000033F8 File Offset: 0x000015F8
        protected virtual void ImpactExplode(Thing hitThing)
        {
            if (def.projectile.explosionDelay == 0)
            {
                Explode();
            }
            else
            {
                landed = true;
                ticksToDetonation = def.projectile.explosionDelay;
                GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, def.projectile.damageDef, launcher.Faction);
            }
        }

        // Token: 0x0600002E RID: 46 RVA: 0x00003468 File Offset: 0x00001668
        protected virtual void Explode()
        {
            var map = Map;
            Destroy();
            if (def.projectile.explosionEffect != null)
            {
                var effecter = def.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(Position, map), new TargetInfo(Position, map));
                effecter.Cleanup();
            }

            var position = Position;
            var map2 = map;
            var explosionRadius = def.projectile.explosionRadius;
            var damageDef = def.projectile.damageDef;
            var instigator = launcher;
            var damageAmountBase = def.projectile.GetDamageAmount(launcher);
            var soundExplode = def.projectile.soundExplode;
            var thingDef = equipmentDef;
            var projectile = def;
            var postExplosionSpawnThingDef = def.projectile.postExplosionSpawnThingDef;
            var postExplosionSpawnChance = def.projectile.postExplosionSpawnChance;
            var postExplosionSpawnThingCount = def.projectile.postExplosionSpawnThingCount;
            var preExplosionSpawnThingDef = def.projectile.preExplosionSpawnThingDef;
            GenExplosion.DoExplosion(position, map2, explosionRadius, damageDef, instigator, damageAmountBase, -1,
                soundExplode, thingDef, projectile, null, postExplosionSpawnThingDef, postExplosionSpawnChance,
                postExplosionSpawnThingCount, def.projectile.applyDamageToExplosionCellsNeighbors,
                preExplosionSpawnThingDef, def.projectile.preExplosionSpawnChance,
                def.projectile.preExplosionSpawnThingCount, def.projectile.explosionChanceToStartFire,
                def.projectile.explosionDamageFalloff);
        }
    }
}