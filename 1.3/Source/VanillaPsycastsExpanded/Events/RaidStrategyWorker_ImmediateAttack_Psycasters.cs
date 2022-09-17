namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    using VFECore.Abilities;

    public class RaidStrategyWorker_ImmediateAttack_Psycasters : RaidStrategyWorker_ImmediateAttack
    {
		public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
		{
			if (!PawnGenOptionsWithRequiredPawns(parms.faction, groupKind).Any())
			{
				return false;
			}
			return base.CanUseWith(parms, groupKind);
		}
		protected bool MatchesRequiredPawnKind(PawnKindDef kind)
		{
			return kind.HasModExtension<PawnKindAbilityExtension_Psycasts>();
		}
        protected int MinRequiredPawnsForPoints(float pointsTotal, Faction faction = null)
		{
			return 1;
		}
		public override float MinimumPoints(Faction faction, PawnGroupKindDef groupKind)
		{
			return Mathf.Max(base.MinimumPoints(faction, groupKind), CheapestRequiredPawnCost(faction, groupKind));
		}

		public override float MinMaxAllowedPawnGenOptionCost(Faction faction, PawnGroupKindDef groupKind)
		{
			return CheapestRequiredPawnCost(faction, groupKind);
		}

		private float CheapestRequiredPawnCost(Faction faction, PawnGroupKindDef groupKind)
		{
			IEnumerable<PawnGroupMaker> enumerable = PawnGenOptionsWithRequiredPawns(faction, groupKind);
			if (!enumerable.Any())
			{
				Log.Error("Tried to get MinimumPoints for " + GetType().ToString() + " for faction " + faction.ToString() + " but the faction has no groups with the required pawn kind. groupKind=" + groupKind);
				return 99999f;
			}
			float num = 9999999f;
			foreach (PawnGroupMaker item in enumerable)
			{
				foreach (PawnGenOption item2 in item.options.Where((PawnGenOption op) => MatchesRequiredPawnKind(op.kind)))
				{
					if (item2.Cost < num)
					{
						num = item2.Cost;
					}
				}
			}
			return num;
		}

		public override bool CanUsePawnGenOption(float pointsTotal, PawnGenOption opt, List<PawnGenOption> chosenOpts, Faction faction = null)
		{
			if (chosenOpts.Count < MinRequiredPawnsForPoints(pointsTotal, faction) && !MatchesRequiredPawnKind(opt.kind))
			{
				return false;
			}
			return true;
		}

		private IEnumerable<PawnGroupMaker> PawnGenOptionsWithRequiredPawns(Faction faction, PawnGroupKindDef groupKind)
		{
			if (faction.def.pawnGroupMakers == null)
			{
				return Enumerable.Empty<PawnGroupMaker>();
			}
			return faction.def.pawnGroupMakers.Where((PawnGroupMaker gm) => gm.kindDef == groupKind && gm.options != null && gm.options.Any((PawnGenOption op) => MatchesRequiredPawnKind(op.kind)));
		}
	}
}