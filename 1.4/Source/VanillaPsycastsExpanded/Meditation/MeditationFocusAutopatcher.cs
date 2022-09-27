namespace VanillaPsycastsExpanded
{
    using System.Collections.Generic;
    using RimWorld;
    using Verse;

    [StaticConstructorOnStartup]
    internal class MeditationFocusAutopatcher
    {
        static MeditationFocusAutopatcher()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.thingClass != null && typeof(Building_ResearchBench).IsAssignableFrom(def.thingClass))
                {
                    def.comps ??= new List<CompProperties>();
                    def.comps.Add(new CompProperties_MeditationFocus
                    {
                        statDef    = StatDefOf.MeditationFocusStrength,
                        focusTypes = new List<MeditationFocusDef> {VPE_DefOf.VPE_Science},
                        offsets = new List<FocusStrengthOffset>
                        {
                            new FocusStrengthOffset_ResearchSpeed
                            {
                                offset = 0.5f
                            }
                        }
                    });
                    def.statBases ??= new List<StatModifier>();
                    def.statBases.Add(
                        new StatModifier
                        {
                            stat  = StatDefOf.MeditationFocusStrength,
                            value = 0.0f
                        }
                    );
                }

                if (def.techLevel == TechLevel.Archotech)
                {
                    def.comps ??= new List<CompProperties>();
                    def.comps.Add(new CompProperties_MeditationFocus
                    {
                        statDef    = StatDefOf.MeditationFocusStrength,
                        focusTypes = new List<MeditationFocusDef> {VPE_DefOf.VPE_Archotech},
                        offsets = new List<FocusStrengthOffset>
                        {
                            new FocusStrengthOffset_NearbyOfTechlevel
                            {
                                radius    = 4.9f,
                                techLevel = TechLevel.Archotech
                            }
                        }
                    });
                    def.statBases ??= new List<StatModifier>();
                    def.statBases.Add(
                        new StatModifier
                        {
                            stat  = StatDefOf.MeditationFocusStrength,
                            value = 0.0f
                        }
                    );
                }
            }
        }
    }
}