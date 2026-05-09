namespace VanillaPsycastsExpanded;

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;


/// <summary>
/// The introduction of stacking focus types with complex focus strength calculations makes per-tick calculation of meditation focus
/// very computationally expensive. This can easily slow down games noticeably with even a single meditating pawn.
/// 
/// This class serves as a buffer, caching the boosted gain to a meditation focus from nearby foci, which massively speeds up
/// the function call for StatPart_NearbyFoci.TransformValue. Cached values expire after an hour, so in normal gameplay pawns will
/// never lose/gain more than a few percentage points of psyfocus compared to vanilla calculations, and only when meditation objects are
/// actively moved around a meditating pawn.
/// </summary>
public static class NearbyFocusBoostCachingUtility
{
    private static int EXPIRATION_TIMEOUT = 2500; // 1 in-game hour

    private class CachedFocusInfo : Tuple<Pawn, Thing, float, int>
    {
        //private bool usedSinceLastCalcuation = false;
        private CachedFocusInfo(Pawn pawn, Thing focusTarget, float focus, int expiration) : base(pawn, focusTarget, focus, expiration) { }

        public static CachedFocusInfo GenerateCacheInfoFor(Pawn pawn, Thing focusTarget, float baseVal)
        {
            float modifiedPsyfocus = baseVal;
            var list = StatPart_NearbyFoci.AllFociNearby(focusTarget, pawn);
            for (var i = 0; i < list.Count; i++) modifiedPsyfocus += list[i].value;


            return new CachedFocusInfo(
                pawn,
                focusTarget,
                modifiedPsyfocus,
                Current.Game.tickManager.TicksGame + EXPIRATION_TIMEOUT
                );
        }

        public Pawn Pawn { get { return this.Item1; } }
        public Thing FocusTarget { get { return this.Item2; } }
        public float ModifiedFocus { get { return this.Item3; } }
        public int ExpirationTick { get { return this.Item4; } }

        public bool IsExpired { get { return ExpirationTick < Current.Game.tickManager.TicksGame; } }
    }

    private static Dictionary<string, CachedFocusInfo> cachedInfo = new Dictionary<string, CachedFocusInfo>();

    /** Tries to get the Modified psyfocus of a medititation foci for a given pawn, taking into account
     * other nearby meditation foci that are valid for the pawn, and the foci's base psyfocus gain.
     * If the value is cached and not stale, return that, otherwise recalculate the value and re-cache it
     * before returning it.
     * 
     */
    public static float GetOrCacheModifiedFociBoost(Pawn pawn, Thing focus, float basePsyfocus)
    {
        // Check if info is cached
        CachedFocusInfo info;
        string cacheKey = pawn.ThingID + focus.ThingID;
        if (cachedInfo.ContainsKey(cacheKey))
        {
            info = cachedInfo[cacheKey];
            // Check if info is still useable
            if(!info.IsExpired)
            {
                return info.ModifiedFocus;
            }
        } 
        info = CachedFocusInfo.GenerateCacheInfoFor(pawn, focus, basePsyfocus);
        cachedInfo[cacheKey] = info;

        // Cleanup dictionary whenever we modify it to avoid caching dozens of visiting nobles' info forever
        CleanupCache();

        return info.ModifiedFocus;

    }

    private static void CleanupCache()
    {
        List<string> removeIds = new List<string>();
        foreach (string id in cachedInfo.Keys) if (cachedInfo[id].IsExpired) removeIds.Add(id);
        foreach (string id in removeIds)
        {
            //Log.Message($"Removing stale info for {id}");
            cachedInfo.Remove(id);
        }
    }

}
