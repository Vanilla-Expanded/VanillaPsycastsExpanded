using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaPsycastsExpanded.Nightstalker;

[HarmonyPatch]
[StaticConstructorOnStartup]
public class CompDarkener : CompGlower
{
    private static readonly AccessTools.FieldRef<object, CompGlower> lightGlower;
    private static readonly Dictionary<Map, HashSet<IntVec3>> darkCells = new();

    static CompDarkener()
    {
        var glowGridLight = AccessTools.Inner(typeof(GlowGrid), "Light");
        lightGlower = AccessTools.FieldRefAccess<CompGlower>(glowGridLight, "glower");
    }

    protected override bool ShouldBeLitNow => true;

    [HarmonyPatch(typeof(GlowGrid), "CombineColors")]
    [HarmonyPrefix]
    public static bool CombineColorsDark(ref Color32 __result, CompGlower toAddGlower)
    {
        if (toAddGlower is CompDarkener)
        {
            __result = new(0, 0, 0, 0);
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(GlowGrid), nameof(GlowGrid.RegisterGlower))]
    [HarmonyPostfix]
    public static void MoveDarkLast(List<object> ___lights)
    {
        var darkeners = new List<object>();
        for (var i = ___lights.Count; i-- > 0;)
        {
            var light = ___lights[i];
            if (lightGlower(light) is CompDarkener)
            {
                darkeners.Add(light);
                ___lights.RemoveAt(i);
            }
        }

        ___lights.AddRange(darkeners);
    }

    [HarmonyPatch(typeof(GlowGrid), nameof(GlowGrid.GroundGlowAt))]
    [HarmonyPrefix]
    public static void IgnoreSkyDark(IntVec3 c, ref bool ignoreSky, Map ___map)
    {
        if (darkCells.TryGetValue(___map, out var set) && set.Contains(c)) ignoreSky = true;
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        RecacheDarkCells(parent.Map);
    }

    public override void PostDeSpawn(Map map)
    {
        base.PostDeSpawn(map);
        RecacheDarkCells(map);
    }

    private static void RecacheDarkCells(Map map)
    {
        if (!darkCells.TryGetValue(map, out var set)) set = new();
        foreach (var thing in map.listerThings.AllThings)
            if (thing.TryGetComp<CompGlower>() is CompDarkener darkener)
                foreach (var cell in GenRadial.RadialCellsAround(thing.Position, darkener.GlowRadius, true))
                    set.Add(cell);
        if (set.Count == 0)
            darkCells.Remove(map);
        else
            darkCells[map] = set;
    }
}

public class CompProperties_Darkness : CompProperties_Glower
{
    public CompProperties_Darkness() => compClass = typeof(CompDarkener);
}
