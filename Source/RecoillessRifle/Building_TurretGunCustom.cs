using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RecoillessRifle
{
	// Token: 0x02000004 RID: 4
	[StaticConstructorOnStartup]
	public class Building_TurretGunCustom : Building_TurretGun
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600000C RID: 12 RVA: 0x00002A60 File Offset: 0x00000C60
		public CompTurretTopSize TopSizeComp
		{
			get
			{
				return this.topSizeComp;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600000D RID: 13 RVA: 0x00002A78 File Offset: 0x00000C78
		private bool WarmingUp
		{
			get
			{
				return this.burstWarmupTicksLeft > 0;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600000E RID: 14 RVA: 0x00002A94 File Offset: 0x00000C94
		public bool CanSetForcedTarget
		{
			get
			{
				bool flag = this.mannableComp == null;
				return !flag && (base.Faction == Faction.OfPlayer || this.MannedByColonist) && !this.MannedByNonColonist;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600000F RID: 15 RVA: 0x00002ADC File Offset: 0x00000CDC
		private bool CanToggleHoldFire
		{
			get
			{
				return (base.Faction == Faction.OfPlayer || this.MannedByColonist) && !this.MannedByNonColonist;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000010 RID: 16 RVA: 0x00002B10 File Offset: 0x00000D10
		private bool MannedByColonist
		{
			get
			{
				return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction == Faction.OfPlayer;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000011 RID: 17 RVA: 0x00002B54 File Offset: 0x00000D54
		private bool MannedByNonColonist
		{
			get
			{
				return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction != Faction.OfPlayer;
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002B98 File Offset: 0x00000D98
		public Building_TurretGunCustom()
		{
			this.top = new TurretTop_CustomSize(this);
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002BBC File Offset: 0x00000DBC
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.topSizeComp = base.GetComp<CompTurretTopSize>();
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002BD4 File Offset: 0x00000DD4
		public override void Tick()
		{
			base.Tick();
			bool flag = this.forcedTarget.IsValid && !this.CanSetForcedTarget;
			if (flag)
			{
				this.ResetForcedTarget();
			}
			bool flag2 = !this.CanToggleHoldFire;
			if (flag2)
			{
				this.holdFire = false;
			}
			bool thingDestroyed = this.forcedTarget.ThingDestroyed;
			if (thingDestroyed)
			{
				this.ResetForcedTarget();
			}
			bool flag3 = (this.powerComp == null || this.powerComp.PowerOn) && (this.mannableComp == null || this.mannableComp.MannedNow) && base.Spawned;
			if (flag3)
			{
				base.GunCompEq.verbTracker.VerbsTick();
				bool flag4 = !this.stunner.Stunned && base.GunCompEq.PrimaryVerb.state != VerbState.Bursting;
				if (flag4)
				{
					bool warmingUp = this.WarmingUp;
					if (warmingUp)
					{
						this.burstWarmupTicksLeft--;
						bool flag5 = this.burstWarmupTicksLeft == 0;
						if (flag5)
						{
							base.BeginBurst();
						}
					}
					else
					{
						bool flag6 = this.burstCooldownTicksLeft > 0;
						if (flag6)
						{
							this.burstCooldownTicksLeft--;
						}
						bool flag7 = this.burstCooldownTicksLeft <= 0 && this.IsHashIntervalTick(10);
						if (flag7)
						{
							base.TryStartShootSomething(true);
						}
					}
					this.top.TurretTopTick();
				}
			}
			else
			{
				this.ResetCurrentTarget();
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002D50 File Offset: 0x00000F50
		private bool IsValidTarget(Thing t)
		{
			Pawn pawn = t as Pawn;
			bool flag = pawn != null;
			if (flag)
			{
				bool flag2 = base.GunCompEq.PrimaryVerb.ProjectileFliesOverhead();
				if (flag2)
				{
					RoofDef roofDef = base.Map.roofGrid.RoofAt(t.Position);
					bool flag3 = roofDef != null && roofDef.isThickRoof;
					if (flag3)
					{
						return false;
					}
				}
				bool flag4 = this.mannableComp == null;
				if (flag4)
				{
					return false;
				}
				bool flag5 = pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer;
				if (flag5)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002DFC File Offset: 0x00000FFC
		private void ResetForcedTarget()
		{
			this.forcedTarget = LocalTargetInfo.Invalid;
			this.burstWarmupTicksLeft = 0;
			bool flag = this.burstCooldownTicksLeft <= 0;
			if (flag)
			{
				base.TryStartShootSomething(false);
			}
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00002E38 File Offset: 0x00001038
		private void UpdateGunVerbs()
		{
			List<Verb> allVerbs = this.gun.TryGetComp<CompEquippable>().AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				Verb verb = allVerbs[i];
				verb.caster = this;
				verb.castCompleteCallback = new Action(base.BurstComplete);
			}
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002E8F File Offset: 0x0000108F
		private void ResetCurrentTarget()
		{
			this.currentTargetInt = LocalTargetInfo.Invalid;
			this.burstWarmupTicksLeft = 0;
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002EA4 File Offset: 0x000010A4
		public override void Draw()
		{
			this.top.DrawTurret();
			base.Comps_PostDraw();
		}

		// Token: 0x04000001 RID: 1
		protected bool hasGainedLoadcount = false;

		// Token: 0x04000002 RID: 2
		protected int lastLoadcount = 0;

		// Token: 0x04000003 RID: 3
		protected new TurretTop_CustomSize top;

		// Token: 0x04000004 RID: 4
		protected CompTurretTopSize topSizeComp;

		// Token: 0x04000005 RID: 5
		private bool holdFire;

		// Token: 0x04000006 RID: 6
		private const int TryStartShootSomethingIntervalTicks = 10;
	}
}
