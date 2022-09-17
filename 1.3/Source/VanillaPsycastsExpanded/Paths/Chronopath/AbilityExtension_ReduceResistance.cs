namespace VanillaPsycastsExpanded.Chronopath;

using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

public class AbilityExtension_ReduceResistance : AbilityExtension_AbilityMod
{
    public override void Cast(GlobalTargetInfo[] targets, Ability ability)
    {
        base.Cast(targets, ability);
        foreach (GlobalTargetInfo target in targets)
        {
            if (target.Thing is not Pawn { HostFaction: { } host, GuestStatus: GuestStatus.Prisoner } pawn || host != ability.pawn.Faction) continue;
            pawn.guest.resistance -= 20f;
        }
    }
}