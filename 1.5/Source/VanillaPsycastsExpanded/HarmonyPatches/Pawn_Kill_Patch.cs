namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Linq;
    using Verse;
    using VFECore.Abilities;

    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Pawn_Kill_Patch
    {
        private static bool Prefix(Pawn __instance)
        {
            var hediff = __instance.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_DeathShield);
            if (hediff != null)
            {
                return false;
            }
            return true;
        }
        private static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (__instance.Dead)
            {
                if (dinfo.HasValue && dinfo.Value.Instigator is Pawn attacker)
                {
                    var hediff = attacker.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_ControlledFrenzy) as Hediff_Ability;
                    if (hediff != null)
                    {
                        attacker.psychicEntropy.TryAddEntropy(-10f);
                        hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = hediff.ability.GetDurationForPawn();
                    }
                }

                var hediff2 = __instance.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_IceBlock);
                if (hediff2 != null)
                {
                    __instance.health.RemoveHediff(hediff2);
                }
            }
        }
    }
}