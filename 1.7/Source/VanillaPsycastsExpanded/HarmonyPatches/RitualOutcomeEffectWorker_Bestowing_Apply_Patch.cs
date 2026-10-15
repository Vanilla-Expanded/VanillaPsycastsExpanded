using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch(typeof(RitualOutcomeEffectWorker_Bestowing), nameof(RitualOutcomeEffectWorker_Bestowing.Apply))]
public class RitualOutcomeEffectWorker_Bestowing_Apply_Patch
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        var info1 = AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.GetPsylinkLevel));
        var info2 = AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.GetMaxPsylinkLevelByTitle));

        var idx1 = codes.FindIndex(ins => ins.Calls(info1)) - 1;
        var idx2 = codes.FindIndex(ins => ins.Calls(info2)) + 1;

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
        var psylink = pawn.Psycasts();
        var newMax = newTitle.maxPsylinkLevel;
        var oldMax = oldTitle?.maxPsylinkLevel ?? 0;

        if (psylink == null)
        {
            pawn.ChangePsylinkLevel(1, false); // ChangePsylinkLevel ignores levelOffset when no psylink is present
            psylink = pawn.Psycasts();
            psylink.ChangeLevel(newMax - oldMax, false);
            psylink.maxLevelFromTitles = newMax;
            return;
        }

        if (psylink.maxLevelFromTitles > newMax) return;
        psylink.ChangeLevel(newMax - oldMax, false);
        psylink.maxLevelFromTitles = newMax;
    }
}
