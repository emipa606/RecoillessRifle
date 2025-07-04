using RecoillessRifle;
using RimWorld;

namespace Verse;

public class RR_Verb_LaunchMultipleProjectile : Verb
{
    protected virtual ThingDef Projectile
    {
        get
        {
            if (EquipmentSource == null)
            {
                return verbProps.defaultProjectile;
            }

            var comp = EquipmentSource.GetComp<CompChangeableProjectile>();
            return comp is { Loaded: true } ? comp.Projectile : verbProps.defaultProjectile;
        }
    }

    private static int pelletsPerShot(ThingDef projectile)
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

    private static float forcedScatterRadius(ThingDef projectile)
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

    private static float scatterRadiusAt10TilesAway(ThingDef projectile)
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

    public override void WarmupComplete()
    {
        base.WarmupComplete();
        Find.BattleLog.Add(new BattleLogEntry_RangedFire(caster,
            !currentTarget.HasThing ? null : currentTarget.Thing,
            EquipmentSource?.def, Projectile, ShotsPerBurst > 1));
    }

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
                    _ = (Projectile)GenSpawn.Spawn(projectile, shootLine.Source, caster.Map);
                    var num = pelletsPerShot(projectile);
                    if (num < 1)
                    {
                        num = 1;
                    }

                    var array = new Projectile[num];
                    var array2 = new ShootLine[num];
                    for (var i = 0; i < num; i++)
                    {
                        TryFindShootLineFromTo(caster.Position, currentTarget, out array2[i]);
                        array[i] = (Projectile)GenSpawn.Spawn(projectile, array2[i].Source, caster.Map);
                    }

                    var num2 = (currentTarget.Cell - caster.Position).LengthHorizontal;
                    var num3 = scatterRadiusAt10TilesAway(projectile) * num2 / 10f;
                    var num4 = verbProps.ForcedMissRadius + forcedScatterRadius(projectile) + num3;
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

                            array2[j].ChangeDestToMissWild(shotReport.AimOnTargetChance, false, caster.Map);
                            var hasThing2 = currentTarget.HasThing;
                            if (hasThing2)
                            {
                                array[j].HitFlags = ProjectileHitFlags.All;
                            }

                            if (!array[j].def.projectile.flyOverhead)
                            {
                                array[j].HitFlags = ProjectileHitFlags.IntendedTarget;
                            }

                            array[j].Launch(thing, drawPos, new LocalTargetInfo(array2[j].Dest), currentTarget,
                                equipment: thing2, hitFlags: array[j].HitFlags);
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