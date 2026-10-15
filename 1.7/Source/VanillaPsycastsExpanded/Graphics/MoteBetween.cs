using UnityEngine;
using Verse;

namespace VanillaPsycastsExpanded;

public class MoteBetween : Mote
{
    protected MoteAttachLink link2 = MoteAttachLink.Invalid;

    public float LifetimeFraction => AgeSecs / def.mote.Lifespan;

    public void Attach(TargetInfo a, TargetInfo b)
    {
        link1 = new(a, Vector3.zero);
        link2 = new(b, Vector3.zero);
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        UpdatePositionAndRotation(ref drawLoc);
        base.DrawAt(drawLoc, flip);
    }

    protected void UpdatePositionAndRotation(ref Vector3 drawPos)
    {
        if (link1.Linked && link2.Linked)
        {
            if (!link1.Target.ThingDestroyed) link1.UpdateDrawPos();

            if (!link2.Target.ThingDestroyed) link2.UpdateDrawPos();

            var a = link1.LastDrawPos;
            var b = link2.LastDrawPos;

            exactPosition = a + (b - a) * LifetimeFraction;

            if (def.mote.rotateTowardsTarget) exactRotation = a.AngleToFlat(b) + 90f;
        }

        exactPosition.y = def.altitudeLayer.AltitudeFor();

        drawPos = exactPosition;
    }
}
