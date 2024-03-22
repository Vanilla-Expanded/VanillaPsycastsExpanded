using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded;

public class AbilityExtension_GiveInspiration : AbilityExtension_AbilityMod
{
    public bool onlyPlayer;

    public override void Cast(GlobalTargetInfo[] targets, Ability ability)
    {
        base.Cast(targets, ability);
        foreach (var target in targets)
        {
            var pawn = target.Thing as Pawn;
            if (pawn != null && (!onlyPlayer || pawn.Faction is { IsPlayer: true }))
            {
                var randomAvailableInspirationDef = pawn.mindState.inspirationHandler.GetRandomAvailableInspirationDef();
                if (randomAvailableInspirationDef != null)
                    pawn.mindState.inspirationHandler.TryStartInspiration(randomAvailableInspirationDef,
                        "LetterPsychicInspiration".Translate(
                            pawn.Named("PAWN"), ability.pawn.Named("CASTER")));
            }
        }
    }

    public override bool CanApplyOn(LocalTargetInfo target, Ability ability, bool throwMessages = false) =>
        Valid(new[] { target.ToGlobalTargetInfo(target.Thing.Map) }, ability);

    public override bool Valid(GlobalTargetInfo[] targets, Ability ability, bool throwMessages = false)
    {
        foreach (var target in targets)
        {
            var pawn = target.Thing as Pawn;
            if (pawn != null && (!onlyPlayer || pawn.Faction is { IsPlayer: true }))
            {
                if (!AbilityUtility.ValidateNoInspiration(pawn, throwMessages, null)) return false;
                if (!AbilityUtility.ValidateCanGetInspiration(pawn, throwMessages, null)) return false;
            }
        }

        return base.Valid(targets, ability, throwMessages);
    }
}