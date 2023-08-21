using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using VanillaPsycastsExpanded.Graphics;
using Verse;
using Verse.Sound;

namespace VanillaPsycastsExpanded;

[StaticConstructorOnStartup]
public class Projectile_FrostRay : Projectile
{
    private static readonly Material shadowMaterial = MaterialPool.MatFrom("Things/Skyfaller/SkyfallerShadowCircle", ShaderDatabase.Transparent);

    public static Func<Projectile, float> ArcHeightFactor = (Func<Projectile, float>)
        Delegate.CreateDelegate(typeof(Func<Projectile, float>), null, AccessTools.Method(typeof(Projectile), "get_ArcHeightFactor"));

    private Sustainer sustainer;

    public override void Draw()
    {
        var num = ArcHeightFactor(this) * GenMath.InverseParabola(DistanceCoveredFraction);
        var distanceSize = Vector3.Distance(origin.Yto0(), DrawPos.Yto0());
        var drawPos = Vector3.Lerp(origin, DrawPos, 0.5f);
        drawPos.y += 5f;
        var position = drawPos + new Vector3(0f, 0f, 1f) * num;
        if (def.projectile.shadowSize > 0f) DrawShadow(drawPos, num);
        Comps_PostDraw();
        UnityEngine.Graphics.DrawMesh(MeshPool.GridPlane(new Vector2(5f, distanceSize)), position, ExactRotation, (Graphic as Graphic_Animated).MatSingle, 0);
    }

    public override void Tick()
    {
        base.Tick();
        if (sustainer == null || sustainer.Ended)
            sustainer = VPE_DefOf.VPE_FrostRay_Sustainer.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
        sustainer.Maintain();
        if (launcher is Pawn pawn)
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_FrostRay);
            if (hediff is null || pawn.Downed || pawn.Dead)
            {
                Destroy();
                return;
            }
        }

        if (this.IsHashIntervalTick(10))
        {
            var resultingLine = new ShootLine(origin.ToIntVec3(), DrawPos.ToIntVec3());
            var cells = resultingLine.Points().Where(x => x != resultingLine.Source);
            var pawns = new HashSet<Pawn>();
            foreach (var cell in cells)
            foreach (var thing in cell.GetThingList(Map).OfType<Pawn>())
                pawns.Add(thing);
            foreach (var victim in pawns)
            {
                var battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, victim,
                    intendedTarget.Thing, launcher.def, def, targetCoverDef);
                Find.BattleLog.Add(battleLogEntry_RangedImpact);
                var dinfo = new DamageInfo(def.projectile.damageDef, DamageAmount, ArmorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef,
                    DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
                victim.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                if (victim.CanReceiveHypothermia(out var hediff))
                {
                    HealthUtility.AdjustSeverity(victim, hediff, 0.08f / 6f);
                }
                HealthUtility.AdjustSeverity(victim, VPE_DefOf.VFEP_HypothermicSlowdown, 0.08f / 6f);
            }
        }
    }

    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        // we skip it
    }

    private void DrawShadow(Vector3 drawLoc, float height)
    {
        if (!(shadowMaterial == null))
        {
            var num = def.projectile.shadowSize * Mathf.Lerp(1f, 0.6f, height);
            var s = new Vector3(num, 1f, num);
            var b = new Vector3(0f, -0.01f, 0f);
            var matrix = default(Matrix4x4);
            matrix.SetTRS(drawLoc + b, Quaternion.identity, s);
            UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, shadowMaterial, 0);
        }
    }
}