namespace VanillaPsycastsExpanded;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

public class StatPart_NearbyFoci : StatPart
{
    public static bool ShouldApply = true;

    public override void TransformValue(StatRequest req, ref float val)
    {
        if (req.Thing == null || req.Pawn == null || !ShouldApply || req.Thing.Map == null) return;
        try
        {
            ShouldApply =  false;
            // Use foreach loop over Linq's Sum() to avoid the overhead invoking the delegate, which had significant effect on performance.
            foreach (var tuple in AllFociNearby(req.Thing, req.Pawn)) val += tuple.value;
        }
        finally
        {
            ShouldApply =  true;
        }
    }

    private static IEnumerable<(Thing thing, float value)> AllFociNearby(Thing main, Pawn pawn)
    {
        var compMain = main.TryGetComp<CompMeditationFocus>();
        if (compMain == null)
        {
            return Enumerable.Empty<(Thing, float)>();
        }
        var map = pawn.Map;
        var pos = main.Position;
        var collectedFocusTypes = new HashSet<MeditationFocusDef>(compMain.Props.focusTypes);
        var potentialFoci = new List<(Thing thing, List<MeditationFocusDef> types, float value)>();
        float searchRadiusSq = MeditationUtility.FocusObjectSearchRadius * MeditationUtility.FocusObjectSearchRadius;
        List<Thing> thingsInGroup = map.listerThings.ThingsInGroup(ThingRequestGroup.MeditationFocus);
        for (int i = 0; i < thingsInGroup.Count; i++)
        {
            Thing thing = thingsInGroup[i];
            if (thing.Position.DistanceToSquared(pos) > searchRadiusSq)
            {
                continue;
            }

            var comp = thing.TryGetComp<CompMeditationFocus>();
            if (comp == null || !comp.CanPawnUse(pawn))
            {
                continue;
            }

            float strength = thing.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn);
            potentialFoci.Add((thing, comp.Props.focusTypes, strength));
        }

        potentialFoci.Sort((a, b) => b.value.CompareTo(a.value));
        var result = new List<(Thing, float)>();

        foreach ((Thing thing, List<MeditationFocusDef> types, float value) in potentialFoci)
        {
            bool hasNewType = false;
            foreach (MeditationFocusDef type in types)
            {
                if (collectedFocusTypes.Add(type))
                {
                    hasNewType = true;
                }
            }
            if (hasNewType)
            {
                result.Add((thing, value));
            }
        }
        return result;
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (req.Thing == null || req.Pawn == null || !ShouldApply || req.Thing.Map == null) return "";
        try
        {
            ShouldApply = false;
            List<string> lines = AllFociNearby(req.Thing, req.Pawn)
                                 .Select(tuple => tuple.thing.LabelCap + ": " +
                                                  StatDefOf.MeditationFocusStrength
                                                           .Worker.ValueToString(tuple.value, true, ToStringNumberSense.Offset))
                                 .ToList();
            return lines.Count > 0 ? "VPE.Nearby".Translate() + ":\n" + lines.ToLineList("  ", true) : "";
        }
        finally
        {
            ShouldApply = true;
        }
    }
}