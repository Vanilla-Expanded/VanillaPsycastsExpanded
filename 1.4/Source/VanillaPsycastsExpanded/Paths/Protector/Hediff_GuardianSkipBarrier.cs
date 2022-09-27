namespace VanillaPsycastsExpanded;

using UnityEngine;
using Verse;
using Verse.Sound;

public class Hediff_GuardianSkipBarrier : Hediff_Overshield
{
    private         Sustainer sustainer;
    public override Color     OverlayColor => new ColorInt(79, 141, 247).ToColor;
    public override float     OverlaySize  => 9;

    protected override void DestroyProjectile(Projectile projectile)
    {
        base.DestroyProjectile(projectile);
        this.AddEntropy();
    }

    public override void PostTick()
    {
        base.PostTick();
        this.AddEntropy();
        if (this.sustainer == null || this.sustainer.Ended)
            this.sustainer = VPE_DefOf.VPE_GuardianSkipbarrier_Sustainer.TrySpawnSustainer(SoundInfo.InMap(this.pawn, MaintenanceType.PerTick));
        this.sustainer.Maintain();
    }

    public override void PostRemoved()
    {
        base.PostRemoved();
        if (!this.sustainer.Ended) this.sustainer?.End();
    }

    private void AddEntropy()
    {
        if (Find.TickManager.TicksGame % 10       == 0) this.pawn.psychicEntropy.TryAddEntropy(1f, overLimit: true);
        if (this.pawn.psychicEntropy.EntropyValue >= this.pawn.psychicEntropy.MaxEntropy) this.pawn.health.RemoveHediff(this);
    }
}