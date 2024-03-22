using UnityEngine;
using Verse;

namespace VanillaPsycastsExpanded;

public class Hediff_Deathshield : Hediff_Overlay
{
    private static readonly Color RottenColor = new(0.29f, 0.25f, 0.22f);

    public float curAngle;

    public Color? skinColor;
    public override float OverlaySize => 1.5f;
    public override string OverlayPath => "Effects/Necropath/Deathshield/Deathshield";

    public override void PostAdd(DamageInfo? dinfo)
    {
        base.PostAdd(dinfo);
        if (ModCompatibility.AlienRacesIsActive)
        {
            skinColor = ModCompatibility.GetSkinColorFirst(pawn);
            ModCompatibility.SetSkinColorFirst(pawn, RottenColor);
        }
        else
        {
            skinColor = pawn.story.skinColorOverride;
            pawn.story.skinColorOverride = RottenColor;
        }

        pawn.Drawer.renderer.SetAllGraphicsDirty();
    }

    public override void PostRemoved()
    {
        base.PostRemoved();
        if (ModCompatibility.AlienRacesIsActive)
            ModCompatibility.SetSkinColorFirst(pawn, skinColor.Value);
        else
            pawn.story.skinColorOverride = skinColor;
        pawn.Drawer.renderer.SetAllGraphicsDirty();
    }

    public override void Tick()
    {
        base.Tick();
        curAngle += 0.07f;
        if (curAngle > 360) curAngle = 0;
    }

    public override void Draw()
    {
        if (pawn.Spawned)
        {
            var pos = pawn.DrawPos;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            var matrix = default(Matrix4x4);
            matrix.SetTRS(pos, Quaternion.AngleAxis(curAngle, Vector3.up), new(OverlaySize, 1f, OverlaySize));
            UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, OverlayMat, 0, null, 0, MatPropertyBlock);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref curAngle, "curAngle");
    }
}
