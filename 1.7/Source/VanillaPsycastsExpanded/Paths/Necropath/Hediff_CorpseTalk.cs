namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using System.Collections.Generic;
    using UnityEngine;
    using Verse;

    public class Hediff_CorpseTalk : HediffWithComps
    {
        public Dictionary<SkillDef, int> skillXPDifferences = new Dictionary<SkillDef, int>();
        public override void PostRemoved()
        {
            base.PostRemoved();
            ResetSkills();
        }

        public void ResetSkills()
        {
            foreach (var kvp in skillXPDifferences)
            {
                this.pawn.skills.GetSkill(kvp.Key).Level = Mathf.Max(0, this.pawn.skills.GetSkill(kvp.Key).Level - kvp.Value);
            }
            skillXPDifferences.Clear();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref skillXPDifferences, "skillXPDifferences", LookMode.Def, LookMode.Value);
        }
    }
}