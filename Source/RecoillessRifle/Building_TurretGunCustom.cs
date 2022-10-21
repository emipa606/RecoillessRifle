using RimWorld;
using Verse;

namespace RecoillessRifle;

[StaticConstructorOnStartup]
public class Building_TurretGunCustom : Building_TurretGun
{
    private const int TryStartShootSomethingIntervalTicks = 10;

    protected bool hasGainedLoadcount = false;

    private bool holdFire;

    protected int lastLoadcount = 0;

    protected new TurretTop_CustomSize top;

    protected CompTurretTopSize topSizeComp;

    public Building_TurretGunCustom()
    {
        top = new TurretTop_CustomSize(this);
    }

    public CompTurretTopSize TopSizeComp => topSizeComp;

    private bool WarmingUp => burstWarmupTicksLeft > 0;

    public bool CanSetForcedTarget => mannableComp != null && (Faction == Faction.OfPlayer || MannedByColonist) &&
                                      !MannedByNonColonist;

    private bool CanToggleHoldFire => (Faction == Faction.OfPlayer || MannedByColonist) && !MannedByNonColonist;

    private bool MannedByColonist =>
        mannableComp?.ManningPawn != null && mannableComp.ManningPawn.Faction == Faction.OfPlayer;

    private bool MannedByNonColonist =>
        mannableComp?.ManningPawn != null && mannableComp.ManningPawn.Faction != Faction.OfPlayer;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        topSizeComp = GetComp<CompTurretTopSize>();
    }

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

    private bool IsValidTarget(Thing t)
    {
        if (t is not Pawn pawn)
        {
            return true;
        }

        if (GunCompEq.PrimaryVerb.ProjectileFliesOverhead())
        {
            var roofDef = Map.roofGrid.RoofAt(t.Position);
            if (roofDef is { isThickRoof: true })
            {
                return false;
            }
        }

        if (mannableComp == null)
        {
            return false;
        }

        return !pawn.RaceProps.Animal || pawn.Faction != Faction.OfPlayer;
    }

    private void ResetForcedTarget()
    {
        forcedTarget = LocalTargetInfo.Invalid;
        burstWarmupTicksLeft = 0;
        if (burstCooldownTicksLeft <= 0)
        {
            TryStartShootSomething(false);
        }
    }

    private void UpdateGunVerbs()
    {
        var allVerbs = gun.TryGetComp<CompEquippable>().AllVerbs;
        foreach (var verb in allVerbs)
        {
            verb.caster = this;
            verb.castCompleteCallback = BurstComplete;
        }
    }

    private void ResetCurrentTarget()
    {
        currentTargetInt = LocalTargetInfo.Invalid;
        burstWarmupTicksLeft = 0;
    }

    public override void Draw()
    {
        top.DrawTurret();
        Comps_PostDraw();
    }
}