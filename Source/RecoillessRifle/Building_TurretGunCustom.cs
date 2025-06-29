using RimWorld;
using UnityEngine;
using Verse;

namespace RecoillessRifle;

[StaticConstructorOnStartup]
public class Building_TurretGunCustom : Building_TurretGun
{
    private const int TryStartShootSomethingIntervalTicks = 10;

    private new readonly TurretTop_CustomSize top;

    protected bool hasGainedLoadcount = false;

    private bool holdFire;

    protected int lastLoadcount = 0;

    public Building_TurretGunCustom()
    {
        top = new TurretTop_CustomSize(this);
    }

    public CompTurretTopSize TopSizeComp { get; private set; }

    private bool WarmingUp => burstWarmupTicksLeft > 0;

    private bool CanSetForcedTarget => mannableComp != null && (Faction == Faction.OfPlayer || MannedByColonist) &&
                                       !MannedByNonColonist;

    private bool CanToggleHoldFire => (Faction == Faction.OfPlayer || MannedByColonist) && !MannedByNonColonist;

    private bool MannedByColonist =>
        mannableComp?.ManningPawn != null && mannableComp.ManningPawn.Faction == Faction.OfPlayer;

    private bool MannedByNonColonist =>
        mannableComp?.ManningPawn != null && mannableComp.ManningPawn.Faction != Faction.OfPlayer;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        TopSizeComp = GetComp<CompTurretTopSize>();
    }

    protected override void Tick()
    {
        base.Tick();
        if (forcedTarget.IsValid && !CanSetForcedTarget)
        {
            resetForcedTarget();
        }

        if (!CanToggleHoldFire)
        {
            holdFire = false;
        }

        var thingDestroyed = forcedTarget.ThingDestroyed;
        if (thingDestroyed)
        {
            resetForcedTarget();
        }

        if ((powerComp == null || powerComp.PowerOn) && (mannableComp == null || mannableComp.MannedNow) &&
            Spawned)
        {
            GunCompEq.verbTracker.VerbsTick();
            if (IsStunned || GunCompEq.PrimaryVerb.state == VerbState.Bursting)
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
            resetCurrentTarget();
        }
    }

    private void resetForcedTarget()
    {
        forcedTarget = LocalTargetInfo.Invalid;
        burstWarmupTicksLeft = 0;
        if (burstCooldownTicksLeft <= 0)
        {
            TryStartShootSomething(false);
        }
    }

    private void resetCurrentTarget()
    {
        currentTargetInt = LocalTargetInfo.Invalid;
        burstWarmupTicksLeft = 0;
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        top.DrawTurret();
        Comps_PostDraw();
    }
}