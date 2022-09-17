namespace VanillaPsycastsExpanded.Chronopath
{
    using RimWorld.Planet;
    using Verse;
    using VFECore.Abilities;

    public class AbilityExtension_Foretelling : AbilityExtension_GiveInspiration
    {
        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            if (Rand.Chance(0.5f))
                base.Cast(targets, ability);
            else
                foreach (GlobalTargetInfo target in targets)
                    if (target.Thing is Pawn pawn)
                        pawn.needs.mood.thoughts.memories.TryGainMemoryFast(VPE_DefOf.VPE_Future);
        }
    }
}