namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using System.Linq;
    using UnityEngine;
    using Verse;
    public class Hediff_IceBlock : Hediff_Overlay
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            var facedCell = pawn.Rotation.FacingCell;
            var ticksToStand = this.TryGetComp<HediffComp_Disappears>().ticksToDisappear;
            var job = JobMaker.MakeJob(VPE_DefOf.VPE_StandFreeze);
            job.expiryInterval = ticksToStand;
            job.overrideFacing = pawn.Rotation;
            pawn.jobs.TryTakeOrderedJob(job);
            pawn.pather.StopDead();
            pawn.stances.SetStance(new Stance_Stand(ticksToStand, facedCell, null));
        }
        public override string OverlayPath => "Effects/Frostshaper/IceBlock/IceBlock";
        public override void Draw()
        {
            Vector3 pos = pawn.DrawPos;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Matrix4x4 matrix = default(Matrix4x4);
            var drawSize = 1.5f;
            matrix.SetTRS(pos, Quaternion.identity, new Vector3(drawSize, 1f, drawSize));
            UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, OverlayMat, 0, null, 0, MatPropertyBlock);
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn.IsHashIntervalTick(60) && pawn.CanReceiveHypothermia(out var hediffDef))
            {
                HealthUtility.AdjustSeverity(pawn, hediffDef, 0.05f);
            }
        }
    }
}