namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    public class Hediff_Deathshield : Hediff_Overlay
    {
        public override float OverlaySize => 1.5f;
        public override string OverlayPath => "Effects/Necropath/Deathshield/Deathshield";

        public float curAngle;

        public Color? skinColor;
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            if (ModCompatibility.AlienRacesIsActive)
            {
                skinColor = ModCompatibility.GetSkinColorFirst(this.pawn);
                ModCompatibility.SetSkinColorFirst(this.pawn, PawnGraphicSet.RottingColorDefault);
            }
            else
            {
                skinColor = this.pawn.story.skinColorOverride;
                this.pawn.story.skinColorOverride = PawnGraphicSet.RottingColorDefault;
            }
            this.pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (ModCompatibility.AlienRacesIsActive)
            {
                ModCompatibility.SetSkinColorFirst(this.pawn, skinColor.Value);
            }
            else
            {
                this.pawn.story.skinColorOverride = skinColor;
            }
            this.pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }
        public override void Tick()
        {
            base.Tick();
            curAngle += 0.07f;
            if (curAngle > 360)
            {
                curAngle = 0;
            }
        }
        public override void Draw()
        {
            if (pawn.Spawned)
            {
                Vector3 pos = pawn.DrawPos;
                pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(pos, Quaternion.AngleAxis(curAngle, Vector3.up), new Vector3(OverlaySize, 1f, OverlaySize));
                UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, OverlayMat, 0, null, 0, MatPropertyBlock);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref curAngle, "curAngle");
        }
    }
}