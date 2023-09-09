using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using VFEMech;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded.Staticlord;

public class ChainBolt : TeslaProjectile
{
    protected override int MaxBounceCount
    {
        get
        {
            var ability = SourceAbility;
            return ability != null ? Mathf.RoundToInt(ability.GetPowerForPawn()) : base.MaxBounceCount;
        }
    }

    private Ability SourceAbility
    {
        get
        {
            if (this.TryGetComp<CompAbilityProjectile>() is { ability: { } sourceAbility }) return sourceAbility;
            for (var i = allProjectiles.Count; i-- > 0;)
                if (allProjectiles[i].TryGetComp<CompAbilityProjectile>() is { ability: { } ability })
                    return ability;
            return null;
        }
    }
}

public class Ability_ChainBolt : Ability_ShootProjectile
{
    public override float GetPowerForPawn() => def.power + Mathf.FloorToInt((pawn.GetStatValue(StatDefOf.PsychicSensitivity) - 1) * 4);
}
