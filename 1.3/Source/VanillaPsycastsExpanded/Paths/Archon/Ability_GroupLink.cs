namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Verse.Sound;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;
    public class Ability_GroupLink : Ability
    {
        public override Hediff ApplyHediff(Pawn targetPawn, HediffDef hediffDef, BodyPartRecord bodyPart, int duration, float severity)
        {
            var hediff = base.ApplyHediff(targetPawn, hediffDef, bodyPart, duration, severity) as Hediff_GroupLink;
            hediff.LinkAllPawnsAround();
            return hediff;
        }
    }
}