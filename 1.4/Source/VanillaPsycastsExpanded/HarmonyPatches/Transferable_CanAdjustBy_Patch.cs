namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using UnityEngine;
    using Verse;

    [HarmonyPatch(typeof(Transferable), "CanAdjustBy")]
	public static class Transferable_CanAdjustBy_Patch
    {
        public static Transferable curTransferable;
        private static readonly HashSet<ThingDef> eltexThings;

        static Transferable_CanAdjustBy_Patch()
        {
            eltexThings = DefDatabase<RecipeDef>.AllDefs
                .Where(recipe => recipe.ingredients.Any(x => x.IsFixedIngredient && x.FixedIngredient == VPE_DefOf.VPE_Eltex))
                .Select(recipe => recipe.ProducedThingDef)
                .ToHashSet();
        }

		public static void Postfix(Transferable __instance)
		{
            if (curTransferable != __instance && Find.WindowStack.IsOpen<Dialog_Trade>() && __instance.CountToTransferToDestination > 0 && TradeSession.trader != null
                && TradeSession.trader.Faction != Faction.OfEmpire && __instance.ThingDef.IsEltexOrHasEltexMaterial())
            {
                curTransferable = __instance;
                if (TradeSession.giftMode)
                {
                    Messages.Message("VPE.GiftingEltexWarning".Translate(), MessageTypeDefOf.CautionInput);
                }
                else
                {
                    Messages.Message("VPE.SellingEltexWarning".Translate(), MessageTypeDefOf.CautionInput);
                }
            }
		}

        public static bool IsEltexOrHasEltexMaterial(this ThingDef def)
        {
            if (def != null)
            {
                if (def == VPE_DefOf.VPE_Eltex)
                {
                    return true;
                }
                else if (def.costList != null && def.costList.Any(x => x.thingDef == VPE_DefOf.VPE_Eltex))
                {
                    return true;
                }
                else if (eltexThings.Contains(def))
                {
                    return true;
                }
            }
            return false;
        }
    }
}