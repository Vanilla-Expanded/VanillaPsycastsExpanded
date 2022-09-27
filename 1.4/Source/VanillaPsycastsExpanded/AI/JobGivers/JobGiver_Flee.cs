using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace VanillaPsycastsExpanded
{
	public class JobGiver_Flee : ThinkNode_JobGiver
	{
        protected override Job TryGiveJob(Pawn pawn)
        {
            var enemies = pawn.Map.mapPawns.AllPawnsSpawned.Where(x => !x.Dead && !x.Downed && x.Position.DistanceTo(pawn.Position) < 50f
                && GenSight.LineOfSight(x.Position, pawn.Position, pawn.Map)).OrderBy(x => x.Position.DistanceTo(pawn.Position)).ToList();
            if (enemies.Any())
            {
                if (pawn.Faction != Faction.OfPlayer && CellFinderLoose.GetFleeExitPosition(pawn, 10f, out var pos))
                {
                    Job job = JobMaker.MakeJob(JobDefOf.Flee, pos, enemies.First());
                    job.exitMapOnArrival = true;
                    return job;
                }
                return FleeJob(pawn, enemies.First(), enemies.Cast<Thing>().ToList());
            }
            return null;
        }

        public Job FleeJob(Pawn pawn, Thing danger, List<Thing> dangers)
        {
            Job job = null;
            IntVec3 intVec;
            if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Flee)
            {
                intVec = pawn.CurJob.targetA.Cell;
            }
            else
            {
                intVec = CellFinderLoose.GetFleeDest(pawn, dangers, 24f);
            }

            if (intVec == pawn.Position)
            {
                intVec = GenRadial.RadialCellsAround(pawn.Position, 1, 15).RandomElement();
            }
            if (intVec != pawn.Position)
            {
                job = JobMaker.MakeJob(JobDefOf.Flee, intVec, danger);
            }
            return job;
        }
    }
}
