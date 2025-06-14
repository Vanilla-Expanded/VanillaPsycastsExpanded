using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaPsycastsExpanded;

public class JobGiver_Flick : ThinkNode_JobGiver
{
    public PathEndMode PathEndMode => PathEndMode.Touch;

    public IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        var desList = pawn.Map.designationManager.designationsByDef[DesignationDefOf.Flick];
        foreach (var des in desList) yield return des.target.Thing;
    }

    public bool ShouldSkip(Pawn pawn, bool forced = false) => !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.Flick);

    public bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Flick) == null) return false;
        if (!pawn.CanReserve(t, 1, -1, null, forced)) return false;
        return true;
    }

    protected override Job TryGiveJob(Pawn pawn)
    {
        if (ShouldSkip(pawn))
            return null;

        Predicate<Thing> predicate = x => HasJobOnThing(pawn, x);
        var t = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
            PathEndMode, TraverseParms.For(pawn, Danger.Some), 100f, predicate, PotentialWorkThingsGlobal(pawn));
        if (t is null) return null;
        return JobMaker.MakeJob(JobDefOf.Flick, t);
    }
}