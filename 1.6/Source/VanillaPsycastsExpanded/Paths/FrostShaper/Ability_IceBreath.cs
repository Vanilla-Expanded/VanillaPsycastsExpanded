namespace VanillaPsycastsExpanded
{
    using RimWorld.Planet;
    using Verse;
    using VEF.Abilities;

    public class Ability_IceBreath : Ability_ShootProjectile
    {
        protected override Projectile ShootProjectile(GlobalTargetInfo target)
        {
			var projectile = base.ShootProjectile(target) as IceBreatheProjectile;
			projectile.ability = this;
			return projectile;
        }
    }
}