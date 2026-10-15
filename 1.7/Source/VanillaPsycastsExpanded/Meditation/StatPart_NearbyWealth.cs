using UnityEngine;

namespace VanillaPsycastsExpanded;

using RimWorld;
using Verse;

public class StatPart_NearbyWealth : StatPart_Focus
{
    public override void TransformValue(StatRequest req, ref float val)
    {
        if (!this.ApplyOn(req) || req.Thing.Map == null) return;

        // Use the map's wealth, with a minimum of 1000.
        // Negative values and zero would cause bugs.
        float total = Mathf.Max(req.Thing.Map.wealthWatcher.WealthTotal, 1000f);
        if (total <= 0f)
            return;

        // Use the wealth around location, but no more than total map wealth
        var nearby = Mathf.Min(GenRadialCached.WealthAround(req.Thing.Position, req.Thing.Map, 6f, true), total);

        val += nearby / total;
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!this.ApplyOn(req) || req.Thing.Map == null) return string.Empty;

        float total = Mathf.Max(req.Thing.Map.wealthWatcher.WealthTotal, 1000f);
        float nearby = Mathf.Min(GenRadialCached.WealthAround(req.Thing.Position, req.Thing.Map, 6f, true), total);
        return "VPE.WealthNearby".Translate(nearby.ToStringMoney(), total.ToStringMoney()) + ": " +
               this.parentStat.Worker.ValueToString(nearby / total, true, ToStringNumberSense.Offset);
    }
}