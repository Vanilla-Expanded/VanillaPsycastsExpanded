namespace VanillaPsycastsExpanded.HarmonyPatches
{
    using Verse;

    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            PsycastsMod.Harm.PatchAll();
        }
    }
}