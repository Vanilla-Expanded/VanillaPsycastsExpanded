namespace VanillaPsycastsExpanded
{
    using Verse;
    public class HediffCompProperties_DisappearsOnDowned : HediffCompProperties
    {
        public HediffCompProperties_DisappearsOnDowned()
        {
            compClass = typeof(HediffComp_DisappearsOnDowned);
        }
    }
    public class HediffComp_DisappearsOnDowned : HediffComp
    {
        public override bool CompShouldRemove => this.Pawn.Downed;
    }
}
