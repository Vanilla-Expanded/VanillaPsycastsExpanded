namespace VanillaPsycastsExpanded;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

[HarmonyPatch(typeof(RitualOutcomeEffectWorker_Bestowing), nameof(RitualOutcomeEffectWorker_Bestowing.Apply))]
public class RitualOutcomeEffectWorker_Bestowing_Apply_Patch
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = instructions.ToList();
        MethodInfo            info1 = AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.GetPsylinkLevel));
        MethodInfo            info2 = AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.GetMaxPsylinkLevelByTitle));

        int idx1 = codes.FindIndex(ins => ins.Calls(info1)) - 1;
        int idx2 = codes.FindIndex(ins => ins.Calls(info2)) + 1;

        codes.RemoveRange(idx1, idx2 - idx1 + 1);
        codes.InsertRange(idx1, new[]
        {
            new CodeInstruction(OpCodes.Ldloc_2),
            new CodeInstruction(OpCodes.Ldloc, 9),
            new CodeInstruction(OpCodes.Ldloc, 10),
            CodeInstruction.Call(typeof(RitualOutcomeEffectWorker_Bestowing_Apply_Patch), nameof(ApplyTitlePsylink))
        });
        return codes;
    }

    public static void ApplyTitlePsylink(Pawn pawn, RoyalTitleDef oldTitle, RoyalTitleDef newTitle)
    {
        Hediff_PsycastAbilities psylink = pawn.Psycasts();
        int                     newMax  = newTitle.maxPsylinkLevel;
        int                     oldMax  = oldTitle?.maxPsylinkLevel ?? 0;

        if (psylink == null)
        {
            pawn.ChangePsylinkLevel(newMax - oldMax, false);
            pawn.Psycasts().maxLevelFromTitles = newMax;
            return;
        }

        if (psylink.maxLevelFromTitles > newMax) return;
        psylink.ChangeLevel(newMax - oldMax, false);
        psylink.maxLevelFromTitles = newMax;
    }
}