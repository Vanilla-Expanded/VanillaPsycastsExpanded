namespace VanillaPsycastsExpanded.Skipmaster;

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

[HarmonyPatch]
public static class SkipdoorPatches
{
    [HarmonyPatch(typeof(Settlement), nameof(Settlement.GetFloatMenuOptions))]
    [HarmonyPostfix]
    public static void SettlementFloatOptions_Postfix(ref IEnumerable<FloatMenuOption> __result, Settlement __instance, Caravan caravan)
    {
        if (!__instance.HasMap) return;
        Skipdoor       origin    = null;
        HashSet<Map>   maps      = new();
        List<Skipdoor> skipdoors = new();
        foreach (Skipdoor skipdoor in WorldComponent_SkipdoorManager.Instance.Skipdoors)
            if (skipdoor.Map == __instance.Map) origin = skipdoor;
            else if (!maps.Contains(skipdoor.Map))
            {
                maps.Add(skipdoor.Map);
                skipdoors.Add(skipdoor);
            }

        if (origin != null)
            __result = __result.Concat(
                skipdoors.SelectMany(skipdoor => CaravanArrivalAction_UseSkipdoor.GetFloatMenuOptions(caravan, origin, skipdoor)));
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    [HarmonyPrefix]
    public static void Pawn_Kill_Prefix(Pawn __instance)
    {
        if (__instance.Faction is { IsPlayer: true })
            foreach (Skipdoor skipdoor in WorldComponent_SkipdoorManager.Instance.Skipdoors.ToList())
                if (skipdoor.Pawn == __instance)
                {
                    GenExplosion.DoExplosion(skipdoor.Position, skipdoor.Map, 4.9f, DamageDefOf.Bomb, skipdoor, 35);
                    if (!skipdoor.Destroyed) skipdoor.Destroy();
                }
    }

    [HarmonyPatch(typeof(MapDeiniter), nameof(MapDeiniter.Deinit))]
    [HarmonyPrefix]
    public static void Deinit_Prefix(Map map)
    {
        WorldComponent_SkipdoorManager.Instance.Skipdoors.RemoveAll(skipdoor => skipdoor.Map == map);
    }
}