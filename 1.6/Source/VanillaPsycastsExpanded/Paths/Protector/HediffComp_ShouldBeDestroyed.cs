namespace VanillaPsycastsExpanded
{
    using Verse;

    public class HediffComp_ShouldBeDestroyed : HediffComp
    {
        public override bool CompShouldRemove => true;
    }
}