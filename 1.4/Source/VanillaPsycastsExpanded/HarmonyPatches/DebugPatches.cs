using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VanillaPsycastsExpanded;

//[HarmonyPatch(typeof(RitualRoleAnimaLinker), nameof(RitualRoleAnimaLinker.AppliesToPawn))]
//public static class AppliesToPawnDebug
//{
//    [HarmonyPrefix]
//    public static void Prefix(ref bool skipReason, Pawn p)
//    {
//        skipReason = false;
//        Log.Message($"max={AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.GetMaxPsylinkLevel)).Invoke(null, new object[] { p })}");
//    }
//
//    [HarmonyPostfix]
//    public static void Postfix(Pawn p, bool __result, string reason)
//    {
//        Log.Message($"{p} can link: {__result} because {reason}"
//                  + $"\nnatural={MeditationFocusDefOf.Natural.CanPawnUse(p)}, level={p.GetPsylinkLevel()}, max={p.GetMaxPsylinkLevel()}"
//                  + $"\nCheck: {!MeditationFocusDefOf.Natural.CanPawnUse(p) || p.GetPsylinkLevel() >= p.GetMaxPsylinkLevel()}");
//    }
//
//    [HarmonyTranspiler]
//    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
//    {
//        var info = AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.GetMaxPsylinkLevel));
//        foreach (var instruction in instructions.AddLogs(original, new List<MethodInfo>
//                 {
//                     AccessTools.Method(typeof(MeditationFocusDef), nameof(MeditationFocusDef.CanPawnUse)),
//                     AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.GetPsylinkLevel)),
//                     info
//                 }))
//            yield return instruction;
//    }
//}

[HarmonyPatch]
public static class MiscDebugPatches { }

public static class DebugHelpers
{
    public static IEnumerable<CodeInstruction> AddLogs(this IEnumerable<CodeInstruction> instructions, MethodBase original, List<MethodInfo> methods)
    {
        foreach (var instruction in instructions)
        {
            yield return instruction;
            if (methods.FirstOrDefault(instruction.Calls) is { Name: var name, ReturnType: var result })
            {
                yield return new CodeInstruction(OpCodes.Dup);
                yield return new CodeInstruction(OpCodes.Ldstr, name);
                yield return new CodeInstruction(OpCodes.Ldstr, $"{original.DeclaringType?.Name ?? "Free"}.{original.Name}");
                yield return CodeInstruction.Call(typeof(DebugHelpers), nameof(DoLog), generics: new[] { result });
            }
        }
    }

    public static void DoLog<T>(T obj, string header, string context) => Log.Message($"[{context}] {header}: {obj}");
}
