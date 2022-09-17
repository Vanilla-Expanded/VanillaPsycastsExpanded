namespace VanillaPsycastsExpanded.Nightstalker
{
    using System.Collections.Generic;
    using System.Linq;
    using HarmonyLib;
    using RimWorld;
    using UnityEngine;
    using Verse;

    [HarmonyPatch]
    [StaticConstructorOnStartup]
    public class CompDarkener : CompGlower
    {
        private static readonly HashSet<IntVec3> DarkenedCells = new();

        private IntVec3 position;
        private int     spawnedTick;

        private IEnumerable<IntVec3> AffectedCells => GenRadial.RadialCellsAround(this.position, this.Props.glowRadius, true);

        [HarmonyPatch(typeof(GlowGrid), "RecalculateAllGlow")]
        [HarmonyPostfix]
        public static void RecalculateAllGlow_Postfix(List<CompGlower> ___litGlowers, Color32[] ___glowGrid, Color32[] ___glowGridNoCavePlants)
        {
            foreach (CompDarkener darkener in ___litGlowers.OfType<CompDarkener>())
            foreach (IntVec3 c in darkener.AffectedCells)
            {
                int ind                                         = darkener.parent.Map.cellIndices.CellToIndex(c);
                ___glowGrid[ind] = ___glowGridNoCavePlants[ind] = new Color32(0, 0, 0, 1);
            }
        }

        [HarmonyPatch(typeof(GlowGrid), nameof(GlowGrid.GameGlowAt))]
        [HarmonyPrefix]
        public static bool GameGlowAt_Prefix(IntVec3 c, ref float __result)
        {
            if (DarkenedCells.Contains(c))
            {
                __result = 0f;
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(Projectile))]
        [HarmonyPatch(nameof(Projectile.Launch), typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags),
                      typeof(bool), typeof(Thing), typeof(ThingDef))]
        [HarmonyPrefix]
        public static void Launch_Prefix(Thing launcher, LocalTargetInfo intendedTarget, ref LocalTargetInfo usedTarget)
        {
            if (intendedTarget == usedTarget && DarkenedCells.Any())
            {
                ShootLine shootLine = new(launcher.Position, intendedTarget.Cell);
                if (shootLine.Points().Any(c => DarkenedCells.Contains(c)))
                {
                    shootLine.ChangeDestToMissWild(0f);
                    usedTarget = shootLine.Dest;
                }
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.position = this.parent.Position;
            DarkenedCells.UnionWith(this.AffectedCells);
            if (!respawningAfterLoad) this.spawnedTick = Find.TickManager.TicksGame;
        }

        public override void PostDeSpawn(Map map)
        {
            HashSet<IntVec3> toRemove = this.AffectedCells.ToHashSet();
            DarkenedCells.RemoveWhere(c => toRemove.Contains(c));
            base.PostDeSpawn(map);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.spawnedTick, nameof(this.spawnedTick));
        }
    }

    public class CompProperties_Darkness : CompProperties_Glower
    {
        public CompProperties_Darkness() => this.compClass = typeof(CompDarkener);
    }
}