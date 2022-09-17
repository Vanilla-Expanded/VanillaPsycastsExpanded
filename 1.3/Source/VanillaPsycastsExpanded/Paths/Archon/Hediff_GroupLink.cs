namespace VanillaPsycastsExpanded
{
    using RimWorld.Planet;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    using Verse.Sound;
    using VFECore.Abilities;
    public class Hediff_Thrall : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 60 == 0)
            {

            }
        }
    }
    public class Hediff_GroupLink : Hediff_Overlay
	{
        public override string OverlayPath => "Other/ForceField";
        public virtual Color OverlayColor => new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.5f);
        public override float OverlaySize => this.ability.GetRadiusForPawn();

        public List<Pawn> linkedPawns = new List<Pawn>();
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            LinkAllPawnsAround();
        }
        public void LinkAllPawnsAround()
        {
            foreach (var pawnToLink in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, this.ability.GetRadiusForPawn(), true)
                .OfType<Pawn>().Where(x => x.RaceProps.Humanlike && x != pawn))
            {
                if (!linkedPawns.Contains(pawnToLink))
                {
                    linkedPawns.Add(pawnToLink);
                }
            }
        }

        private void UnlinkAll()
        {
            for (var i = linkedPawns.Count - 1; i >= 0; i--)
            {
                linkedPawns.RemoveAt(i);
            }
        }
        public override void PostRemoved()
        {
            base.PostRemoved();
            UnlinkAll();
        }
        public override void Tick()
        {
            base.Tick();
            for (var i = linkedPawns.Count - 1; i >= 0; i--)
            {
                var linkedPawn = linkedPawns[i];
                if (linkedPawn.Map != this.pawn.Map || linkedPawn.Position.DistanceTo(pawn.Position) > this.ability.GetRadiusForPawn())
                {
                    linkedPawns.RemoveAt(i);
                }
            }
            if (!linkedPawns.Any())
            {
                this.pawn.health.RemoveHediff(this);
            }
        }

        public override void Draw()
        {
            Vector3 pos = pawn.DrawPos;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Color value = OverlayColor;
            MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, Quaternion.identity, new Vector3(OverlaySize * 2f * 1.16015625f, 1f, OverlaySize * 2f * 1.16015625f));
            UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, OverlayMat, 0, null, 0, MatPropertyBlock);
            foreach (var linked in linkedPawns)
            {
                GenDraw.DrawLineBetween(linked.DrawPos, this.pawn.DrawPos, SimpleColor.Yellow);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref linkedPawns, "linkedPawns", LookMode.Reference);
        }
    }
}