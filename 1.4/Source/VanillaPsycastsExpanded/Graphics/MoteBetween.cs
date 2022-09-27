namespace VanillaPsycastsExpanded
{
    using UnityEngine;
    using Verse;

    public class MoteBetween : Mote
    {
        protected MoteAttachLink link2 = MoteAttachLink.Invalid;

        public float LifetimeFraction => this.AgeSecs / this.def.mote.Lifespan;

        public void Attach(TargetInfo a, TargetInfo b)
        {
            this.link1 = new MoteAttachLink(a, Vector3.zero);
            this.link2 = new MoteAttachLink(b, Vector3.zero);
        }

        public override void Draw()
        {
            this.UpdatePositionAndRotation();
            base.Draw();
        }

        protected void UpdatePositionAndRotation()
        {
            if (this.link1.Linked && this.link2.Linked)
            {
                if (!this.link1.Target.ThingDestroyed) this.link1.UpdateDrawPos();

                if (!this.link2.Target.ThingDestroyed) this.link2.UpdateDrawPos();

                Vector3 a = this.link1.LastDrawPos;
                Vector3 b = this.link2.LastDrawPos;

                this.exactPosition = a + (b - a) * this.LifetimeFraction;

                if (this.def.mote.rotateTowardsTarget) this.exactRotation = a.AngleToFlat(b) + 90f;
            }

            this.exactPosition.y = this.def.altitudeLayer.AltitudeFor();
        }
    }
}