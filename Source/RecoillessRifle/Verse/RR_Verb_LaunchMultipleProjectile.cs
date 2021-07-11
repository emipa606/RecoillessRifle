using RecoillessRifle;
using RimWorld;

namespace Verse
{
    // Token: 0x02000002 RID: 2
    public class RR_Verb_LaunchMultipleProjectile : Verb
    {
        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public virtual ThingDef Projectile
        {
            get
            {
                if (EquipmentSource == null)
                {
                    return verbProps.defaultProjectile;
                }

                var comp = EquipmentSource.GetComp<CompChangeableProjectile>();
                if (comp != null && comp.Loaded)
                {
                    return comp.Projectile;
                }

                return verbProps.defaultProjectile;
            }
        }

        // Token: 0x06000002 RID: 2 RVA: 0x000020A4 File Offset: 0x000002A4
        public int PelletsPerShot(ThingDef projectile)
        {
            if (projectile.comps == null)
            {
                return 1;
            }

            var i = 0;
            var count = projectile.comps.Count;
            while (i < count)
            {
                if (projectile.comps[i] is CompProperties_ProjectileMultiple compProperties_ProjectileMultiple)
                {
                    return compProperties_ProjectileMultiple.pellets;
                }

                i++;
            }

            return 1;
        }

        // Token: 0x06000003 RID: 3 RVA: 0x00002110 File Offset: 0x00000310
        public float ForsedScatterRadius(ThingDef projectile)
        {
            if (projectile.comps == null)
            {
                return 0f;
            }

            var i = 0;
            var count = projectile.comps.Count;
            while (i < count)
            {
                if (projectile.comps[i] is CompProperties_ProjectileMultiple compProperties_ProjectileMultiple)
                {
                    return compProperties_ProjectileMultiple.forsedScatterRadius;
                }

                i++;
            }

            return 0f;
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002180 File Offset: 0x00000380
        public float ScatterRadiusAt10tilesAway(ThingDef projectile)
        {
            if (projectile.comps == null)
            {
                return 0f;
            }

            var i = 0;
            var count = projectile.comps.Count;
            while (i < count)
            {
                if (projectile.comps[i] is CompProperties_ProjectileMultiple compProperties_ProjectileMultiple)
                {
                    return compProperties_ProjectileMultiple.scatterRadiusAt10tilesAway;
                }

                i++;
            }

            return 0f;
        }

        // Token: 0x06000005 RID: 5 RVA: 0x000021F0 File Offset: 0x000003F0
        public override void WarmupComplete()
        {
            base.WarmupComplete();
            Find.BattleLog.Add(new BattleLogEntry_RangedFire(caster,
                !currentTarget.HasThing ? null : currentTarget.Thing,
                EquipmentSource?.def, Projectile, ShotsPerBurst > 1));
        }

        // Token: 0x06000006 RID: 6 RVA: 0x0000225C File Offset: 0x0000045C
        protected override bool TryCastShot()
        {
            bool result;
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                result = false;
            }
            else
            {
                var projectile = Projectile;
                if (projectile == null)
                {
                    result = false;
                }
                else
                {
                    var shootLine = new ShootLine();
                    if (verbProps.stopBurstWithoutLos &&
                        !TryFindShootLineFromTo(caster.Position, currentTarget, out shootLine))
                    {
                        result = false;
                    }
                    else
                    {
                        if (EquipmentSource != null)
                        {
                            var comp = EquipmentSource.GetComp<CompChangeableProjectile>();
                            comp?.Notify_ProjectileLaunched();
                        }

                        var thing = caster;
                        Thing thing2 = EquipmentSource;
                        if (caster.TryGetComp<CompMannable>() != null &&
                            caster.TryGetComp<CompMannable>().ManningPawn != null)
                        {
                            thing = caster.TryGetComp<CompMannable>().ManningPawn;
                            thing2 = caster;
                        }

                        var drawPos = caster.DrawPos;
                        var unused = (Projectile) GenSpawn.Spawn(projectile, shootLine.Source, caster.Map);
                        var num = PelletsPerShot(projectile);
                        if (num < 1)
                        {
                            num = 1;
                        }

                        var array = new Projectile[num];
                        var array2 = new ShootLine[num];
                        for (var i = 0; i < num; i++)
                        {
                            TryFindShootLineFromTo(caster.Position, currentTarget, out array2[i]);
                            array[i] = (Projectile) GenSpawn.Spawn(projectile, array2[i].Source, caster.Map);
                            //array[i].FreeIntercept = (this.canFreeInterceptNow && !array[i].def.projectile.flyOverhead);
                        }

                        var num2 = (currentTarget.Cell - caster.Position).LengthHorizontal;
                        var num3 = ScatterRadiusAt10tilesAway(projectile) * num2 / 10f;
                        var num4 = verbProps.ForcedMissRadius + ForsedScatterRadius(projectile) + num3;
                        var j = 0;
                        while (j < num)
                        {
                            if (!(num4 > 0.5f))
                            {
                                goto IL_41E;
                            }

                            float num5 = (currentTarget.Cell - caster.Position).LengthHorizontalSquared;
                            float num6;
                            switch (num5)
                            {
                                case < 9f:
                                    num6 = 0f;
                                    break;
                                case < 25f:
                                    num6 = num4 * 0.5f;
                                    break;
                                case < 49f:
                                    num6 = num4 * 0.8f;
                                    break;
                                default:
                                    num6 = num4 * 1f;
                                    break;
                            }

                            if (!(num6 > 0.5f))
                            {
                                goto IL_41E;
                            }

                            var max = GenRadial.NumCellsInRadius(num4);
                            var num7 = Rand.Range(0, max);
                            if (num7 > 0)
                            {
                                var drawShooting = DebugViewSettings.drawShooting;
                                if (drawShooting)
                                {
                                    MoteMaker.ThrowText(caster.DrawPos, caster.Map, "ToForRad");
                                }

                                var hasThing = currentTarget.HasThing;
                                if (hasThing)
                                {
                                    array[j].HitFlags = ProjectileHitFlags.All;
                                    //array[j].ThingToNeverIntercept = this.currentTarget.Thing;
                                }

                                if (!array[j].def.projectile.flyOverhead)
                                {
                                    array[j].HitFlags = ProjectileHitFlags.IntendedTarget;
                                }

                                var c = currentTarget.Cell + GenRadial.RadialPattern[num7];
                                array[j].Launch(thing, drawPos, new LocalTargetInfo(c), currentTarget,
                                    equipment: thing2, hitFlags: array[j].HitFlags);
                            }
                            else
                            {
                                array[j].Launch(thing, drawPos, new LocalTargetInfo(currentTarget.Cell), currentTarget,
                                    equipment: thing2, hitFlags: array[j].HitFlags);
                                //array[j].Launch(thing, drawPos, this.currentTarget.Cell, thing2, this.currentTarget.Thing);
                            }

                            IL_6D8:
                            j++;
                            continue;
                            IL_41E:
                            var shotReport = ShotReport.HitReportFor(caster, this, currentTarget);
                            if (Rand.Value > shotReport.AimOnTargetChance_IgnoringPosture)
                            {
                                var drawShooting2 = DebugViewSettings.drawShooting;
                                if (drawShooting2)
                                {
                                    MoteMaker.ThrowText(caster.DrawPos, caster.Map, "ToWild");
                                }

                                array2[j].ChangeDestToMissWild(shotReport.AimOnTargetChance);
                                var hasThing2 = currentTarget.HasThing;
                                if (hasThing2)
                                {
                                    array[j].HitFlags = ProjectileHitFlags.All;
                                    //array[j].ThingToNeverIntercept = this.currentTarget.Thing;
                                }

                                if (!array[j].def.projectile.flyOverhead)
                                {
                                    array[j].HitFlags = ProjectileHitFlags.IntendedTarget;
                                    //array[j].InterceptWalls = true;
                                }

                                array[j].Launch(thing, drawPos, new LocalTargetInfo(array2[j].Dest), currentTarget,
                                    equipment: thing2, hitFlags: array[j].HitFlags);
                                //array[j].Launch(thing, drawPos, array2[j].Dest, thing2, this.currentTarget.Thing);
                                goto IL_6D8;
                            }

                            if (Rand.Value > shotReport.PassCoverChance)
                            {
                                var drawShooting3 = DebugViewSettings.drawShooting;
                                if (drawShooting3)
                                {
                                    MoteMaker.ThrowText(caster.DrawPos, caster.Map, "ToCover");
                                }

                                if (currentTarget.Thing != null &&
                                    currentTarget.Thing.def.category == ThingCategory.Pawn)
                                {
                                    var randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
                                    if (!array[j].def.projectile.flyOverhead)
                                    {
                                        array[j].HitFlags = ProjectileHitFlags.IntendedTarget;
                                    }

                                    array[j].Launch(thing, drawPos, new LocalTargetInfo(randomCoverToMissInto),
                                        currentTarget, equipment: thing2, hitFlags: array[j].HitFlags);
                                    //array[j].Launch(thing, drawPos, randomCoverToMissInto, thing2, this.currentTarget.Thing);
                                    goto IL_6D8;
                                }
                            }

                            var drawShooting4 = DebugViewSettings.drawShooting;
                            if (drawShooting4)
                            {
                                MoteMaker.ThrowText(caster.DrawPos, caster.Map, "ToHit");
                            }

                            if (!array[j].def.projectile.flyOverhead && (!currentTarget.HasThing ||
                                                                         currentTarget.Thing.def.Fillage ==
                                                                         FillCategory.Full))
                            {
                                array[j].HitFlags = ProjectileHitFlags.IntendedTarget;
                            }

                            array[j].Launch(thing, drawPos,
                                currentTarget.Thing != null ? currentTarget : new LocalTargetInfo(array2[j].Dest),
                                currentTarget, equipment: thing2,
                                hitFlags: array[j].HitFlags);

                            goto IL_6D8;
                        }

                        result = true;
                    }
                }
            }

            return result;
        }
    }
}