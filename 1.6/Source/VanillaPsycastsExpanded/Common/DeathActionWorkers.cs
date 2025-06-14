using RimWorld;
using Verse;
using Verse.AI.Group;

namespace VanillaPsycastsExpanded;

public class DeathActionWorker_SlagChunk : DeathActionWorker
{
    public override void PawnDied(Corpse corpse, Lord prevLord)
    {
        if (corpse.Map != null)
        {
            var chunk = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel);
            GenSpawn.Spawn(chunk, corpse.Position, corpse.Map);
            corpse.Destroy();
        }
    }
}

public class DeathActionWorker_RockChunk : DeathActionWorker
{
    public override void PawnDied(Corpse corpse, Lord prevLord)
    {
        if (corpse.Map != null && corpse.InnerPawn?.TryGetComp<CompSetStoneColour>()?.KilledLeave is { } def)
        {
            var chunk = ThingMaker.MakeThing(def);
            GenSpawn.Spawn(chunk, corpse.Position, corpse.Map);
            corpse.Destroy();
        }
    }
}
