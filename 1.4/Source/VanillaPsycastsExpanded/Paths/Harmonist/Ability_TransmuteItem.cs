using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VFECore;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded.Harmonist;

[HotSwappable]
public class Ability_TransmuteItem : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (var target in targets)
        {
            var item = target.Thing;
            var map = item.Map;
            var value = item.MarketValue * item.stackCount;
            var pos = item.Position;

            var allItems =
                (from thingDef in DefDatabase<ThingDef>.AllDefs
                    where IsValid(thingDef)
                    let marketValue = thingDef.BaseMarketValue
                    let count = Mathf.FloorToInt(value / thingDef.BaseMarketValue)
                    where marketValue <= value
                    where count <= thingDef.stackLimit
                    where count >= 1
                    select thingDef
                ).ToList();

            float WeightSelector(ThingDef thingDef)
            {
                var amount = value / thingDef.BaseMarketValue;
                var weight = Mathf.Abs(amount - Mathf.FloorToInt(amount));
                return weight;
            }

            var maxWeight = allItems.Max(WeightSelector);
            var chosen = allItems.RandomElementByWeight(thingDef => maxWeight - WeightSelector(thingDef));
            item.Destroy();
            item = ThingMaker.MakeThing(chosen);
            item.stackCount = Mathf.FloorToInt(value / chosen.BaseMarketValue);
            GenSpawn.Spawn(item, pos, map);
        }
    }

    private bool IsValid(ThingDef thingDef)
    {
        if (thingDef.category != ThingCategory.Item) return false;
        if (thingDef.IsCorpse) return false;
        if (thingDef.MadeFromStuff) return false;
        if (thingDef.IsEgg) return false;
        if (thingDef.tradeTags != null)
            if (thingDef.tradeTags.Any(tag => tag.Contains("CE") && tag.Contains("Ammo")))
                return false;

        return true;
    }

    public override bool CanHitTarget(LocalTargetInfo target) =>
        targetParams.CanTarget(target.Thing, this) &&
        GenSight.LineOfSight(pawn.Position, target.Cell, pawn.Map, true);

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        if (!base.ValidateTarget(target, showMessages)) return false;
        if (target.Thing.MarketValue < 1f)
        {
            if (showMessages) Messages.Message("VPE.TooCheap".Translate(), MessageTypeDefOf.RejectInput, false);
            return false;
        }

        return true;
    }
}
