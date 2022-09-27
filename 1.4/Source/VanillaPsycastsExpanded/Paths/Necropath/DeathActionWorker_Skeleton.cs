namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using Verse;

    public class DeathActionWorker_Skeleton : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse)
        {
            FilthMaker.TryMakeFilth(corpse.Position, corpse.Map, ThingDefOf.Filth_CorpseBile, 3);
            corpse.Destroy();
        }
    }
}