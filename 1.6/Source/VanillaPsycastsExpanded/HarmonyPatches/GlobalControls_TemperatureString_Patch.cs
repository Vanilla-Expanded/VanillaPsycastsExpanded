namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using Verse;

    [HarmonyPatch(typeof(GlobalControls), "TemperatureString")]
    public static class GlobalControls_TemperatureString_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                yield return code;
                if (code.opcode == OpCodes.Stloc_S && code.operand is LocalBuilder lb && lb.LocalIndex == 4)
                {
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 4);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Find), nameof(Find.CurrentMap)));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GlobalControls_TemperatureString_Patch), nameof(ModifyTemperatureIfNeeded)));
                }
            }
        }

        public static void ModifyTemperatureIfNeeded(ref float result, IntVec3 cell, Map map)
        {
            if (GenTemperature_GetTemperatureForCell_Patch.cachedComp?.map != map)
            {
                GenTemperature_GetTemperatureForCell_Patch.cachedComp = map.GetComponent<MapComponent_PsycastsManager>();
            }
            if (GenTemperature_GetTemperatureForCell_Patch.cachedComp.TryGetOverridenTemperatureFor(cell, out var value))
            {
                result = value;
            }
        }
    }
}