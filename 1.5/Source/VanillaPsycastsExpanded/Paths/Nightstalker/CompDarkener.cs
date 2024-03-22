using System;
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
    private static readonly Type glowGridLight;
    private static readonly AccessTools.FieldRef<object, CompGlower> lightGlower;

    static CompDarkener()
    {
        glowGridLight = AccessTools.Inner(typeof(GlowGrid), "Light");
        lightGlower = AccessTools.FieldRefAccess<CompGlower>(glowGridLight, "glower");
    }

    [HarmonyPatch(typeof(GlowGrid), "CombineColors")]
    [HarmonyPrefix]
    public static bool CombineColorsDark(ref Color32 __result, Color32 existingSum, Color32 toAdd, CompGlower toAddGlower)
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
}

public class CompProperties_Darkness : CompProperties_Glower
{
    public CompProperties_Darkness() => compClass = typeof(CompDarkener);
}
