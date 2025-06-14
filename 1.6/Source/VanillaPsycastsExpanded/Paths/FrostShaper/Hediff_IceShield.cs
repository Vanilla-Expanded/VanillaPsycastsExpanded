namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    public class Hediff_IceShield : Hediff_Overlay
    {
        public override float OverlaySize => 1.5f;
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            var coldHediffs = this.pawn.health.hediffSet.hediffs.Where(x => x.def == HediffDefOf.Hypothermia 
                || x.def == VPE_DefOf.VFEP_HypothermicSlowdown || x.def == VPE_DefOf.HypothermicSlowdown).ToList();
            foreach (var coldHediff in coldHediffs)
            {
                this.pawn.health.RemoveHediff(coldHediff);
            }
        }
        public override string OverlayPath => "Effects/Frostshaper/FrostShield/Frostshield";
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            if (dinfo.Instigator is Pawn attacker && Vector3.Distance(attacker.DrawPos, pawn.DrawPos) <= OverlaySize
                && attacker.CanReceiveHypothermia(out var hediff))
            {
                HealthUtility.AdjustSeverity(attacker, hediff, 0.05f);
            }
        }

        public override void Draw()
        {
            Vector3 pos = pawn.DrawPos;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, Quaternion.identity, new Vector3(OverlaySize, 1f, OverlaySize));
            UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, OverlayMat, 0, null, 0, MatPropertyBlock);
        }
    }
}