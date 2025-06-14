namespace VanillaPsycastsExpanded.Graphics
{
    using UnityEngine;
    using Verse;

    public class Projectile_Pointing : Projectile_Explosive
    {
        private Vector3 LookTowards =>
            new(this.destination.x - this.origin.x, this.def.Altitude, this.destination.z - this.origin.z +
                                                                       this.ArcHeightFactor * (4 - 8 * this.DistanceCoveredFraction));

        private float ArcHeightFactor
        {
            get
            {
                float num                               = this.def.projectile.arcHeightFactor;
                float num2                              = (this.destination - this.origin).MagnitudeHorizontalSquared();
                if (num * num > num2 * 0.2f * 0.2f) num = Mathf.Sqrt(num2) * 0.2f;

                return num;
            }
        }

        public override Quaternion ExactRotation => Quaternion.LookRotation(this.LookTowards);
    }
}