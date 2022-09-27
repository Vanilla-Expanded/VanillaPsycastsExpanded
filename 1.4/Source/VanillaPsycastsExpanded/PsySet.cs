namespace VanillaPsycastsExpanded
{
    using System.Collections.Generic;
    using Verse;
    using VFECore.Abilities;

    public class PsySet : IExposable
    {
        public string Name;

        public HashSet<AbilityDef> Abilities = new();

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.Name, "name");
            Scribe_Collections.Look(ref this.Abilities, "abilities");
        }
    }
}