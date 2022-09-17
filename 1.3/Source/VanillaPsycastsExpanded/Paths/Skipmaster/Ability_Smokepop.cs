namespace VanillaPsycastsExpanded.Skipmaster;

using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

public class Ability_Smokepop : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (GlobalTargetInfo target in targets)
            GenExplosion.DoExplosion(target.Cell, target.Map, this.GetRadiusForPawn(), DamageDefOf.Smoke, null, -1, -1f, null, null, null, null,
                                     ThingDefOf.Gas_Smoke, 1f);
    }
}