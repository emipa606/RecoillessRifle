using System;
using RecoillessRifle;
using RimWorld;
using UnityEngine;

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
				bool flag = this.EquipmentSource != null;
				if (flag)
				{
					CompChangeableProjectile comp = this.EquipmentSource.GetComp<CompChangeableProjectile>();
					bool flag2 = comp != null && comp.Loaded;
					if (flag2)
					{
						return comp.Projectile;
					}
				}
				return this.verbProps.defaultProjectile;
			}
		}

		// Token: 0x06000002 RID: 2 RVA: 0x000020A4 File Offset: 0x000002A4
		public int PelletsPerShot(ThingDef projectile)
		{
			bool flag = projectile.comps != null;
			if (flag)
			{
				int i = 0;
				int count = projectile.comps.Count;
				while (i < count)
				{
					CompProperties_ProjectileMultiple compProperties_ProjectileMultiple = projectile.comps[i] as CompProperties_ProjectileMultiple;
					bool flag2 = compProperties_ProjectileMultiple != null;
					if (flag2)
					{
						return compProperties_ProjectileMultiple.pellets;
					}
					i++;
				}
			}
			return 1;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002110 File Offset: 0x00000310
		public float ForsedScatterRadius(ThingDef projectile)
		{
			bool flag = projectile.comps != null;
			if (flag)
			{
				int i = 0;
				int count = projectile.comps.Count;
				while (i < count)
				{
					CompProperties_ProjectileMultiple compProperties_ProjectileMultiple = projectile.comps[i] as CompProperties_ProjectileMultiple;
					bool flag2 = compProperties_ProjectileMultiple != null;
					if (flag2)
					{
						return compProperties_ProjectileMultiple.forsedScatterRadius;
					}
					i++;
				}
			}
			return 0f;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002180 File Offset: 0x00000380
		public float ScatterRadiusAt10tilesAway(ThingDef projectile)
		{
			bool flag = projectile.comps != null;
			if (flag)
			{
				int i = 0;
				int count = projectile.comps.Count;
				while (i < count)
				{
					CompProperties_ProjectileMultiple compProperties_ProjectileMultiple = projectile.comps[i] as CompProperties_ProjectileMultiple;
					bool flag2 = compProperties_ProjectileMultiple != null;
					if (flag2)
					{
						return compProperties_ProjectileMultiple.scatterRadiusAt10tilesAway;
					}
					i++;
				}
			}
			return 0f;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000021F0 File Offset: 0x000003F0
		public override void WarmupComplete()
		{
			base.WarmupComplete();
			Find.BattleLog.Add(new BattleLogEntry_RangedFire(this.caster, (!this.currentTarget.HasThing) ? null : this.currentTarget.Thing, (this.EquipmentSource == null) ? null : this.EquipmentSource.def, this.Projectile, this.ShotsPerBurst > 1));
		}

		// Token: 0x06000006 RID: 6 RVA: 0x0000225C File Offset: 0x0000045C
		protected override bool TryCastShot()
		{
			bool flag = this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				ThingDef projectile = this.Projectile;
				bool flag2 = projectile == null;
				if (flag2)
				{
					result = false;
				}
				else
				{
					ShootLine shootLine;
					bool flag3 = base.TryFindShootLineFromTo(this.caster.Position, this.currentTarget, out shootLine);
					bool flag4 = this.verbProps.stopBurstWithoutLos && !flag3;
					if (flag4)
					{
						result = false;
					}
					else
					{
						bool flag5 = this.EquipmentSource != null;
						if (flag5)
						{
							CompChangeableProjectile comp = this.EquipmentSource.GetComp<CompChangeableProjectile>();
							bool flag6 = comp != null;
							if (flag6)
							{
								comp.Notify_ProjectileLaunched();
							}
						}
						Thing thing = this.caster;
						Thing thing2 = this.EquipmentSource;
						CompMannable compMannable = this.caster.TryGetComp<CompMannable>();
						bool flag7 = compMannable != null && compMannable.ManningPawn != null;
						if (flag7)
						{
							thing = compMannable.ManningPawn;
							thing2 = this.caster;
						}
						Vector3 drawPos = this.caster.DrawPos;
						Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, shootLine.Source, this.caster.Map);
						int num = this.PelletsPerShot(projectile);
						bool flag8 = num < 1;
						if (flag8)
						{
							num = 1;
						}
						Projectile[] array = new Projectile[num];
						ShootLine[] array2 = new ShootLine[num];
						for (int i = 0; i < num; i++)
						{
							base.TryFindShootLineFromTo(this.caster.Position, this.currentTarget, out array2[i]);
							array[i] = (Projectile)GenSpawn.Spawn(projectile, array2[i].Source, this.caster.Map);
							//array[i].FreeIntercept = (this.canFreeInterceptNow && !array[i].def.projectile.flyOverhead);
						}
						float num2 = (this.currentTarget.Cell - this.caster.Position).LengthHorizontal;
						float num3 = this.ScatterRadiusAt10tilesAway(projectile) * num2 / 10f;
						float num4 = this.verbProps.forcedMissRadius + this.ForsedScatterRadius(projectile) + num3;
						int j = 0;
						while (j < num)
						{
							bool flag9 = num4 > 0.5f;
							if (!flag9)
							{
								goto IL_41E;
							}
							float num5 = (float)(this.currentTarget.Cell - this.caster.Position).LengthHorizontalSquared;
							bool flag10 = num5 < 9f;
							float num6;
							if (flag10)
							{
								num6 = 0f;
							}
							else
							{
								bool flag11 = num5 < 25f;
								if (flag11)
								{
									num6 = num4 * 0.5f;
								}
								else
								{
									bool flag12 = num5 < 49f;
									if (flag12)
									{
										num6 = num4 * 0.8f;
									}
									else
									{
										num6 = num4 * 1f;
									}
								}
							}
							bool flag13 = num6 > 0.5f;
							if (!flag13)
							{
								goto IL_41E;
							}
							int max = GenRadial.NumCellsInRadius(num4);
							int num7 = Rand.Range(0, max);
							bool flag14 = num7 > 0;
							if (flag14)
							{
								bool drawShooting = DebugViewSettings.drawShooting;
								if (drawShooting)
								{
									MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToForRad", -1f);
								}
								bool hasThing = this.currentTarget.HasThing;
								if (hasThing)
								{
									array[j].HitFlags = ProjectileHitFlags.All;
									//array[j].ThingToNeverIntercept = this.currentTarget.Thing;
								}
								bool flag15 = !array[j].def.projectile.flyOverhead;
								if (flag15)
								{
									array[j].HitFlags = ProjectileHitFlags.IntendedTarget;
                                }
								IntVec3 c = this.currentTarget.Cell + GenRadial.RadialPattern[num7];
								array[j].Launch(thing, origin: drawPos, usedTarget: new LocalTargetInfo(c), intendedTarget: currentTarget, equipment: thing2, hitFlags: array[j].HitFlags);
							}
							else
                            {
                                array[j].Launch(thing, origin: drawPos, usedTarget: new LocalTargetInfo(this.currentTarget.Cell), intendedTarget: currentTarget, equipment: thing2, hitFlags: array[j].HitFlags);
                                //array[j].Launch(thing, drawPos, this.currentTarget.Cell, thing2, this.currentTarget.Thing);
							}
							IL_6D8:
							j++;
							continue;
							IL_41E:
							ShotReport shotReport = ShotReport.HitReportFor(this.caster, this, this.currentTarget);
							bool flag16 = Rand.Value > shotReport.AimOnTargetChance_IgnoringPosture;
							if (flag16)
							{
								bool drawShooting2 = DebugViewSettings.drawShooting;
								if (drawShooting2)
								{
									MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToWild", -1f);
								}
								array2[j].ChangeDestToMissWild(shotReport.AimOnTargetChance);
								bool hasThing2 = this.currentTarget.HasThing;
								if (hasThing2)
								{
									array[j].HitFlags = ProjectileHitFlags.All;
									//array[j].ThingToNeverIntercept = this.currentTarget.Thing;
								}
								bool flag17 = !array[j].def.projectile.flyOverhead;
								if (flag17)
								{
									array[j].HitFlags = ProjectileHitFlags.IntendedTarget;
                                    //array[j].InterceptWalls = true;
								}
                                array[j].Launch(thing, origin: drawPos, usedTarget: new LocalTargetInfo(array2[j].Dest), intendedTarget: currentTarget, equipment: thing2, hitFlags: array[j].HitFlags);
                                //array[j].Launch(thing, drawPos, array2[j].Dest, thing2, this.currentTarget.Thing);
								goto IL_6D8;
							}
							bool flag18 = Rand.Value > shotReport.PassCoverChance;
							if (flag18)
							{
								bool drawShooting3 = DebugViewSettings.drawShooting;
								if (drawShooting3)
								{
									MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToCover", -1f);
								}
								bool flag19 = this.currentTarget.Thing != null && this.currentTarget.Thing.def.category == ThingCategory.Pawn;
								if (flag19)
								{
									Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
									bool flag20 = !array[j].def.projectile.flyOverhead;
									if (flag20)
									{
										array[j].HitFlags = ProjectileHitFlags.IntendedTarget;
                                    }
                                    array[j].Launch(thing, origin: drawPos, usedTarget: new LocalTargetInfo(randomCoverToMissInto), intendedTarget: currentTarget, equipment: thing2, hitFlags: array[j].HitFlags);
                                    //array[j].Launch(thing, drawPos, randomCoverToMissInto, thing2, this.currentTarget.Thing);
									goto IL_6D8;
								}
							}
							bool drawShooting4 = DebugViewSettings.drawShooting;
							if (drawShooting4)
							{
								MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToHit", -1f);
							}
							bool flag21 = !array[j].def.projectile.flyOverhead && (!this.currentTarget.HasThing || this.currentTarget.Thing.def.Fillage == FillCategory.Full);
							if (flag21)
							{
								array[j].HitFlags = ProjectileHitFlags.IntendedTarget;
                            }
							bool flag22 = this.currentTarget.Thing != null;
							if (flag22)
							{
                                array[j].Launch(thing, origin: drawPos, usedTarget: currentTarget, intendedTarget: currentTarget, equipment: thing2, hitFlags: array[j].HitFlags);
                                //array[j].Launch(thing, drawPos, this.currentTarget, thing2, this.currentTarget.Thing);
							}
							else
							{
                                array[j].Launch(thing, origin: drawPos, usedTarget: new LocalTargetInfo(array2[j].Dest), intendedTarget: currentTarget, equipment: thing2, hitFlags: array[j].HitFlags);
                                //array[j].Launch(thing, drawPos, array2[j].Dest, thing2, this.currentTarget.Thing);
							}
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
