namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using Verse;
    using Verse.AI;
    using Verse.AI.Group;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;

    public class AbilityExtension_Flee : AbilityExtension_AbilityMod
    {
        public bool onlyHostile = true;

        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
            foreach (GlobalTargetInfo target in targets)
            {
                Pawn pawn = target.Thing as Pawn;
                if (!this.onlyHostile || !pawn.HostileTo(ability.pawn)) return;
                pawn.GetLord()?.RemovePawn(pawn);
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, ability.def.label, true, false, null, true, false, true);
            }
        }
    }
}