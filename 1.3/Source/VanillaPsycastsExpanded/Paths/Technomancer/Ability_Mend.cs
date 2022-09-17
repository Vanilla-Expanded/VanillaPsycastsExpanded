namespace VanillaPsycastsExpanded.Technomancer
{
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_Mend : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            foreach (GlobalTargetInfo target in targets)
                target.Thing.HitPoints = Mathf.Clamp(target.Thing.HitPoints + (int) ((this.pawn.GetStatValue(StatDefOf.PsychicSensitivity) - 0.8f) * 100f),
                                                     target.Thing.HitPoints, target.Thing.MaxHitPoints);
        }

        public override bool CanHitTarget(LocalTargetInfo target) => base.CanHitTarget(target) && target.Thing is {def: {useHitPoints: true}};
    }
}