using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VEF.Abilities.Ability;

namespace VanillaPsycastsExpanded;

public class Ability_PsychicShock : Ability
{
    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        if (target.Thing is not Pawn victim || victim.GetStatValue(StatDefOf.PsychicSensitivity) <= 0) return false;
        return base.ValidateTarget(target, showMessages);
    }

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (var target in targets)
            if (Rand.Chance(0.3f))
            {
                var pawn = target.Thing as Pawn;
                target.Thing.TryAttachFire(0.5f, Caster);
                var brain = pawn?.health.hediffSet.GetBrain();
                if (brain != null)
                {
                    var num = Rand.RangeInclusive(1, 5);
                    pawn.TakeDamage(new(DamageDefOf.Flame, num, 0f, -1f, this.pawn, brain));
                }
            }
    }
}
