namespace VanillaPsycastsExpanded.Technomancer
{
    using System.Collections.Generic;
    using System.Linq;
    using HarmonyLib;
    using RimWorld;
    using RimWorld.Planet;
    using Verse;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_ReverseEngineer : Ability
    {
        private static readonly Dictionary<Thing, List<ResearchProjectDef>> researchCache = new();

        private static readonly AccessTools.FieldRef<ResearchManager, Dictionary<ResearchProjectDef, float>> progressRef = AccessTools
            .FieldRefAccess<ResearchManager,
                Dictionary<ResearchProjectDef, float>>("progress");

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            foreach (ResearchProjectDef project in GetResearchFor(targets[0].Thing).Where(project => project is {IsFinished: false}))
            {
                int techprints = Find.ResearchManager.GetTechprints(project);
                if (techprints < project.TechprintCount) Find.ResearchManager.AddTechprints(project, techprints - project.TechprintCount);
                progressRef(Find.ResearchManager)[project] = project.baseCost;
                Find.ResearchManager.ReapplyAllMods();
                TaleRecorder.RecordTale(TaleDefOf.FinishedResearchProject, this.pawn, project);
                DiaNode diaNode = new("ResearchFinished".Translate(project.LabelCap) + "\n\n" + project.description);
                diaNode.options.Add(DiaOption.DefaultOK);
                Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true));
                if (!project.discoveredLetterTitle.NullOrEmpty() && Find.Storyteller.difficulty.AllowedBy(project.discoveredLetterDisabledWhen))
                    Find.LetterStack.ReceiveLetter(project.discoveredLetterTitle, project.discoveredLetterText, LetterDefOf.NeutralEvent);
            }

            targets[0].Thing.Destroy();
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!base.ValidateTarget(target, showMessages)) return false;
            if (!target.HasThing) return false;

            List<ResearchProjectDef> projects = GetResearchFor(target.Thing);
            if (projects.NullOrEmpty())
            {
                if (showMessages) Messages.Message("VPE.Research".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            if (projects.TrueForAll(project => project.IsFinished))
            {
                if (showMessages) Messages.Message("VPE.AlreadyResearch".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return true;
        }

        private static List<ResearchProjectDef> GetResearchFor(Thing t)
        {
            if (researchCache.TryGetValue(t, out List<ResearchProjectDef> projects)) return projects;
            projects = new List<ResearchProjectDef>();
            if (!t.def.researchPrerequisites.NullOrEmpty()) projects.AddRange(t.def.researchPrerequisites);
            if (t.def.recipeMaker != null)
            {
                if (t.def.recipeMaker.researchPrerequisite != null) projects.Add(t.def.recipeMaker.researchPrerequisite);
                if (!t.def.recipeMaker.researchPrerequisites.NullOrEmpty()) projects.AddRange(t.def.recipeMaker.researchPrerequisites);
            }

            foreach (RecipeDef recipeDef in DefDatabase<RecipeDef>.AllDefs)
                if (recipeDef.products.Any(prod => prod.thingDef == t.def))
                {
                    if (recipeDef.researchPrerequisite != null) projects.Add(recipeDef.researchPrerequisite);
                    if (!recipeDef.researchPrerequisites.NullOrEmpty()) projects.AddRange(recipeDef.researchPrerequisites);
                }

            projects.RemoveAll(proj => proj == null);
            return researchCache[t] = projects;
        }
    }
}