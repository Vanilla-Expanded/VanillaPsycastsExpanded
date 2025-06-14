namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Verse;
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "GetLetterText")]
    public static class IncidentWorker_RaidEnemy_GetLetterText_Patch
    {
        private static bool Prefix(ref string __result, IncidentParms parms, List<Pawn> pawns)
        {
            if (parms.raidStrategy.Worker is RaidStrategyWorker_ImmediateAttack_Psycasters)
            {
                string text = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, 
                    parms.faction.Name.ApplyTag(parms.faction)).CapitalizeFirst();
                text += "\n\n";
                text += parms.raidStrategy.arrivalTextEnemy;
                var psycasters = pawns.Where(x => x.HasPsylink).ToList();
                var builder = new StringBuilder();
                foreach (var p in psycasters.Select(x => x.Name + " - " + x.KindLabel))
                {
                    builder.AppendLine(p);
                }
                text += "VPE.PsycasterRaidDescription".Translate(builder.ToString());
                Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
                if (pawn != null)
                {
                    text += "\n\n";
                    text += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
                }
                __result = text;
                return false;
            }
            return true;
        }
    }
}