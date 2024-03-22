using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded.Skipmaster;

public class Ability_Smokepop : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (var target in targets)
            GenExplosion.DoExplosion(target.Cell, target.Map, GetRadiusForPawn(), DamageDefOf.Smoke, pawn, postExplosionGasType: GasType.BlindSmoke);
    }
}