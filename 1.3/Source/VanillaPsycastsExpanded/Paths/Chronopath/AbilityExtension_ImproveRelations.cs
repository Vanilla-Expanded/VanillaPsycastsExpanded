namespace VanillaPsycastsExpanded.Chronopath;

using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

public class AbilityExtension_ImproveRelations : AbilityExtension_AbilityMod
{
    public override void Cast(GlobalTargetInfo[] targets, Ability ability)
    {
        base.Cast(targets, ability);
        foreach (GlobalTargetInfo target in targets)
        {
            if (target.Thing is not Pawn { Faction.IsPlayer: false } pawn) continue;
            if (pawn.Faction.RelationKindWith(ability.pawn.Faction) == FactionRelationKind.Hostile) continue;
            if (pawn.guest.HostFaction                              != null) continue;
            pawn.Faction.TryAffectGoodwillWith(ability.pawn.Faction, 20, reason: VPE_DefOf.VPE_Foretelling);
        }
    }
}