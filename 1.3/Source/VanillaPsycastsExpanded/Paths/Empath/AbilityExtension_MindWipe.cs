namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using Verse;
using VFECore;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;
    public class AbilityExtension_MindWipe : AbilityExtension_AbilityMod
	{
        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
            foreach (var target in targets)
            {
                var pawn = target.Thing as Pawn;
                if (pawn.Faction != ability.pawn.Faction)
                {
                    pawn.SetFaction(ability.pawn.Faction);
                }
                pawn.needs.mood.thoughts.memories.Memories.Clear();
                pawn.relations.ClearAllRelations();
                var passions = new Dictionary<SkillDef, Passion>();
                foreach (var skillRecord in pawn.skills.skills)
                {
                    passions[skillRecord.def] = skillRecord.passion;
                }
                pawn.skills = new Pawn_SkillTracker(pawn);
                NonPublicMethods.GenerateSkills(pawn);
                foreach (var kvp in passions)
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