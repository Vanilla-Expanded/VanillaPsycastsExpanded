namespace VanillaPsycastsExpanded;

using System.Linq;
using RimWorld;
using Verse;

public class StatPart_NearbyWealth : StatPart_Focus
{
    public override void TransformValue(StatRequest req, ref float val)
    {
        if (!this.ApplyOn(req) || req.Thing.Map == null) return;

        float total  = req.Thing.Map.wealthWatcher.WealthTotal;
        float nearby = GenRadialCached.RadialDistinctThingsAround(req.Thing.Position, req.Thing.Map, 6f, true).Sum(t => t.MarketValue * t.stackCount);
        val += nearby / total;
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!this.ApplyOn(req) || req.Thing.Map == null) return "";

        float total  = req.Thing.Map.wealthWatcher.WealthTotal;
        float nearby = GenRadialCached.RadialDistinctThingsAround(req.Thing.Position, req.Thing.Map, 6f, true).Sum(t => t.MarketValue * t.stackCount);
        return "VPE.WealthNearby".Translate(nearby.ToStringMoney(), total.ToStringMoney()) + ": " +
               this.parentStat.Worker.ValueToString(nearby / total, true, ToStringNumberSense.Offset);
    }
}