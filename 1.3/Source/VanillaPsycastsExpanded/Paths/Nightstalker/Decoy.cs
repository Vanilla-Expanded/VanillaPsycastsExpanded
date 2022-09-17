namespace VanillaPsycastsExpanded.Nightstalker;

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using VFECore.Abilities;

[HarmonyPatch]
public class Decoy : ThingWithComps, IAttackTarget
{
    private static readonly HashSet<Map> mapsWithDecoys = new();
    private                 Pawn         pawn;

    public bool ThreatDisabled(IAttackTargetSearcher disabledFor) => false;

    public Thing           Thing                   => this;
    public LocalTargetInfo TargetCurrentlyAimingAt => LocalTargetInfo.Invalid;
    public float           TargetPriorityFactor    => float.MaxValue;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        this.pawn = this.GetComp<CompAbilitySpawn>().pawn;
        mapsWithDecoys.Add(map);
        base.SpawnSetup(map, respawningAfterLoad);
        foreach (IAttackTarget target in this.pawn.Map.attackTargetsCache.TargetsHostileToFaction(this.pawn.Faction))
            if (target.Thing is Pawn { CurJobDef: var jobDef } targetPawn && (jobDef == JobDefOf.Wait_Combat || jobDef == JobDefOf.Goto))
                targetPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        if (!this.Map.listerThings.AllThings.OfType<Decoy>().Except(this).Any()) mapsWithDecoys.Remove(this.Map);
        base.DeSpawn(mode);
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        DecoyOverlayUtility.DrawOverlay = true;
        this.pawn.Drawer.DrawAt(drawLoc);
        DecoyOverlayUtility.DrawOverlay = false;
    }

    [HarmonyPatch(typeof(DamageFlasher), "GetDamagedMat")]
    [HarmonyPrefix]
    private static void GetDuplicateMat(ref Material baseMat)
    {
        if (DecoyOverlayUtility.DrawOverlay) baseMat = DecoyOverlayUtility.GetDuplicateMat(baseMat);
    }

    [HarmonyPatch(typeof(AttackTargetFinder), nameof(AttackTargetFinder.BestAttackTarget))]
    [HarmonyPrefix]
    public static bool BestAttackTarget_Prefix(IAttackTargetSearcher searcher, ref IAttackTarget __result)
    {
        if (!mapsWithDecoys.Contains(searcher.Thing.MapHeld)) return true;
        List<IAttackTarget> list1 = searcher.Thing.Map.attackTargetsCache.GetPotentialTargetsFor(searcher);
        List<Decoy>         list2 = list1.OfType<Decoy>().ToList();
        if (list2.NullOrEmpty()) return true;
        __result = list2.RandomElement();
        return false;
    }

    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "UpdateEnemyTarget")]
    [HarmonyPrefix]
    public static void UpdateEnemyTarget_Prefix(Pawn pawn)
    {
        if (pawn.mindState.enemyTarget is { } and not Decoy && mapsWithDecoys.Contains(pawn.MapHeld)) pawn.mindState.enemyTarget = null;
    }
}

public static class DecoyOverlayUtility
{
    [TweakValue("00", 0f, 1f)] public static float ColorR = 0f;
    [TweakValue("00", 0f, 1f)] public static float ColorG = 0f;
    [TweakValue("00", 0f, 1f)] public static float ColorB = 0f;
    [TweakValue("00", 0f, 1f)] public static float ColorA = 1f;

    private static readonly Dictionary<Material, Material> Materials = new();

    public static bool  DrawOverlay;
    public static Color OverlayColor => new(ColorR, ColorG, ColorB, ColorA);

    public static Material GetDuplicateMat(Material baseMat)
    {
        if (!Materials.TryGetValue(baseMat, out Material value))
        {
            value       = MaterialAllocator.Create(baseMat);
            value.color = OverlayColor;
            if (Materials.ContainsKey(baseMat))
                Materials[baseMat] = value;
            else
                Materials.Add(baseMat, value);
        }

        return value;
    }
}