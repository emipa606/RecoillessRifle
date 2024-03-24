using RimWorld;
using UnityEngine;
using Verse;

namespace RecoillessRifle;

public class CompProperties_ProjectileExtraDamage : CompProperties
{
    public readonly int damageAmountBase = 1;

    public readonly string hitText = "RR_Hit";

    public DamageDef damageDef;

    public Color hitTextColor = new Color32(byte.MaxValue, 153, 102, byte.MaxValue);

    public CompProperties_ProjectileExtraDamage()
    {
        compClass = typeof(CompProjectileExtraDamage);
    }

    public override void ResolveReferences(ThingDef parentDef)
    {
        base.ResolveReferences(parentDef);
        if (damageDef == null)
        {
            damageDef = DamageDefOf.Bullet;
        }
    }
}