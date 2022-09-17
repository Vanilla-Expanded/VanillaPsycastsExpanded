namespace VanillaPsycastsExpanded
{
    using RimWorld;
	using RimWorld.Planet;
	using System.Collections.Generic;
    using Verse;
    using Verse.AI;
    using Verse.Sound;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;

    public class AbilityExtension_Neuroquake : AbilityExtension_AbilityMod
	{
		private Dictionary<Faction, Pair<bool, Pawn>> affectedFactions;

		private List<Pawn> giveMentalStateTo = new List<Pawn>();

		public int goodwillImpactForNeuroquake;

		public int goodwillImpactForBerserk;

		public int worldRangeTiles;
        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
			if (affectedFactions == null)
			{
				affectedFactions = new Dictionary<Faction, Pair<bool, Pawn>>();
			}
			else
			{
				affectedFactions.Clear();
			}
			giveMentalStateTo.Clear();
			foreach (Pawn item in ability.pawn.Map.mapPawns.AllPawnsSpawned)
			{
				if (CanApplyEffects(item) && !item.Fogged())
				{
					bool flag = !item.Spawned || item.Position.InHorDistOf(ability.pawn.Position, ability.GetAdditionalRadius())
						|| !item.Position.InHorDistOf(ability.pawn.Position, ability.GetRadiusForPawn());
					AffectGoodwill(item.HomeFaction, !flag, item);
					if (!flag)
					{
						giveMentalStateTo.Add(item);
					}
					else
					{
						GiveNeuroquakeThought(item);
					}
				}
			}
			foreach (Map map in Find.Maps)
			{
				if (map == ability.pawn.Map || Find.WorldGrid.TraversalDistanceBetween(map.Tile, ability.pawn.Map.Tile, passImpassable: true,
					worldRangeTiles + 1) > worldRangeTiles)
				{
					continue;
				}
				foreach (Pawn allPawn in map.mapPawns.AllPawns)
				{
					if (CanApplyEffects(allPawn))
					{
						GiveNeuroquakeThought(allPawn);
					}
				}
			}
			foreach (Caravan caravan in Find.WorldObjects.Caravans)
			{
				if (Find.WorldGrid.TraversalDistanceBetween(caravan.Tile, ability.pawn.Map.Tile, passImpassable: true, worldRangeTiles + 1) > worldRangeTiles)
				{
					continue;
				}
				foreach (Pawn pawn in caravan.pawns)
				{
					if (CanApplyEffects(pawn))
					{
						GiveNeuroquakeThought(pawn);
					}
				}
			}
			foreach (Pawn item2 in giveMentalStateTo)
			{
				MentalStateDef mentalStateDef = null;
				mentalStateDef = (item2.RaceProps.IsMechanoid ? MentalStateDefOf.BerserkMechanoid : MentalStateDefOf.Berserk);
				AbilityExtension_GiveMentalState.TryGiveMentalStateWithDuration(mentalStateDef, item2, ability, StatDefOf.PsychicSensitivity, false);
				RestUtility.WakeUp(item2);
			}
			foreach (Faction allFaction in Find.FactionManager.AllFactions)
			{
				if (!allFaction.IsPlayer && !allFaction.defeated)
				{
					AffectGoodwill(allFaction, gaveMentalBreak: false);
				}
			}
			if (ability.pawn.Faction == Faction.OfPlayer)
			{
				foreach (KeyValuePair<Faction, Pair<bool, Pawn>> affectedFaction in affectedFactions)
				{
					Faction key = affectedFaction.Key;
					bool first = affectedFaction.Value.First;
					_ = affectedFaction.Value.Second;
					int goodwillChange = (first ? goodwillImpactForBerserk : goodwillImpactForNeuroquake);
					Faction.OfPlayer.TryAffectGoodwillWith(key, goodwillChange, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.UsedHarmfulAbility);
				}
			}
			base.Cast(targets, ability);
			affectedFactions.Clear();
			giveMentalStateTo.Clear();
			var coma = HediffMaker.MakeHediff(VPE_DefOf.PsychicComa, ability.pawn);
			coma.TryGetComp<HediffComp_Disappears>().ticksToDisappear = (int)((GenDate.TicksPerDay * 5) / ability.pawn.GetStatValue(StatDefOf.PsychicSensitivity));
			ability.pawn.health.AddHediff(coma);
		}

		private void AffectGoodwill(Faction faction, bool gaveMentalBreak, Pawn p = null)
		{
			if (faction != null && !faction.IsPlayer && !faction.HostileTo(Faction.OfPlayer) && (p == null || !p.IsSlaveOfColony) && (!affectedFactions.TryGetValue(faction, out var value) || (!value.First && gaveMentalBreak)))
			{
				affectedFactions[faction] = new Pair<bool, Pawn>(gaveMentalBreak, p);
			}
		}

		private void GiveNeuroquakeThought(Pawn p)
		{
			p.needs?.mood?.thoughts.memories.TryGainMemory(ThoughtDefOf.NeuroquakeEcho);
		}

		private bool CanApplyEffects(Pawn p)
		{
			if (!p.Dead && !p.Suspended)
			{
				return p.GetStatValue(StatDefOf.PsychicSensitivity) > float.Epsilon;
			}
			return false;
		}
	}
}