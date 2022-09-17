namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using VanillaPsycastsExpanded.Graphics;
    using Verse;
    using Verse.Sound;
    using VFECore.Abilities;

    [StaticConstructorOnStartup]
    public class Projectile_FrostRay : Projectile
    {
        private static readonly Material shadowMaterial = MaterialPool.MatFrom("Things/Skyfaller/SkyfallerShadowCircle", ShaderDatabase.Transparent);

        public static Func<Projectile, float> ArcHeightFactor = (Func<Projectile, float>)
            Delegate.CreateDelegate(typeof(Func<Projectile, float>), null, AccessTools.Method(typeof(Projectile), "get_ArcHeightFactor"));
        public override void Draw()
        {
            float num = ArcHeightFactor(this) * GenMath.InverseParabola(DistanceCoveredFraction);
            var distanceSize = Vector3.Distance(origin.Yto0(), DrawPos.Yto0());
            Vector3 drawPos = Vector3.Lerp(origin, DrawPos, 0.5f);
            drawPos.y += 5f;
            Vector3 position = drawPos + new Vector3(0f, 0f, 1f) * num;
            if (def.projectile.shadowSize > 0f)
            {
                DrawShadow(drawPos, num);
            }
            Comps_PostDraw();
            UnityEngine.Graphics.DrawMesh(MeshPool.GridPlane(new Vector2(5f, distanceSize)), position, ExactRotation, (this.Graphic as Graphic_Animated).MatSingle, 0);
        }

        private Sustainer sustainer;
        public override void Tick()
        {
            base.Tick();
            if (sustainer == null || sustainer.Ended)
            {
                sustainer = VPE_DefOf.VPE_FrostRay_Sustainer.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
            }
            sustainer.Maintain();
            if (this.launcher is Pawn pawn)
            {
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_FrostRay);
                if (hediff is null || pawn.Downed || pawn.Dead)
                {
                    this.Destroy();
                    return;
                }
            }
            if (this.IsHashIntervalTick(10))
            {
                var resultingLine = new ShootLine(origin.ToIntVec3(), DrawPos.ToIntVec3());
                var cells = resultingLine.Points().Where(x => x != resultingLine.Source);
                var pawns = new HashSet<Pawn>();
                foreach (var cell in cells)
                {
                    foreach (var thing in cell.GetThingList(Map).OfType<Pawn>())
                    {
                        pawns.Add(thing);
                    }
                }
                foreach (var victim in pawns)
                {
                    BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, victim, 
                        intendedTarget.Thing, launcher.def, def, targetCoverDef);
                    Find.BattleLog.Add(battleLogEntry_RangedImpact);
                    DamageInfo dinfo = new DamageInfo(def.projectile.damageDef, base.DamageAmount, base.ArmorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
                    victim.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                    HealthUtility.AdjustSeverity(victim, HediffDefOf.Hypothermia, 0.016f);
                    HealthUtility.AdjustSeverity(victim, VPE_DefOf.VFEP_HypothermicSlowdown, 0.016f);
                }
            }
        }

        protected override void Impact(Thing hitThing)
        {
            // we skip it
        }

        private void DrawShadow(Vector3 drawLoc, float height)
        {
            if (!(shadowMaterial == null))
            {
                float num = def.projectile.shadowSize * Mathf.Lerp(1f, 0.6f, height);
                Vector3 s = new Vector3(num, 1f, num);
                Vector3 b = new Vector3(0f, -0.01f, 0f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawLoc + b, Quaternion.identity, s);
                UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, shadowMaterial, 0);
            }
        }
    }
}