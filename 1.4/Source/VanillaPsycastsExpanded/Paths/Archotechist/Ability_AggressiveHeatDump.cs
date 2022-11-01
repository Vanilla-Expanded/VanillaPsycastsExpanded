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
        public override float GetRadiusForPawn() => 
                Mathf.Min(this.pawn.psychicEntropy.EntropyValue / 20f, 9f * base.GetRadiusForPawn(),
                    GenRadial.MaxRadialPatternRadius);

        public override float GetPowerForPawn() => this.pawn.psychicEntropy.EntropyValue * base.GetPowerForPawn();

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var explosionRadius = this.GetRadiusForPawn();
            var explosionPower = this.GetPowerForPawn();
            this.pawn.psychicEntropy.RemoveAllEntropy();
            MakeStaticFleck(targets[0].Cell, targets[0].Thing.Map, FleckDefOf.PsycastAreaEffect, explosionRadius, 0);
            MakeStaticFleck(targets[0].Cell, targets[0].Thing.Map, VPE_DefOf.VPE_AggresiveHeatDump, explosionRadius, 0);
            GenExplosion.DoExplosion(targets[0].Cell, pawn.Map, explosionRadius, DamageDefOf.Flame, pawn,
                (int)explosionPower, ignoredThings: new List<Thing> { pawn });
        }
    }
}