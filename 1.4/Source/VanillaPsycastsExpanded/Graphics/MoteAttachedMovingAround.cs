namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using System;
    using UnityEngine;
    using Verse;

    public class MoteAttachedMovingAround : MoteAttached
    {
        private float direction;
        private Vector3 curPosition;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                curPosition = new Vector3(Rand.Range(-0.5f, 0.5f), 0, Rand.Range(-0.5f, 0.5f));
                exactPosition = GetRootPosition() + curPosition;
                exactPosition.y = link1.Target.CenterVector3.y + 1;
            }
        }
        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);
            curPosition = GetNewMoveVector();
            var rootPosition = GetRootPosition();
            exactPosition = rootPosition + curPosition;
            exactPosition.y = link1.Target.CenterVector3.y + 1;
        }

        public Vector3 GetNewMoveVector()
        {
            var realPosition = new Vector2(curPosition.x, curPosition.z);
            direction += Rand.Range(-22.5f, 22.5f);
            if (direction < -360)
            {
                direction = Mathf.Abs(direction - -360);
            }
            if (direction > 360)
            {
                direction = direction - 360;
            }
            var newPosition = realPosition.Moved(direction, 0.01f);
            var pos = new Vector3(newPosition.x, 0, newPosition.y);
            pos = Vector3.ClampMagnitude(pos, 0.5f);
            return pos;
        }
        public Vector3 GetRootPosition()
        {
            Vector3 vector = def.mote.attachedDrawOffset;
            if (def.mote.attachedToHead)
            {
                Pawn pawn = link1.Target.Thing as Pawn;
                if (pawn != null && pawn.story != null)
                {
                    vector = pawn.Drawer.renderer.BaseHeadOffsetAt((pawn.GetPosture() == PawnPosture.Standing) ? Rot4.North : pawn.Drawer.renderer.LayingFacing()).RotatedBy(pawn.Drawer.renderer.BodyAngle());
                }
            }
            return link1.LastDrawPos + vector;
        }
    }
}