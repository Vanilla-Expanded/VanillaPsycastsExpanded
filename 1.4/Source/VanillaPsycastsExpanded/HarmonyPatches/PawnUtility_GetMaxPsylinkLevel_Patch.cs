namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;

    [HarmonyPatch(typeof(PawnUtility), nameof(PawnUtility.GetMaxPsylinkLevel))]
    public class PawnUtility_GetMaxPsylinkLevel_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref int __result)
        {
            __result = int.MaxValue;
            return false;
        }
    }
}