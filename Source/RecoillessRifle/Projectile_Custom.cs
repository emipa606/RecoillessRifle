using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RecoillessRifle;

public class Projectile_Custom : Projectile
{
    protected CompProjectileExtraDamage extraDamageComp;

    protected CompProjectileSmoke smokepopComp;

    private int ticksToDetonation;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksToDetonation, "ticksToDetonation");
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        extraDamageComp = GetComp<CompProjectileExtraDamage>();
        smokepopComp = GetComp<CompProjectileSmoke>();
    }

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

    protected override void Impact(Thing hitThing, bool blockedByShield = false)
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

    protected virtual void ImpactPopSmoke(Thing hitThing)
    {
        var position = Position;
        var map = Map;
        var num = 1f + smokepopComp.Props.smokepopRadius;
        var smoke = DamageDefOf.Smoke;
        GenExplosion.DoExplosion(position, map, num, smoke, null, -1, -1, null, null, null, null, null, 0f, 1,
            GasType.BlindSmoke);
    }

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
                    (float)(Mathf.Sqrt(def.projectile.GetDamageAmount(launcher)) * 1.0), 4f);
            }
        }
    }

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
            postExplosionSpawnThingCount, def.projectile.postExplosionGasType,
            def.projectile.applyDamageToExplosionCellsNeighbors,
            preExplosionSpawnThingDef, def.projectile.preExplosionSpawnChance,
            def.projectile.preExplosionSpawnThingCount, def.projectile.explosionChanceToStartFire,
            def.projectile.explosionDamageFalloff);
    }
}