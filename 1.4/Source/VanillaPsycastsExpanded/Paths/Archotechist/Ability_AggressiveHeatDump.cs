namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Verse;
    using Verse.Sound;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_AggressiveHeatDump : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var damageMultiplier = this.pawn.psychicEntropy.EntropyValue / 100f;
            var explosionRadius = Mathf.Min(Mathf.Ceil(this.pawn.psychicEntropy.EntropyValue / 20f), 9f);
            this.pawn.psychicEntropy.RemoveAllEntropy();
            MakeStaticFleck(targets[0].Cell, targets[0].Thing.Map, FleckDefOf.PsycastAreaEffect, explosionRadius, 0);
            MakeStaticFleck(targets[0].Cell, targets[0].Thing.Map, VPE_DefOf.VPE_AggresiveHeatDump, explosionRadius, 0);
            GenExplosion.DoExplosion(targets[0].Cell, pawn.Map, explosionRadius, DamageDefOf.Flame, pawn,
                (int)(DamageDefOf.Flame.defaultDamage * damageMultiplier), ignoredThings: new List<Thing> { pawn });
        }
    }
}