using Verse;

namespace VanillaPsycastsExpanded.HarmonyPatches;

[StaticConstructorOnStartup]
public static class HarmonyInit
{
    static HarmonyInit()
    {
        PsycastsMod.Harm.PatchAll();
    }
}
