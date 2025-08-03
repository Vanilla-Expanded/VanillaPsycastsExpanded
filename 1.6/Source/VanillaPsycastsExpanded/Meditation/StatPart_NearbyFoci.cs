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
            var list = AllFociNearby(req.Thing, req.Pawn);
            for (var i = 0; i < list.Count; i++) val += list[i].value;
        }
        finally
        {
            ShouldApply =  true;
        }
    }

    private static List<(Thing thing, float value)> AllFociNearby(Thing main, Pawn pawn)
    {
        var compMain = main.TryGetComp<CompMeditationFocus>();
        if (compMain == null)
        {
            return [];
        }
        var map = pawn.Map;
        var pos = main.Position;
        var collectedFocusTypes = new HashSet<MeditationFocusDef>(compMain.Props.focusTypes);
        var potentialFoci = new List<(Thing thing, List<MeditationFocusDef> types, float value)>();
        var meditationFoci = GenRadialCached.MeditationFociAround(pos, map, MeditationUtility.FocusObjectSearchRadius, true);
        foreach (var focus in meditationFoci)
        {
            if (!focus.CanPawnUse(pawn))
            {
                continue;
            }

            float strength = focus.parent.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn);
            potentialFoci.Add((focus.parent, focus.Props.focusTypes, strength));
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