using RimWorld;
using Verse;
using Verse.AI.Group;

namespace VanillaPsycastsExpanded;

public class DeathActionWorker_Skeleton : DeathActionWorker
{
    public override void PawnDied(Corpse corpse, Lord prevLord)
    {
        FilthMaker.TryMakeFilth(corpse.Position, corpse.Map, ThingDefOf.Filth_CorpseBile, 3);
        corpse.Destroy();
    }
}
