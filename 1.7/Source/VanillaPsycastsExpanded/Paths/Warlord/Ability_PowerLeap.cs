using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VEF.Abilities.Ability;

namespace VanillaPsycastsExpanded;

public class Ability_PowerLeap : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        var map = Caster.Map;
        var flyer = (JumpingPawn)PawnFlyer.MakeFlyer(VPE_DefOf.VPE_JumpingPawn, CasterPawn, targets[0].Cell, null, null);
        flyer.ability = this;
        GenSpawn.Spawn(flyer, Caster.Position, map);
        base.Cast(targets);
    }
}
