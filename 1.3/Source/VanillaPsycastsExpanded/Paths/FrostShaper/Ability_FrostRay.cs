namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using Verse;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;
    public class Ability_FrostRay : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            Projectile projectile = GenSpawn.Spawn(this.def.GetModExtension<AbilityExtension_Projectile>().projectile, this.pawn.Position, this.pawn.Map) as Projectile;
            if (projectile is AbilityProjectile abilityProjectile)
            {
                abilityProjectile.ability = this;
            }
            projectile?.Launch(this.pawn, this.pawn.DrawPos, ((LocalTargetInfo)targets[0]), ((LocalTargetInfo)targets[0]), ProjectileHitFlags.IntendedTarget);
            this.pawn.stances.SetStance(new Stance_Stand(this.GetDurationForPawn(), ((LocalTargetInfo)targets[0]), verb));
        }

        public override void ApplyHediffs(params GlobalTargetInfo[] targetInfo)
        {
            this.ApplyHediff(this.pawn);
        }
    }
}