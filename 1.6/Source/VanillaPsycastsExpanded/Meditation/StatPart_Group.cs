namespace VanillaPsycastsExpanded;

using System.Linq;
using RimWorld;
using Verse;

public class StatPart_Group : StatPart_Focus
{
    public override void TransformValue(StatRequest req, ref float val)
    {
        if (!this.ApplyOn(req) || req.Thing.Map == null || req.Thing.Faction == null) return;

        val += MeditatingPawnsAround(req.Thing) switch
        {
            <= 1 => 0,
            2    => 0.06f,
            3    => 0.2f,
            4    => 0.45f,
            >= 5 => 0.8f
        };
    }

    private static int MeditatingPawnsAround(Thing thing)
    {
        return thing.Map.mapPawns.PawnsInFaction(thing.Faction)
                    .Count(p => p.Position.InHorDistOf(thing.Position, 5f) && p.CurJobDef == JobDefOf.Meditate);
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!this.ApplyOn(req) || req.Thing.Map == null || req.Thing.Faction == null) return "";

        int num = MeditatingPawnsAround(req.Thing);
        return "VPE.GroupFocus".Translate(num - 1) + ": " + this.parentStat.Worker.ValueToString(num switch
        {
            <= 1 => 0,
            2    => 0.06f,
            3    => 0.2f,
            4    => 0.45f,
            >= 5 => 0.8f
        }, true, ToStringNumberSense.Offset);
    }
}