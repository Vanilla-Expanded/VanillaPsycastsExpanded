﻿namespace VanillaPsycastsExpanded;

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
        if (compMain == null) return Enumerable.Empty<(Thing, float)>();

        HashSet<MeditationFocusDef> focusTypes = compMain.Props.focusTypes.ToHashSet();
        List<(Thing thing, List<MeditationFocusDef> foci, float value)> foci =
            (from thing in GenRadialCached.RadialDistinctThingsAround(main.Position, main.Map, MeditationUtility.FocusObjectSearchRadius, true)
             let comp = thing.TryGetComp<CompMeditationFocus>()
             where comp != null && comp.CanPawnUse(pawn)
             let value = thing.GetStatValueForPawn(
                 StatDefOf.MeditationFocusStrength, pawn)
             orderby value descending
             select (thing, comp.Props.focusTypes, value)).ToList();

        List<(Thing, float)> result = new();
        foreach ((Thing thing, List<MeditationFocusDef> types, float value) in foci)
            if (types.Any(type => !focusTypes.Contains(type)))
            {
                focusTypes.UnionWith(types);
                result.Add((thing, value));
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