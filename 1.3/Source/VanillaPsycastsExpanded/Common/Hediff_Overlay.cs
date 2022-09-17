namespace VanillaPsycastsExpanded
{
    using UnityEngine;
    using Verse;
    using VFECore.Abilities;

    [StaticConstructorOnStartup]
    public abstract class Hediff_Overlay : Hediff_Ability
    {
        public MaterialPropertyBlock MatPropertyBlock = new();

        private Material material;
        public Material OverlayMat
        {
            get
            {
                if (this.material == null) this.material = MaterialPool.MatFrom(this.OverlayPath, ShaderDatabase.MoteGlow);
                return this.material;
            }
        }
        public virtual float  OverlaySize => 1f;
        public virtual string OverlayPath { get; }
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            this.pawn.MapHeld?.GetComponent<MapComponent_PsycastsManager>().hediffsToDraw.Add(this);
        }
        public virtual void Draw()
        {
        }
    }
}