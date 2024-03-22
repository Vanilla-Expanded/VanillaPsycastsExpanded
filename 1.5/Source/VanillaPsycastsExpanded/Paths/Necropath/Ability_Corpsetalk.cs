namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    using Verse.AI;
    using VFECore.Abilities;
    public class Ability_Corpsetalk : Ability_TargetCorpse
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var hediff = this.ApplyHediff(this.pawn) as Hediff_CorpseTalk;
            if (hediff.skillXPDifferences != null)
            {
                hediff.ResetSkills();
            }
            else
            {
                hediff.skillXPDifferences = new Dictionary<SkillDef, int>();
            }
            var corpse = targets[0].Thing as Corpse;
            foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
            {
                var pawnSkillRecord = pawn.skills.GetSkill(skillDef);
                var corpseSkillRecord = corpse.InnerPawn.skills.GetSkill(skillDef);
                var diff = corpseSkillRecord.Level - pawnSkillRecord.Level;
                if (diff > 0)
                {
                    var oldValue = pawnSkillRecord.Level;
                    pawnSkillRecord.Level = Mathf.Min(20, pawnSkillRecord.Level + diff);
                    hediff.skillXPDifferences[skillDef] = pawnSkillRecord.Level - oldValue;
                }
            }
        }
    }
}