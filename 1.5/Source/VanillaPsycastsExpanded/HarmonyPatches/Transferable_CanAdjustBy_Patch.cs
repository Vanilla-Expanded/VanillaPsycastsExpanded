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
    }
}