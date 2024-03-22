namespace VanillaPsycastsExpanded
{
    using System.Linq;
    using RimWorld;
    using RimWorld.Planet;
    using Verse;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_SnapFreeze : Ability
    {
        public IntVec3 targetCell;

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            AbilityExtension_Hediff hediffExtension = this.def.GetModExtension<AbilityExtension_Hediff>();
            if (hediffExtension.targetOnlyEnemies && target.Thing != null && !target.Thing.HostileTo(this.pawn))
            {
                if (showMessages) Messages.Message("VFEA.TargetMustBeHostile".Translate(), target.Thing, MessageTypeDefOf.CautionInput, null);
                return false;
            }

            return base.ValidateTarget(target, showMessages);
        }

        public override void ModifyTargets(ref GlobalTargetInfo[] targets)
        {
            this.targetCell = targets[0].Cell;
            base.ModifyTargets(ref targets);
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            Effecter effecter = EffecterDefOf.Skip_Exit.Spawn(this.targetCell, this.pawn.Map, 3f);
            this.AddEffecterToMaintain(effecter, this.targetCell, 60);
        }

        public override void ApplyHediffs(params GlobalTargetInfo[] targetInfo)
        {
            foreach (GlobalTargetInfo target in targetInfo) ApplyHediff(this, (LocalTargetInfo) target);
        }
        public static void ApplyHediff(Ability ability, LocalTargetInfo targetInfo)
        {
            AbilityExtension_Hediff hediffExtension = ability.def.GetModExtension<AbilityExtension_Hediff>();
            if (targetInfo.Pawn == null || !(hediffExtension?.applyAuto ?? false)) return;
            BodyPartRecord bodyPart = hediffExtension.bodyPartToApply != null
                ? ability.pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault(x => x.def == hediffExtension.bodyPartToApply)
                : null;
            Hediff localHediff = HediffMaker.MakeHediff(hediffExtension.hediff, targetInfo.Pawn, bodyPart);
            if (hediffExtension.severity > float.Epsilon)
                localHediff.Severity = hediffExtension.severity;
            if (localHediff is Hediff_Ability hediffAbility) hediffAbility.ability = ability;
            int   duration = ability.GetDurationForPawn();
            float ambientTemperature = targetInfo.Pawn.AmbientTemperature;
            if (ambientTemperature                 >= 0) duration = (int) (duration * (1f - ambientTemperature / 100f));
            if (hediffExtension.durationMultiplier != null) duration = (int) (duration * targetInfo.Pawn.GetStatValue(hediffExtension.durationMultiplier));
            if (localHediff is HediffWithComps hwc)
                foreach (HediffComp hediffComp in hwc.comps)
                    switch (hediffComp)
                    {
                        case HediffComp_Ability hca:
                            hca.ability = ability;
                            break;
                        case HediffComp_Disappears hcd:
                            hcd.ticksToDisappear = duration;
                            break;
                    }

            targetInfo.Pawn.health.AddHediff(localHediff);
        }
    }
}