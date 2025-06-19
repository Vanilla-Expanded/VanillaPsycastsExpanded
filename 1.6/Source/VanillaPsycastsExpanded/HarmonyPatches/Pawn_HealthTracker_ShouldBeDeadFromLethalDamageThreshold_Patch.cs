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
                    yield return new CodeInstruction(OpCodes.Ldfld, typeof(Pawn_HealthTracker).Field(nameof(Pawn_HealthTracker.hediffSet)));
                    yield return new CodeInstruction(OpCodes.Ldfld, typeof(HediffSet).Field(nameof(HediffSet.hediffs)));
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, typeof(List<Hediff>).IndexerGetter([typeof(int)]));
                    yield return new CodeInstruction(OpCodes.Call, typeof(Pawn_HealthTracker_ShouldBeDeadFromLethalDamageThreshold_Patch).Method(nameof(IsNotRegeneratingHediff)));
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