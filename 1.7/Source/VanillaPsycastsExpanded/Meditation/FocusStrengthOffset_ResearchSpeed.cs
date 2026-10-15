namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using Verse;

    public class FocusStrengthOffset_ResearchSpeed : FocusStrengthOffset
    {
        public override bool CanApply(Thing parent, Pawn user = null) => parent is Building_ResearchBench;

        public override float GetOffset(Thing parent, Pawn user = null) => this.offset * parent.GetStatValue(StatDefOf.ResearchSpeedFactor);

        public override string GetExplanation(Thing parent) =>
            "Difficulty_ResearchSpeedFactor_Label".Translate() + ": " + this.GetOffset(parent).ToStringWithSign("0%");

        public override string GetExplanationAbstract(ThingDef def = null)
        {
            if (def == null) return "";

            return "Difficulty_ResearchSpeedFactor_Label".Translate() + ": " +
                   (this.offset * def.GetStatValueAbstract(StatDefOf.ResearchSpeedFactor)).ToStringWithSign("0%");
        }
    }
}