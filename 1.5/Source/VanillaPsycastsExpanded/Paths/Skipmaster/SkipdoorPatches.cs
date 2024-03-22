namespace VanillaPsycastsExpanded.Skipmaster;

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore;

[HarmonyPatch]
public static class SkipdoorPatches
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    [HarmonyPrefix]
    public static void Pawn_Kill_Prefix(Pawn __instance)
    {
        if (__instance.Faction is { IsPlayer: true })
            foreach (Skipdoor skipdoor in WorldComponent_DoorTeleporterManager.Instance.DoorTeleporters.OfType<Skipdoor>().ToList())
                if (skipdoor.Pawn == __instance)
                {
                    GenExplosion.DoExplosion(skipdoor.Position, skipdoor.Map, 4.9f, DamageDefOf.Bomb, skipdoor, 35);
                    if (!skipdoor.Destroyed) skipdoor.Destroy();
                }
    }
}