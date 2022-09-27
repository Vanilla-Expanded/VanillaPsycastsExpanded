namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;
    using System.Diagnostics;
    using UnityEngine;
    using Verse;

    [HarmonyPatch(typeof(Transferable), "CanAdjustBy")]
	public static class Transferable_CanAdjustBy_Patch
    {
        public static Transferable curTransferable;
		public static void Postfix(Transferable __instance)
		{
            if (curTransferable != __instance && Find.WindowStack.IsOpen<Dialog_Trade>() && TradeSession.trader != null 
                && __instance.ThingDef.IsEltexOrHasEltexMaterial() &&  TradeSession.trader.Faction != Faction.OfEmpire && __instance.CountToTransferToDestination > 0)
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
                else
                {
                    foreach (var recipe in DefDatabase<RecipeDef>.AllDefs)
                    {
                        if (recipe.ProducedThingDef == def && recipe.ingredients.Any(x => x.IsFixedIngredient && x.FixedIngredient == VPE_DefOf.VPE_Eltex))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}