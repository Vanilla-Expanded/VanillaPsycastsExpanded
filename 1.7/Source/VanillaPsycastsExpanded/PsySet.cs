using System.Collections.Generic;
using Verse;
using VEF.Abilities;

namespace VanillaPsycastsExpanded;

public class PsySet : IExposable, IRenameable
{
    public HashSet<AbilityDef> Abilities = new();
    public string Name;

    public void ExposeData()
    {
        Scribe_Values.Look(ref Name, "name");
        Scribe_Collections.Look(ref Abilities, "abilities");
    }

    public string RenamableLabel
    {
        get => Name;
        set => Name = value;
    }

    public string BaseLabel => Name;

    public string InspectLabel => Name;
}
