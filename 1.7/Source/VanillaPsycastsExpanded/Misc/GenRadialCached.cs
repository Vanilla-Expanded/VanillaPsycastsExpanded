using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using VEF.CacheClearing;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch]
public static class GenRadialCached
{
    private static Dictionary<Key, HashSet<Thing>> cache = new();
    private static Dictionary<Key, HashSet<CompMeditationFocus>> meditationFocusCache = new();
    private static Dictionary<Key, float> wealthCache = new();

    static GenRadialCached()
    {
        // Just in case, clear caches here
        ClearCaches.clearCacheTypes.Add(typeof(GenRadialCached));
    }

    public static IEnumerable<Thing> RadialDistinctThingsAround(IntVec3 center, Map map, float radius, bool useCenter)
    {
        Key key = new(center, radius, map.Index, useCenter);
        return RadialDistinctThingsAround(ref key, map);
    }

    private static IEnumerable<Thing> RadialDistinctThingsAround(ref readonly Key key, Map map)
    {
        cache ??= new Dictionary<Key, HashSet<Thing>>();
        if (cache.TryGetValue(key, out var things)) return things;
        things = new();
        var numCells = GenRadial.NumCellsInRadius(key.radius);
        for (var i = key.useCenter ? 0 : 1; i < numCells; i++)
        {
            var c = GenRadial.RadialPattern[i] + key.loc;
            if (c.InBounds(map)) things.UnionWith(c.GetThingList(map));
        }

        cache[key] = things;
        return things;
    }

    public static float WealthAround(IntVec3 center, Map map, float radius, bool useCenter)
    {
        Key key = new(center, radius, map.Index, useCenter);

        wealthCache ??= new();
        if (wealthCache.TryGetValue(key, out var value)) return value;

        var things = RadialDistinctThingsAround(ref key, map);
        var wealth = 0f;
        foreach (var thing in things)
            wealth += thing.GetStatValue(StatDefOf.MarketValue) * thing.stackCount;

        wealthCache[key] = wealth;

        return wealth;
    }

    public static IEnumerable<CompMeditationFocus> MeditationFociAround(IntVec3 center, Map map, float radius, bool useCenter)
    {
        Key key = new(center, radius, map.Index, useCenter);

        meditationFocusCache ??= [];
        if (meditationFocusCache.TryGetValue(key, out var value)) return value;

        value = [];
        var things = RadialDistinctThingsAround(ref key, map);
        foreach (var thing in things)
        {
            var comp = thing.TryGetComp<CompMeditationFocus>();
            if (comp != null)
                value.Add(comp);
        }

        meditationFocusCache[key] = value;
        return value;
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.SpawnSetup))]
    [HarmonyPostfix]
    public static void SpawnSetup_Postfix(Thing __instance)
    {
        ClearCacheFor(__instance);
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.DeSpawn))]
    [HarmonyPrefix]
    public static void DeSpawn_Prefix(Thing __instance)
    {
        ClearCacheFor(__instance);
    }

    [HarmonyPatch(typeof(MapDeiniter), nameof(MapDeiniter.Deinit))]
    [HarmonyPostfix]
    public static void Deinit_Postfix(Map map)
    {
        var idx = map.Index;
        foreach (var (key, value) in cache.ToList())
        {
            if (key.mapId >= idx)
            {
                cache.Remove(key);

                if (meditationFocusCache.TryGetValue(key, out var foci))
                    meditationFocusCache.Remove(key);

                if (wealthCache.TryGetValue(key, out var wealth))
                    wealthCache.Remove(key);
                else
                    wealth = float.NaN;

                if (key.mapId != idx)
                {
                    var newKey = key.DecrementMapId();
                    cache.Add(newKey, value);
                    if (foci != null)
                        meditationFocusCache.Add(newKey, foci);
                    if (!float.IsNaN(wealth))
                        wealthCache.Add(newKey, wealth);
                }
            }
        }
    }

    private static void ClearCacheFor(Thing thing)
    {
        if (!thing.Spawned) return;
        cache.RemoveAll(pair =>
        {
            if (pair.Key.mapId != thing.Map.Index || !thing.OccupiedRect().ClosestCellTo(pair.Key.loc).InHorDistOf(pair.Key.loc, pair.Key.radius))
                return false;

            meditationFocusCache.Remove(pair.Key);
            wealthCache.Remove(pair.Key);
            return true;
        });
    }

    private readonly struct Key(IntVec3 loc, float radius, int mapId, bool useCenter) : IEquatable<Key>
    {
        public readonly IntVec3 loc = loc;
        public readonly float radius = radius;
        public readonly int mapId = mapId;
        public readonly bool useCenter = useCenter;

        public Key DecrementMapId() => new Key(loc, radius, mapId - 1, useCenter);

        public bool Equals(Key other)
        {
            return loc.Equals(other.loc) && radius.Equals(other.radius) && mapId == other.mapId && useCenter == other.useCenter;
        }

        public override bool Equals(object obj)
        {
            return obj is Key other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Gen.HashCombineInt(loc.GetHashCode(), radius.GetHashCode(), mapId, useCenter.GetHashCode());
        }
    }
}