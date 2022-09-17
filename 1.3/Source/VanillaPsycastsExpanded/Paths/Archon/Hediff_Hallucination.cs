namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using System.Collections.Generic;
    using Verse;

    public class Hediff_Hallucination : HediffWithComps
    {
        public static List<ThoughtDef> thoughtsToChange = new List<ThoughtDef>
        {
            ThoughtDefOf.AteInImpressiveDiningRoom,
            ThoughtDefOf.JoyActivityInImpressiveRecRoom,
            ThoughtDefOf.SleptInBedroom,
            ThoughtDefOf.SleptInBarracks,
        };
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            foreach (var thoughtDef in thoughtsToChange)
            {
                var memory = pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(thoughtDef);
                if (memory != null)
                {
                    memory.SetForcedStage(thoughtDef.stages.Count - 1);
                }
            }
        }
    }
}