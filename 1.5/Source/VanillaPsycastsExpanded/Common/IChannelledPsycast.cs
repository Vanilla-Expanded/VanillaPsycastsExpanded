namespace VanillaPsycastsExpanded;

using Verse;

public interface IChannelledPsycast : ILoadReferenceable
{
    public bool IsActive { get; }
}