using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch]
public static class Pawn_PsychicEntropyTracker_OptimizeGetters_Patch
{
    // ################## NOTE WHEN REMOVING THIS PATCH ##################
    // ################## IF THIS EVER GETS FIXED IN VANILLA OR WE NEED TO REMOVE THIS PATCH ##################
    // ################## REMOVE XML PATCH FOR CACHING THE PsychicEntropyMax StatDef ##################
    // ################## END OF NOTE ##################

    // Vanilla code for psychic heat (entropy) is pretty bad at the moment, and causes sever performance drops if any pawn has positive heat.
    // The issues are:
    // - MaxEntropy and MaxPotentialEntropy don't use cached access to their respective stats
    // - MaxPotentialEntropy will always return the same value as MaxEntropy, but with an extra StatDef lookup (basically, if inlined/simplified: Mathf.Max(MaxEntropy, MaxEntropy))
    // - Pawn_PsychicEntropyTracker:EntropyToRelativeValue call the MaxEntropy and MaxPotentialEntropy getters multiple times, each one calculating the stats again
    // - Pawn_PsychicEntropyTracker:EntropyToRelativeValue has 2 branches based on if current heat is higher than max heat, but both will have the same result
    //   - Technically, result will be different if MaxPotentialEntropy is Harmony patched to return something else than MaxEntropy, but I'm not aware of any mods doing that.
    // - Pawn_PsychicEntropyTracker:EntropyToRelativeValue is called many times each second (72 to 240 times a second, varies due to VTR, resulting in stat calculation 216 to 720 times a second)
    // 
    // There's 3 main ways I see how to fix this:
    // - Prefix Pawn_PsychicEntropyTracker:EntropyToRelativeValue, prevent original from running and replace it with optimized version to remove dead code/use cached stat calculations
    // - Transpiler Pawn_PsychicEntropyTracker:EntropyToRelativeValue, replace getters with our own methods that use cached stats
    // - Transpiler to MaxEntropy and MaxPotentialEntropy getters, make them cache their values
    // 
    // Each approach requires adding caching to the PsychicEntropyMax StatDef.
    // On top of that, each approach has its own upsides and downsides.
    // 
    // The first approach:
    // + It will be the fastest, since we could remove repeated getter calls (use a local variable) and avoid conditional branching that's unused
    // + It is the simplest solution
    // - Despite being the fastest, the performance benefit is miniscule over the other 2 approaches
    // - It is the least compatible with other mods, since we could prevent other prefixes from running, or have our prefix stopped from running
    // - It gets rid of the call to MaxPotentialEntropy (could replace it with our own), in case any mod ever uses it they'll need to put in extra work to be compatible
    // 
    // The second approach:
    // + It is very compatible patch with other mods
    // + Almost as fast as the first approach, and should be basically the same as the third one performance-wise
    // - It gets rid of the call to MaxPotentialEntropy (could replace it with our own), in case any mod ever uses it they'll need to put in extra work to be compatible
    //
    // The third approach:
    // + It is (possibly) the most compatible patch with other mods
    // + It preserves the call to MaxPotentialEntropy, so mods that patch this method can still keep doing it without any extra work
    // + Almost as fast as the first approach, and should be basically the same as the second one performance-wise
    // + Other code, including modded, will benefit from caching of the StatDef
    // - Some mods may not want the StatDef to be cached

    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return typeof(Pawn_PsychicEntropyTracker).DeclaredPropertyGetter(nameof(Pawn_PsychicEntropyTracker.MaxEntropy));
        yield return typeof(Pawn_PsychicEntropyTracker).DeclaredPropertyGetter(nameof(Pawn_PsychicEntropyTracker.MaxPotentialEntropy));
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase baseMethod)
    {
        var matcher = new CodeMatcher(instr);

        matcher.MatchEndForward(
            // Loads the "PsychicEntropyMax" on the stack
            CodeMatch.LoadsField(typeof(StatDefOf).DeclaredField(nameof(StatDefOf.PsychicEntropyMax))),
            // Loads true as "applyPostProcess" argument on the stack
            CodeMatch.LoadsConstant(1),
            // Loads -1 as "cacheStaleAfterTicks" (what we're changing to 60) on the stack
            CodeMatch.LoadsConstant(-1),
            // Calls the method we're after
            CodeMatch.Calls(() => StatExtension.GetStatValue)
        );

        // Check if we had a match
        if (matcher.IsInvalid)
        {
            Log.Error($"Patch to optimize {baseMethod.DeclaringType?.Name}.{baseMethod.Name} failed, could not find code sequence responsible for accessing max psychic heat. Either vanilla code changed (was fixed?), or another mod modified this code.");
            return matcher.Instructions();
        }

        // If we found a match, move back a single position
        matcher.Advance(-1);

        // Replace current instruction with loading a constat value of 60 to the stack instead.
        // We could just use something like "matcher.SetInstruction(CodeInstruction), but we'd lose any labels/exception blocks. There aren't any, but better safe than sorry.
        matcher.Opcode = OpCodes.Ldc_I4_S;
        matcher.Operand = 60;

        return matcher.Instructions();
    }
}