namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using Verse;
    using Ability = VFECore.Abilities.Ability;

    public abstract class Ability_TargetCorpse : Ability
    {
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            var corpse = target.Thing as Corpse;
            if (corpse is null)
            {
                if (showMessages)
                {
                    Messages.Message("VPE.MustBeCorpse".Translate(), corpse, MessageTypeDefOf.CautionInput);
                }
                return false;
            }
            else if (!corpse.InnerPawn.RaceProps.Humanlike)
            {
                if (showMessages)
                {
                    Messages.Message("VPE.MustBeCorpseHumanlike".Translate(), corpse, MessageTypeDefOf.CautionInput);
                }
                return false;
            }
            return base.ValidateTarget(target, showMessages);
        }
    }
}