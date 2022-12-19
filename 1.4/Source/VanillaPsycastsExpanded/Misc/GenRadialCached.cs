using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace VanillaPsycastsExpanded;

[HarmonyPatch]
public static class GenRadialCached
{
    private static readonly Dictionary<Key, HashSet<Thing>> cache = new();

    public static IEnumerable<Thing> RadialDistinctThingsAround(IntVec3 center, Map map, float radius, bool useCenter)
    {
        Key key = new()
        {
            loc = center,
            radius = radius,
            mapId = map.Index
        };
        if (cache.TryGetValue(key, out var things)) return things;
        things = new HashSet<Thing>();
        var numCells = GenRadial.NumCellsInRadius(radius);
        for (var i = useCenter ? 0 : 1; i < numCells; i++)
        {
            var c = GenRadial.RadialPattern[i] + center;
            if (c.InBounds(map)) things.UnionWith(c.GetThingList(map));
        }

        cache.Add(key, things);
        return things;
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

    [HarmonyPatch(typeof(MapDeiniter), nameof(MapDeiniter.Deinit_NewTemp))]
    [HarmonyPostfix]
    public static void Deinit_Postfix(Map map)
    {
        var idx = map.Index;
        foreach ((var key, var value) in cache.ToList())
        {
            if (key.mapId >= idx) cache.Remove(key);
            if (key.mapId > idx)
            {
                var newKey = key;
                newKey.mapId--;
                cache.Add(key, value);
            }
        }
    }

    private static void ClearCacheFor(Thing thing)
    {
        if (!thing.Spawned) return;
        cache.RemoveAll(
            pair => pair.Key.mapId == thing.Map.Index && thing.OccupiedRect().ClosestCellTo(pair.Key.loc).InHorDistOf(pair.Key.loc, pair.Key.radius));
    }

    private struct Key
    {
        public IntVec3 loc;
        public float radius;
        public int mapId;
    }
}
