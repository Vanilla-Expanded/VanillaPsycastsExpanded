namespace VanillaPsycastsExpanded.Staticlord;

using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Ability = VFECore.Abilities.Ability;

public class Ability_Flashstorm : Ability
{
    private readonly HashSet<Faction> affectedFactionCache = new();


    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (GlobalTargetInfo target in targets)
        {
            Map                             map = target.Map;
            Thing                           conditionCauser = GenSpawn.Spawn(ThingDefOf.Flashstorm, target.Cell, this.pawn.Map);
            GameCondition_PsychicFlashstorm flashstorm = (GameCondition_PsychicFlashstorm)GameConditionMaker.MakeCondition(VPE_DefOf.VPE_PsychicFlashstorm);
            flashstorm.centerLocation     = target.Cell.ToIntVec2;
            flashstorm.areaRadiusOverride = new IntRange(Mathf.RoundToInt(this.GetRadiusForPawn()), Mathf.RoundToInt(this.GetRadiusForPawn()));
            flashstorm.Duration           = this.GetDurationForPawn();
            flashstorm.suppressEndMessage = true;
            flashstorm.initialStrikeDelay = new IntRange(0, 0);
            flashstorm.conditionCauser    = conditionCauser;
            flashstorm.ambientSound       = true;
            flashstorm.numStrikes         = Mathf.FloorToInt(this.GetPowerForPawn());
            map.gameConditionManager.RegisterCondition(flashstorm);
            this.ApplyGoodwillImpact(target, flashstorm.AreaRadius);
        }
    }

    private void ApplyGoodwillImpact(GlobalTargetInfo target, int radius)
    {
        if (this.pawn.Faction != Faction.OfPlayer) return;

        this.affectedFactionCache.Clear();
        foreach (Thing thing in GenRadial.RadialDistinctThingsAround(target.Cell, target.Map, radius, true))
            if (thing is Pawn p                             && thing.Faction != null                              && thing.Faction != this.pawn.Faction &&
                !thing.Faction.HostileTo(this.pawn.Faction) && !this.affectedFactionCache.Contains(thing.Faction) &&
                (this.def.applyGoodwillImpactToLodgers || !p.IsQuestLodger()))
            {
                this.affectedFactionCache.Add(thing.Faction);
                Faction.OfPlayer.TryAffectGoodwillWith(thing.Faction, this.def.goodwillImpact, true, true, HistoryEventDefOf.UsedHarmfulAbility);
            }

        this.affectedFactionCache.Clear();
    }
}

public class GameCondition_PsychicFlashstorm : GameCondition_Flashstorm
{
    private static readonly AccessTools.FieldRef<GameCondition_Flashstorm, int> nextLightningTicksRef =
        AccessTools.FieldRefAccess<GameCondition_Flashstorm, int>("nextLightningTicks");

    public int numStrikes;
    public int TicksBetweenStrikes => this.Duration / this.numStrikes;

    private Vector3 RandomLocation() =>
        this.centerLocation.ToVector3() +
        new Vector3(Vortex.Wrap(Mathf.Abs(Rand.Gaussian(0f, this.AreaRadius)), this.AreaRadius), 0f, 0f).RotatedBy(Rand.Range(0f, 360f));

    public override void GameConditionTick()
    {
        base.GameConditionTick();
        if (nextLightningTicksRef(this) - Find.TickManager.TicksGame > this.TicksBetweenStrikes)
            nextLightningTicksRef(this) = this.TicksBetweenStrikes + Find.TickManager.TicksGame;

        for (int i = 0; i < 2; i++) FleckMaker.ThrowSmoke(this.RandomLocation(), this.SingleMap, 4f);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref this.numStrikes, nameof(this.numStrikes));
    }
}