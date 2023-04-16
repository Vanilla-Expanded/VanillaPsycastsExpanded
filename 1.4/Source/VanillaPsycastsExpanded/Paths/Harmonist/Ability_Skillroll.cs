namespace VanillaPsycastsExpanded.Harmonist
{
    using System;
    using System.Linq;
    using HarmonyLib;
    using MonoMod.Utils;
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_Skillroll : Ability
    {
        private static readonly Func<Pawn, SkillDef, PawnGenerationRequest, int> finalLevelOfSkill =
            AccessTools.Method(typeof(PawnGenerator), "FinalLevelOfSkill").CreateDelegate<Func<Pawn, SkillDef, PawnGenerationRequest, int>>();

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            Pawn pawn   = targets[0].Thing as Pawn;
            int  points = 0;

            var request = new PawnGenerationRequest(pawn.kindDef, developmentalStages: pawn.DevelopmentalStage);
            foreach (SkillRecord skill in pawn.skills.skills)
            {
                int oldLevel = skill.levelInt;
                skill.levelInt =  finalLevelOfSkill(pawn, skill.def, request);
                points         += oldLevel - skill.levelInt;
            }

            points = Mathf.RoundToInt(points * 1.1f);
            for (int i = 0; i < points; i++)
            {
                SkillRecord skill = pawn.skills.skills.Where(skill => !skill.TotallyDisabled && skill.levelInt < 20).RandomElement();
                skill.levelInt++;
            }
        }

        public override bool CanHitTarget(LocalTargetInfo target) =>
            base.CanHitTarget(target) && target.Pawn is {Faction: { } targetFaction} && this.pawn is {Faction: { } faction} &&
            (faction == targetFaction || faction.RelationKindWith(targetFaction) == FactionRelationKind.Ally);
    }
}