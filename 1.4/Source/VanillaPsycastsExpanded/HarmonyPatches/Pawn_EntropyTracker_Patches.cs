using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using VanillaPsycastsExpanded.UI;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), nameof(Pawn_PsychicEntropyTracker.GetGizmo))]
public static class Pawn_EntropyTracker_GetGizmo_Prefix
{
    [HarmonyPrefix]
    public static void Prefix(Pawn_PsychicEntropyTracker __instance, ref Gizmo ___gizmo)
    {
        ___gizmo ??= new PsychicStatusGizmo(__instance);
    }
}

[HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), nameof(Pawn_PsychicEntropyTracker.GainPsyfocus))]
public static class Pawn_EntropyTracker_GainPsyfocus_Postfix
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codes = instructions.ToList();
        var info = AccessTools.Method(typeof(MeditationUtility), nameof(MeditationUtility.PsyfocusGainPerTick));
        var idx1 = codes.FindIndex(ins => ins.Calls(info));
        var gain = generator.DeclareLocal(typeof(float));
        codes.InsertRange(idx1 + 1, new[]
        {
            new CodeInstruction(OpCodes.Stloc, gain),
            new CodeInstruction(OpCodes.Ldloc, gain)
        });

        var idx2 = codes.FindIndex(ins => ins.opcode == OpCodes.Ret);
        var labels = codes[idx2].ExtractLabels();
        codes.InsertRange(idx2, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0).WithLabels(labels),
            new CodeInstruction(OpCodes.Ldloc, gain),
            CodeInstruction.Call(typeof(Pawn_EntropyTracker_GainPsyfocus_Postfix), nameof(GainXpFromPsyfocus))
        });

        return codes;
    }

    public static void GainXpFromPsyfocus(this Pawn_PsychicEntropyTracker __instance, float gain)
    {
        __instance.Pawn?.Psycasts()?.GainExperience(gain * 100f * PsycastsMod.Settings.XPPerPercent);
    }
}

[HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), nameof(Pawn_PsychicEntropyTracker.OffsetPsyfocusDirectly))]
public static class Pawn_EntropyTracker_OffsetPsyfocusDirectly_Postfix
{
    [HarmonyPostfix]
    public static void Postfix(Pawn_PsychicEntropyTracker __instance, float offset)
    {
        if (offset > 0f) __instance.GainXpFromPsyfocus(offset);
    }
}

[HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), nameof(Pawn_PsychicEntropyTracker.RechargePsyfocus))]
public static class Pawn_EntropyTracker_RechargePsyfocus_Postfix
{
    [HarmonyPrefix]
    public static void Prefix(Pawn_PsychicEntropyTracker __instance)
    {
        __instance.GainXpFromPsyfocus(1f - __instance.CurrentPsyfocus);
    }
}

[HarmonyPatch]
public static class MinHeatPatches
{
    [HarmonyTargetMethods]
    public static IEnumerable<MethodInfo> TargetMethods()
    {
        var type = typeof(Pawn_PsychicEntropyTracker);
        yield return AccessTools.Method(type, nameof(Pawn_PsychicEntropyTracker.TryAddEntropy));
        yield return AccessTools.Method(type, nameof(Pawn_PsychicEntropyTracker.PsychicEntropyTrackerTick));
        yield return AccessTools.Method(type, nameof(Pawn_PsychicEntropyTracker.RemoveAllEntropy));
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        foreach (var instruction in instructions)
            if (!found && instruction.opcode == OpCodes.Ldc_R4 && instruction.operand is 0f)
            {
                found = true;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_PsychicEntropyTracker), "pawn"));
                yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(VPE_DefOf), nameof(VPE_DefOf.VPE_PsychicEntropyMinimum)));
                yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue)));
            }
            else yield return instruction;
    }
}