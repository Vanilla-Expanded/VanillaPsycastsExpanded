namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using Verse;

    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.ShouldBeDeadFromLethalDamageThreshold))]
    public static class Pawn_HealthTracker_ShouldBeDeadFromLethalDamageThreshold_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();
            var label = generator.DefineLabel();
            for (var i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (codes[i].opcode == OpCodes.Brfalse_S && codes[i - 1].opcode == OpCodes.Isinst && codes[i - 1].OperandIs(typeof(Hediff_Injury)))
                {
                    codes[i + 1].labels.Add(label);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_HealthTracker), "hediffSet"));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(HediffSet), "hediffs"));
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<Hediff>), "get_Item"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Pawn_HealthTracker_ShouldBeDeadFromLethalDamageThreshold_Patch), nameof(IsNotRegeneratingHediff)));
                    yield return new CodeInstruction(OpCodes.Brfalse_S, codes[i].operand);
                }
            }
        }

        public static bool IsNotRegeneratingHediff(Hediff hediff)
        {
            return hediff.def != VPE_DefOf.VPE_Regenerating;
        }
    }
}