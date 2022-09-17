namespace VanillaPsycastsExpanded
{
    using System.Collections.Generic;
    using HarmonyLib;
    using Verse;
    using VFECore.Abilities;

    [HarmonyPatch(typeof(CompAbilities), nameof(CompAbilities.GetGizmos))]
    public static class CompAbilities_GetGizmos_Patch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, CompAbilities __instance)
        {
            Pawn                    parent   = __instance.parent as Pawn;
            Hediff_PsycastAbilities psycasts = parent.Psycasts();
            if (psycasts != null && parent is {Drafted: true})
                foreach (Gizmo gizmo in psycasts.GetPsySetGizmos())
                    yield return gizmo;
            foreach (Gizmo gizmo in gizmos)
                if (psycasts != null && gizmo is Command_Ability command && command.ability.def.HasModExtension<AbilityExtension_Psycast>())
                {
                    if (psycasts.ShouldShow(command.ability)) yield return command;
                }
                else yield return gizmo;
        }
    }
}