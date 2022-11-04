using System;

namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using System.Collections.Generic;
    using UnityEngine;
    using Verse;
    using Verse.Sound;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_NeuralHeatDetonation : Ability
    {
        public override float GetRadiusForPawn() =>
            Mathf.Min(this.pawn.psychicEntropy.EntropyValue / 10f * base.GetRadiusForPawn(),
                GenRadial.MaxRadialPatternRadius);

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var explosionRadius = this.GetRadiusForPawn();
            this.pawn.psychicEntropy.RemoveAllEntropy();
            MakeStaticFleck(targets[0].Cell, targets[0].Thing.Map, FleckDefOf.PsycastAreaEffect, explosionRadius, 0);
            GenExplosion.DoExplosion(targets[0].Cell, pawn.Map, explosionRadius, DamageDefOf.Flame, pawn, ignoredThings: new List<Thing> { pawn });
        }
    }
}