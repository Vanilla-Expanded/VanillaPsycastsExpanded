namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using Verse;

    public class DeathActionWorker_SlagChunk : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse)
        {
            if (corpse.Map != null)
            {
                Thing chunk = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel);
                GenSpawn.Spawn(chunk, corpse.Position, corpse.Map);
                corpse.Destroy();
            }
        }
    }

    public class DeathActionWorker_RockChunk : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse)
        {
            if (corpse.Map != null && corpse.InnerPawn?.TryGetComp<CompSetStoneColour>()?.KilledLeave is { } def)
            {
                Thing chunk = ThingMaker.MakeThing(def);
                GenSpawn.Spawn(chunk, corpse.Position, corpse.Map);
                corpse.Destroy();
            }
        }
    }
}