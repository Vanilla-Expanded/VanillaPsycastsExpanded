using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using VFECore;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded
{
    public class AbilityExtension_MindWipe : AbilityExtension_AbilityMod
    {
        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
            foreach (GlobalTargetInfo target in targets)
            {
                Pawn pawn = target.Thing as Pawn;
                if (pawn.Faction != ability.pawn.Faction)
                {
                    pawn.SetFaction(ability.pawn.Faction);
                }
                pawn.needs.mood.thoughts.memories.Memories.Clear();
                pawn.relations.ClearAllRelations();
                Dictionary<SkillDef, Passion> passions = new();
                foreach (SkillRecord skillRecord in pawn.skills.skills)
                {
                    passions[skillRecord.def] = skillRecord.passion;
                }
                pawn.skills = new Pawn_SkillTracker(pawn);
                NonPublicMethods.GenerateSkills(pawn, new PawnGenerationRequest(pawn.kindDef, pawn.Faction));
                foreach (KeyValuePair<SkillDef, Passion> kvp in passions)
                {
                    pawn.skills.GetSkill(kvp.Key).passion = kvp.Value;
                }
                if (pawn.ideo.Ideo != ability.pawn.Ideo)
                {
                    pawn.ideo.SetIdeo(ability.pawn.Ideo);
                }
            }
        }
    }
}