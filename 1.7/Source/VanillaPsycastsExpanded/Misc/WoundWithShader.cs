namespace VanillaPsycastsExpanded
{
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using HarmonyLib;
    using RimWorld;
    using Verse;

    [HarmonyPatch]
    public class WoundWithShader : FleshTypeDef.Wound
    {
        public ShaderTypeDef shader;

        [HarmonyPatch(typeof(FleshTypeDef.Wound), nameof(Resolve))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Resolve_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            LocalBuilder woundLocal = generator.DeclareLocal(typeof(WoundWithShader));

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    Label skip = generator.DefineLabel();

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(WoundWithShader));
                    yield return new CodeInstruction(OpCodes.Stloc, woundLocal.LocalIndex);

                    yield return new CodeInstruction(OpCodes.Ldloc, woundLocal.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Brfalse, skip);

                    yield return new CodeInstruction(OpCodes.Ldloc, woundLocal.LocalIndex);
                    yield return new CodeInstruction(
                        OpCodes.Ldfld,
                        AccessTools.Field(typeof(WoundWithShader), nameof(shader)));

                    yield return new CodeInstruction(
                        OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(ShaderTypeDef), nameof(ShaderTypeDef.Shader)));

                    yield return new CodeInstruction(OpCodes.Stloc_0);

                    yield return new CodeInstruction(OpCodes.Nop).WithLabels(skip);
                }
            }
        }
    }
}