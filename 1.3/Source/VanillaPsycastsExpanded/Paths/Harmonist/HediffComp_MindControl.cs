namespace VanillaPsycastsExpanded.Harmonist
{
    using System.Linq;
    using RimWorld;
    using Verse;
    using Verse.AI.Group;

    public class HediffComp_MindControl : HediffComp
    {
        private Faction oldFaction;
        private Lord    oldLord;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            this.oldFaction = this.Pawn.Faction;
            this.oldLord    = this.Pawn.GetLord();
            this.oldLord?.RemovePawn(this.Pawn);
            this.Pawn.SetFaction(Faction.OfPlayer);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            this.Pawn.SetFaction(this.oldFaction);
            if (this.oldLord is not {AnyActivePawn: true})
            {
                if (this.Pawn.Map.mapPawns.SpawnedPawnsInFaction(this.oldFaction).Except(this.Pawn).Any())
                    this.oldLord = ((Pawn) GenClosest.ClosestThing_Global(this.Pawn.Position, this.Pawn.Map.mapPawns.SpawnedPawnsInFaction(this.oldFaction),
                                                                          99999f,
                                                                          p => p != this.Pawn && ((Pawn) p).GetLord() != null)).GetLord();

                if (this.oldLord == null)
                {
                    LordJob_DefendPoint lordJob = new(this.Pawn.Position);
                    this.oldLord = LordMaker.MakeNewLord(this.oldFaction, lordJob, Find.CurrentMap);
                }
            }

            this.oldLord?.AddPawn(this.Pawn);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref this.oldFaction, nameof(this.oldFaction));
            Scribe_References.Look(ref this.oldLord,    nameof(this.oldLord));
        }
    }
}