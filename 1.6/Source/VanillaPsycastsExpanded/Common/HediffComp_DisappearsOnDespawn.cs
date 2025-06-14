namespace VanillaPsycastsExpanded
{
    using Verse;
    public class HediffCompProperties_DisappearsOnDespawn : HediffCompProperties
    {
        public HediffCompProperties_DisappearsOnDespawn()
        {
            compClass = typeof(HediffComp_DisappearsOnDespawn);
        }
    }
    public class HediffComp_DisappearsOnDespawn : HediffComp
    {
        public override bool CompShouldRemove => this.Pawn.MapHeld is null;
    }
}
