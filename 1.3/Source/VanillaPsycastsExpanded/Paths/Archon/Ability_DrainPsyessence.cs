namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using Verse;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_DrainPsyessence : Ability
    {
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (target.Thing is Pawn victim)
            {
                if (!victim.Downed)
                {
                    if (showMessages)
                    {
                        Messages.Message("VPE.MustBeDowned".Translate(), victim, MessageTypeDefOf.CautionInput);
                    }
                    return false;
                }
                if (victim.Psycasts() is null || victim.Psycasts().level < 1)
                {
                    if (showMessages)
                    {
                        Messages.Message("VPE.MustHavePsychicLevel".Translate(), victim, MessageTypeDefOf.CautionInput);
                    }
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
                var targetPsycasts = targetPawn.Psycasts();
                var pawnPsycasts = pawn.Psycasts();
                var targetPawnLevel = targetPsycasts.level;

                targetPsycasts.experience = 0;
                pawnPsycasts.GainExperience(targetPsycasts.experience);
                float previousExperience = 0;
                for (var i = 0; i < targetPawnLevel; i++)
                {
                    previousExperience += Hediff_PsycastAbilities.ExperienceRequiredForLevel(i);
                }
                pawnPsycasts.GainExperience(previousExperience);

                targetPawn.health.RemoveHediff(targetPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicAmplifier));
                targetPawn.health.RemoveHediff(targetPawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_PsycastAbilityImplant));


                targetPawn.Kill(null);
                targetPawn.Corpse.GetComp<CompRottable>().RotProgress += 1200000f;
                FilthMaker.TryMakeFilth(targetPawn.Corpse.Position, targetPawn.Corpse.Map, ThingDefOf.Filth_CorpseBile, 3);
                MoteBetween mote = (MoteBetween)ThingMaker.MakeThing(VPE_DefOf.VPE_PsycastPsychicEffectTransfer);
                mote.Attach(targetPawn.Corpse, this.pawn);
                mote.Scale = 1f;
                mote.exactPosition = targetPawn.Corpse.DrawPos;
                GenSpawn.Spawn(mote, targetPawn.Corpse.Position, targetPawn.MapHeld);
            }

            foreach (Faction allFaction in Find.FactionManager.AllFactions)
            {
                if (!allFaction.IsPlayer && !allFaction.defeated)
                {
                    Faction.OfPlayer.TryAffectGoodwillWith(allFaction, -15, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.UsedHarmfulAbility);
                }
            }
        }
    }
}