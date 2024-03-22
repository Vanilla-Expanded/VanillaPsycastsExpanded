using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaPsycastsExpanded.Nightstalker;

[HarmonyPatch]
public class GameCondition_IntenseShadows : GameCondition
{
    private static readonly HashSet<Map> intenseShadowMaps = new();
    public override SkyTarget? SkyTarget(Map map) => new SkyTarget(1f, new(Color.gray, Color.black, Color.black, 1f), 0.25f, 0.25f);
    public override float SkyTargetLerpFactor(Map map) => 1f;

    public override void Init()
    {
        base.Init();
        intenseShadowMaps.UnionWith(AffectedMaps);
    }

    public override void End()
    {
        foreach (var map in AffectedMaps) intenseShadowMaps.Remove(map);
        base.End();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        if (Scribe.mode == LoadSaveMode.PostLoadInit) intenseShadowMaps.UnionWith(AffectedMaps);
    }

    [HarmonyPatch(typeof(GlowGrid), nameof(GlowGrid.GroundGlowAt))]
    [HarmonyPostfix]
    public static void GameGlowAt_Postfix(ref float __result, Map ___map)
    {
        if (__result < 0.5f && intenseShadowMaps.Contains(___map)) __result = 0f;
    }

    [HarmonyPatch(typeof(GenCelestial), nameof(GenCelestial.CurShadowStrength))]
    [HarmonyPostfix]
    public static void CurShadowStrength_Postfix(Map map, ref float __result)
    {
        if (intenseShadowMaps.Contains(map)) __result = 5f;
    }
}
