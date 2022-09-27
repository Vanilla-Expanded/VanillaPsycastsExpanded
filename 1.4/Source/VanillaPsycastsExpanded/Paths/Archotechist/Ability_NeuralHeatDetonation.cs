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
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var explosionRadius = (this.pawn.psychicEntropy.EntropyValue / 10f) * this.pawn.GetStatValue(StatDefOf.PsychicSensitivity);
            this.pawn.psychicEntropy.RemoveAllEntropy();
            MakeStaticFleck(targets[0].Cell, targets[0].Thing.Map, FleckDefOf.PsycastAreaEffect, explosionRadius, 0);
            GenExplosion.DoExplosion(targets[0].Cell, pawn.Map, explosionRadius, DamageDefOf.Flame, pawn, ignoredThings: new List<Thing> { pawn });
        }
    }
}