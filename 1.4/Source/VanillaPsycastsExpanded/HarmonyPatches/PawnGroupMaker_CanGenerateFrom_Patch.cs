namespace VanillaPsycastsExpanded;

using HarmonyLib;
using RimWorld;
using VFECore.Abilities;

[HarmonyPatch(typeof(PawnGroupMaker), "CanGenerateFrom")]
public static class PawnGroupMaker_CanGenerateFrom_Patch
{
    public static void Postfix(ref bool __result, PawnGroupMaker __instance, PawnGroupMakerParms parms)
    {
        if (__result && __instance is PawnGroupMaker_PsycasterRaid)
            __result = parms.raidStrategy?.Worker is RaidStrategyWorker_ImmediateAttack_Psycasters
                       && __instance.options != null && __instance.options.Exists(x => x.kind.HasModExtension<PawnKindAbilityExtension>());
    }
}