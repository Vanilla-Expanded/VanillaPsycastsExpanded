namespace VanillaPsycastsExpanded.Chronopath;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

public class Ability_Finish : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (GlobalTargetInfo target in targets)
            if (target.Thing is UnfinishedThing thing)
            {
                List<Thing> ingredients = thing.ingredients;
                RecipeDef   recipe      = thing.Recipe;
                ThingDef    stuff       = thing.Stuff;
                Thing       dominant;
                IntVec3     loc     = thing.Position;
                Pawn        creator = thing.Creator ?? this.pawn;
                if (thing.def.MadeFromStuff) dominant               = ingredients.First(ing => ing.def == stuff);
                else if (ingredients.NullOrEmpty()) dominant        = null;
                else if (recipe.productHasIngredientStuff) dominant = ingredients[0];
                else if (recipe.products.Any(x => x.thingDef.MadeFromStuff))
                    dominant  = ingredients.Where(x => x.def.IsStuff).RandomElementByWeight(x => x.stackCount);
                else dominant = ingredients.RandomElementByWeight(x => x.stackCount);
                List<Thing> products = GenRecipe.MakeRecipeProducts(recipe, creator, ingredients, dominant, thing.BoundWorkTable as IBillGiver).ToList();
                ingredients.ForEach(t => recipe.Worker.ConsumeIngredient(t, recipe, this.pawn.Map));
                thing.BoundBill?.Notify_IterationCompleted(creator, ingredients);
                recipe.Worker.ConsumeIngredient(thing, recipe, this.pawn.Map);
                RecordsUtility.Notify_BillDone(creator, products);
                if (products.Count == 0) return;
                if (recipe.WorkAmountTotal(stuff) >= 10000f)
                    TaleRecorder.RecordTale(TaleDefOf.CompletedLongCraftingProject, creator, products[0].GetInnerIfMinified().def);
                Find.QuestManager.Notify_ThingsProduced(creator, products);
                foreach (Thing product in products)
                    if (!GenPlace.TryPlaceThing(product, loc, this.pawn.Map, ThingPlaceMode.Near))
                        Log.Error($"Could not drop recipe product {product} near {loc}");
            }
    }

    public override bool CanHitTarget(LocalTargetInfo target) => base.CanHitTarget(target) && target.Thing is UnfinishedThing;
}