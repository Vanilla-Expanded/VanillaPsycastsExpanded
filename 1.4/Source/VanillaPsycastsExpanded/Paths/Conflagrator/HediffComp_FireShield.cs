namespace VanillaPsycastsExpanded.Conflagrator
{
    using RimWorld;
    using Verse;
    using VFECore.Shields;

    public class HediffComp_FireShield : HediffComp_Shield
    {
        protected override void ApplyDamage(DamageInfo dinfo)
        {
            dinfo.Instigator.TryAttachFire(25f);
        }
    }
}