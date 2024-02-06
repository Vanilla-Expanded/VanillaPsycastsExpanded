using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch(typeof(Hediff_Psylink), nameof(Hediff_Psylink.PostAdd))]
public static class Hediff_Psylink_PostAdd
{
    public static void Postfix(Hediff_Psylink __instance)
    {
        ((Hediff_PsycastAbilities)__instance.pawn.health.AddHediff(VPE_DefOf.VPE_PsycastAbilityImplant, __instance.Part))
           .InitializeFromPsylink(__instance);
    }
}

[HarmonyPatch(typeof(Hediff_Psylink), nameof(Hediff_Psylink.ChangeLevel), typeof(int), typeof(bool))]
public static class Hediff_Psylink_ChangeLevel
{
    public static bool Prefix(Hediff_Psylink __instance, int levelOffset, ref bool sendLetter)
    {
        __instance.pawn.Psycasts().ChangeLevel(levelOffset, sendLetter);
        sendLetter = false;
        return false;
    }
}

[HarmonyPatch(typeof(Hediff_Psylink), nameof(Hediff_Psylink.TryGiveAbilityOfLevel))]
public static class Hediff_Psylink_TryGiveAbilityOfLevel
{
    public static bool Prefix() => false;
}

[HarmonyPatch(typeof(DebugToolsPawns), "GivePsylink")]
public static class DebugToolsPawns_GivePsylink
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var info1 = AccessTools.Field(typeof(HediffDefOf), nameof(HediffDefOf.PsychicAmplifier));
        var info2 = AccessTools.Field(typeof(HediffDef), nameof(HediffDef.maxSeverity));
        foreach (var instruction in instructions)
            if (instruction.LoadsField(info1)) continue;
            else if (instruction.LoadsField(info2))
                yield return new(OpCodes.Ldsfld, AccessTools.Field(typeof(PsycasterPathDef), nameof(PsycasterPathDef.TotalPoints)));
            else yield return instruction;
    }
}
