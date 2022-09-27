namespace VanillaPsycastsExpanded;

using RimWorld;
using Verse;

public static class MeditationUtilities
{
    [DebugAction("Pawns", "Check Meditation Focus Strength", true, actionType = DebugActionType.ToolMapForPawns,
                 allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void CheckStrength(Pawn pawn)
    {
        float value = pawn.GetStatValue(StatDefOf.MeditationFocusGain);
        Log.Message(
            $"Value: {value}, Explanation:\n{StatDefOf.MeditationFocusGain.Worker.GetExplanationFull(StatRequest.For(pawn), ToStringNumberSense.Absolute, value)}");

        LocalTargetInfo focus = MeditationUtility.BestFocusAt(pawn.Position, pawn);
        if (!focus.HasThing) return;

        value = focus.Thing.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn);
        Log.Message(
            $"Value: {value}, Explanation:\n{StatDefOf.MeditationFocusStrength.Worker.GetExplanationFull(StatRequest.For(focus.Thing, pawn), ToStringNumberSense.Absolute, value)}");
    }

    public static bool CanUnlock(this MeditationFocusDef focus, Pawn pawn, out string reason)
    {
        if (focus == VPE_DefOf.Dignified &&
            (pawn.royalty == null || !pawn.royalty.AllTitlesForReading.Any() || !pawn.royalty.CanUpdateTitleOfAnyFaction(out _)))
        {
            reason = "VPE.LockedTitle".Translate();
            return false;
        }

        if (focus.GetModExtension<MeditationFocusExtension>() is { canBeUnlocked: false })
        {
            reason = "VPE.LockedLocked".Translate();
            return false;
        }

        reason = null;
        return true;
    }
}