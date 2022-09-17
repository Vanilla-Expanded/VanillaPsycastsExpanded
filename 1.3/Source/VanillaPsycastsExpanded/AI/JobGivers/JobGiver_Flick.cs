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
	public class JobGiver_Flick : ThinkNode_JobGiver
	{

		public PathEndMode PathEndMode => PathEndMode.Touch;

		public IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			List<Designation> desList = pawn.Map.designationManager.allDesignations;
			for (int i = 0; i < desList.Count; i++)
			{
				if (desList[i].def == DesignationDefOf.Flick)
				{
					yield return desList[i].target.Thing;
				}
			}
		}

		public bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.Flick);
		}

		public bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Flick) == null)
			{
				return false;
			}
			if (!pawn.CanReserve(t, 1, -1, null, forced))
			{
				return false;
			}
			return true;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{

			if (ShouldSkip(pawn))
				return null;

			Predicate<Thing> predicate = (Thing x) => HasJobOnThing(pawn, x);
			Thing t = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
				PathEndMode, TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn), 100f, predicate, PotentialWorkThingsGlobal(pawn));
			if (t is null)
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.Flick, t);

		}
	}
}
