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
	public class JobGiver_Clean : ThinkNode_JobGiver
	{
		private int MinTicksSinceThickened = 600;
		public PathEndMode PathEndMode => PathEndMode.Touch;

		public IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.listerFilthInHomeArea.FilthInHomeArea;
		}
		public bool ShouldSkip(Pawn pawn)
		{
			return pawn.Map.listerFilthInHomeArea.FilthInHomeArea.Count == 0;
		}
		public bool HasJobOnThing(Pawn pawn, Thing t)
		{
			Filth filth = t as Filth;
			if (filth == null)
			{
				return false;
			}
			if (!filth.Map.areaManager.Home[filth.Position])
			{
				return false;
			}
			if (!pawn.CanReserve(t, 1, -1, null))
			{
				return false;
			}
			if (filth.TicksSinceThickened < MinTicksSinceThickened)
			{
				return false;
			}
			return true;
		}
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (ShouldSkip(pawn))
				return null;

			Predicate<Thing> predicate = (Thing x) => x.def.category == ThingCategory.Filth && HasJobOnThing(pawn, x);
			Thing t = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Filth),
				PathEndMode, TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn), 100f, predicate, PotentialWorkThingsGlobal(pawn));
			if (t is null)
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.Clean);
			job.AddQueuedTarget(TargetIndex.A, t);
			int num = 15;
			Map map = t.Map;
			Room room = t.GetRoom();
			for (int i = 0; i < 100; i++)
			{
				IntVec3 c2 = t.Position + GenRadial.RadialPattern[i];
				if (!ShouldClean(c2))
				{
					continue;
				}
				List<Thing> thingList = c2.GetThingList(map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Thing thing = thingList[j];
					if (HasJobOnThing(pawn, thing) && thing != t)
					{
						job.AddQueuedTarget(TargetIndex.A, thing);
					}
				}
				if (job.GetTargetQueue(TargetIndex.A).Count >= num)
				{
					break;
				}
			}
			if (job.targetQueueA != null && job.targetQueueA.Count >= 5)
			{
				job.targetQueueA.SortBy((LocalTargetInfo targ) => targ.Cell.DistanceToSquared(pawn.Position));
			}
			return job;
			bool ShouldClean(IntVec3 c)
			{
				if (!c.InBounds(map))
				{
					return false;
				}
				Room room2 = c.GetRoom(map);
				if (room == room2)
				{
					return true;
				}
				Region region = c.GetDoor(map)?.GetRegion(RegionType.Portal);
				if (region != null && !region.links.NullOrEmpty())
				{
					for (int k = 0; k < region.links.Count; k++)
					{
						RegionLink regionLink = region.links[k];
						for (int l = 0; l < 2; l++)
						{
							if (regionLink.regions[l] != null && regionLink.regions[l] != region && regionLink.regions[l].valid && regionLink.regions[l].Room == room)
							{
								return true;
							}
						}
					}
				}
				return false;
			}
		}
	}
}
