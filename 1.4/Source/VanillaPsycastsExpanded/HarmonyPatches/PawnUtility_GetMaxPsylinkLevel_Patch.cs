using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace VanillaPsycastsExpanded;

public static class GetMaxPsylinkLevel_Patch
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(PawnUtility), nameof(PawnUtility.GetMaxPsylinkLevel))]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        yield return new CodeInstruction(OpCodes.Ldc_I4, 999999);
        yield return new CodeInstruction(OpCodes.Ret);
    }

    [HarmonyPatch]
    public static class FixUsages
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodInfo> TargetMethods()
        {
            yield return AccessTools.Method(typeof(RitualRoleAnimaLinker), nameof(RitualRoleAnimaLinker.AppliesToPawn));
            yield return AccessTools.Method(typeof(Alert_AnimaLinkingReady), "GetTargets");
            yield return AccessTools.Method(typeof(CompPsylinkable), nameof(CompPsylinkable.CanPsylink));
            yield return AccessTools.PropertyGetter(typeof(CompAbilityEffect_StartAnimaLinking), nameof(CompAbilityEffect_StartAnimaLinking.ShouldHideGizmo));
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var info = AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.GetMaxPsylinkLevel));
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.Calls(info))
                {
                    yield return new CodeInstruction(OpCodes.Ldstr, "Initial max");
                    yield return new CodeInstruction(OpCodes.Ldstr, $"{original.DeclaringType?.Name ?? "Free"}.{original.Name}");
                    yield return CodeInstruction.Call(typeof(DebugHelpers), nameof(DebugHelpers.DoLog), generics: new[] { typeof(int) });
                    yield return new CodeInstruction(OpCodes.Ldc_I4, 999999);
                }
            }
        }
    }
}
