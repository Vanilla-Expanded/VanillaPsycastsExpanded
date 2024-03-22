namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Verse.Sound;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;
    public class Ability_GoodwillImpact : Ability
    {
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (target.Thing is Pawn targetPawn)
            {
                if (targetPawn.HostileTo(this.pawn) || targetPawn.Faction == this.pawn.Faction || targetPawn.Faction is null)
                {
                    if (showMessages)
                    {
                        Messages.Message("VPE.MustBeAllyOrNeutral".Translate(), targetPawn, MessageTypeDefOf.CautionInput);
                    }
                    return false;
                }
            }
            return base.ValidateTarget(target, showMessages);
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            foreach (var target in targets)
            {
                var targetPawn = target.Thing as Pawn;
                var goodwillImpact = (int)Mathf.Max(10f, (this.pawn.GetStatValue(StatDefOf.PsychicSensitivity) * 100) - 100);
                targetPawn.Faction.TryAffectGoodwillWith(this.pawn.Faction, goodwillImpact);
            }
        }
    }
}