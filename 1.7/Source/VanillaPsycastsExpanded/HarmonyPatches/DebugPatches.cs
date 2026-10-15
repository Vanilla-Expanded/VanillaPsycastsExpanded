using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace VanillaPsycastsExpanded;

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
