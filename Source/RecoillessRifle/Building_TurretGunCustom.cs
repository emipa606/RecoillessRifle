using RimWorld;
using Verse;

namespace RecoillessRifle
{
    // Token: 0x02000004 RID: 4
    [StaticConstructorOnStartup]
    public class Building_TurretGunCustom : Building_TurretGun
    {
        // Token: 0x04000006 RID: 6
        private const int TryStartShootSomethingIntervalTicks = 10;

        // Token: 0x04000001 RID: 1
        protected bool hasGainedLoadcount = false;

        // Token: 0x04000005 RID: 5
        private bool holdFire;

        // Token: 0x04000002 RID: 2
        protected int lastLoadcount = 0;

        // Token: 0x04000003 RID: 3
        protected new TurretTop_CustomSize top;

        // Token: 0x04000004 RID: 4
        protected CompTurretTopSize topSizeComp;

        // Token: 0x06000012 RID: 18 RVA: 0x00002B98 File Offset: 0x00000D98
        public Building_TurretGunCustom()
        {
            top = new TurretTop_CustomSize(this);
        }

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x0600000C RID: 12 RVA: 0x00002A60 File Offset: 0x00000C60
        public CompTurretTopSize TopSizeComp => topSizeComp;

        // Token: 0x17000004 RID: 4
        // (get) Token: 0x0600000D RID: 13 RVA: 0x00002A78 File Offset: 0x00000C78
        private bool WarmingUp => burstWarmupTicksLeft > 0;

        // Token: 0x17000005 RID: 5
        // (get) Token: 0x0600000E RID: 14 RVA: 0x00002A94 File Offset: 0x00000C94
        public bool CanSetForcedTarget => mannableComp != null && (Faction == Faction.OfPlayer || MannedByColonist) &&
                                          !MannedByNonColonist;

        // Token: 0x17000006 RID: 6
        // (get) Token: 0x0600000F RID: 15 RVA: 0x00002ADC File Offset: 0x00000CDC
        private bool CanToggleHoldFire => (Faction == Faction.OfPlayer || MannedByColonist) && !MannedByNonColonist;

        // Token: 0x17000007 RID: 7
        // (get) Token: 0x06000010 RID: 16 RVA: 0x00002B10 File Offset: 0x00000D10
        private bool MannedByColonist =>
            mannableComp?.ManningPawn != null && mannableComp.ManningPawn.Faction == Faction.OfPlayer;

        // Token: 0x17000008 RID: 8
        // (get) Token: 0x06000011 RID: 17 RVA: 0x00002B54 File Offset: 0x00000D54
        private bool MannedByNonColonist =>
            mannableComp?.ManningPawn != null && mannableComp.ManningPawn.Faction != Faction.OfPlayer;

        // Token: 0x06000013 RID: 19 RVA: 0x00002BBC File Offset: 0x00000DBC
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            topSizeComp = GetComp<CompTurretTopSize>();
        }

        // Token: 0x06000014 RID: 20 RVA: 0x00002BD4 File Offset: 0x00000DD4
        public override void Tick()
        {
            base.Tick();
            if (forcedTarget.IsValid && !CanSetForcedTarget)
            {
                ResetForcedTarget();
            }

            if (!CanToggleHoldFire)
            {
                holdFire = false;
            }

            var thingDestroyed = forcedTarget.ThingDestroyed;
            if (thingDestroyed)
            {
                ResetForcedTarget();
            }

            if ((powerComp == null || powerComp.PowerOn) && (mannableComp == null || mannableComp.MannedNow) &&
                Spawned)
            {
                GunCompEq.verbTracker.VerbsTick();
                if (stunner.Stunned || GunCompEq.PrimaryVerb.state == VerbState.Bursting)
                {
                    return;
                }

                var warmingUp = WarmingUp;
                if (warmingUp)
                {
                    burstWarmupTicksLeft--;
                    if (burstWarmupTicksLeft == 0)
                    {
                        BeginBurst();
                    }
                }
                else
                {
                    if (burstCooldownTicksLeft > 0)
                    {
                        burstCooldownTicksLeft--;
                    }

                    if (burstCooldownTicksLeft <= 0 && this.IsHashIntervalTick(10))
                    {
                        TryStartShootSomething(true);
                    }
                }

                top.TurretTopTick();
            }
            else
            {
                ResetCurrentTarget();
            }
        }

        // Token: 0x06000015 RID: 21 RVA: 0x00002D50 File Offset: 0x00000F50
        private bool IsValidTarget(Thing t)
        {
            if (!(t is Pawn pawn))
            {
                return true;
            }

            if (GunCompEq.PrimaryVerb.ProjectileFliesOverhead())
            {
                var roofDef = Map.roofGrid.RoofAt(t.Position);
                if (roofDef != null && roofDef.isThickRoof)
                {
                    return false;
                }
            }

            if (mannableComp == null)
            {
                return false;
            }

            if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }

            return true;
        }

        // Token: 0x06000016 RID: 22 RVA: 0x00002DFC File Offset: 0x00000FFC
        private void ResetForcedTarget()
        {
            forcedTarget = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
            if (burstCooldownTicksLeft <= 0)
            {
                TryStartShootSomething(false);
            }
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00002E38 File Offset: 0x00001038
        private void UpdateGunVerbs()
        {
            var allVerbs = gun.TryGetComp<CompEquippable>().AllVerbs;
            foreach (var verb in allVerbs)
            {
                verb.caster = this;
                verb.castCompleteCallback = BurstComplete;
            }
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00002E8F File Offset: 0x0000108F
        private void ResetCurrentTarget()
        {
            currentTargetInt = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
        }

        // Token: 0x06000019 RID: 25 RVA: 0x00002EA4 File Offset: 0x000010A4
        public override void Draw()
        {
            top.DrawTurret();
            Comps_PostDraw();
        }
    }
}