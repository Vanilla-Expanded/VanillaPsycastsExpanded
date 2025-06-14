namespace VanillaPsycastsExpanded;

using Verse;

public class Hediff_NoMerge : HediffWithComps
{
    public override bool TryMergeWith(Hediff other) => false;
}