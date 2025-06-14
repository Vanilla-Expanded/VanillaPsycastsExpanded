namespace VanillaPsycastsExpanded;

using Verse;

public interface IMinHeatGiver : ILoadReferenceable
{
    public bool IsActive { get; }
    public int  MinHeat  { get; }
}