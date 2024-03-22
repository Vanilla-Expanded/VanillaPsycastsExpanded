namespace VanillaPsycastsExpanded.Wildspeaker
{
    using RimWorld;
    using Verse;
    using Verse.AI;

    public class MentalState_ManhunterTerritorial : MentalState_Manhunter
    {
        public override bool ForceHostileTo(Faction f) => f.HostileTo(Faction.OfPlayer);

        public override bool ForceHostileTo(Thing t) => t.HostileTo(Faction.OfPlayer);
    }
}