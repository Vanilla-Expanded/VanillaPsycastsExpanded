namespace VanillaPsycastsExpanded
{
    using UnityEngine;
    using Verse;
    public class MoteAttachedScaled : MoteAttached
    {
        public float maxScale;
        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);
            if (!Destroyed)
            {
                if (def.mote.growthRate != 0f)
                {
                    exactScale = new Vector3(exactScale.x + def.mote.growthRate * deltaTime, exactScale.y, exactScale.z + def.mote.growthRate * deltaTime);
                    exactScale.x = Mathf.Min(Mathf.Max(exactScale.x, 0.0001f), maxScale);
                    exactScale.z = Mathf.Min(Mathf.Max(exactScale.z, 0.0001f), maxScale);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref maxScale, "maxScale");
        }
    }
}