using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaPsycastsExpanded;

public class RaidStrategyWorker_ImmediateAttack_Psycasters : RaidStrategyWorker_ImmediateAttack
{
    public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
    {
        if (!PawnGenOptionsWithRequiredPawns(parms.faction, groupKind).Any()) return false;
        return base.CanUseWith(parms, groupKind);
    }

    protected bool MatchesRequiredPawnKind(PawnKindDef kind) => kind.HasModExtension<PawnKindAbilityExtension_Psycasts>();

    protected int MinRequiredPawnsForPoints(float pointsTotal, Faction faction = null) => 1;

    public override float MinimumPoints(Faction faction, PawnGroupKindDef groupKind) =>
        Mathf.Max(base.MinimumPoints(faction, groupKind), CheapestRequiredPawnCost(faction, groupKind));

    public override float MinMaxAllowedPawnGenOptionCost(Faction faction, PawnGroupKindDef groupKind) => CheapestRequiredPawnCost(faction, groupKind);

    private float CheapestRequiredPawnCost(Faction faction, PawnGroupKindDef groupKind)
    {
        var enumerable = PawnGenOptionsWithRequiredPawns(faction, groupKind);
        if (!enumerable.Any())
        {
            Log.Error("Tried to get MinimumPoints for " + GetType() + " for faction " + faction +
                      " but the faction has no groups with the required pawn kind. groupKind=" + groupKind);
            return 99999f;
        }

        var num = 9999999f;
        foreach (var item in enumerable)
        foreach (var item2 in item.options.Where(op => MatchesRequiredPawnKind(op.kind)))
            if (item2.Cost < num)
                num = item2.Cost;
        return num;
    }

    public override bool CanUsePawnGenOption(float pointsTotal, PawnGenOption g, List<PawnGenOptionWithXenotype> chosenGroups, Faction faction = null)
    {
        if (chosenGroups != null && chosenGroups.Count < MinRequiredPawnsForPoints(pointsTotal, faction) && !MatchesRequiredPawnKind(g.kind)) return false;

        return true;
    }

    private IEnumerable<PawnGroupMaker> PawnGenOptionsWithRequiredPawns(Faction faction, PawnGroupKindDef groupKind)
    {
        if (faction.def.pawnGroupMakers == null) return Enumerable.Empty<PawnGroupMaker>();
        return faction.def.pawnGroupMakers.Where(gm => gm.kindDef == groupKind && gm.options != null && gm.options.Any(op => MatchesRequiredPawnKind(op.kind)));
    }
}