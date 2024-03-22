namespace VanillaPsycastsExpanded.Harmonist
{
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_LuckTransfer : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            if (targets[0].Thing is not Pawn source || targets[1].Thing is not Pawn dest) return;
            MoteBetween mote = (MoteBetween) ThingMaker.MakeThing(VPE_DefOf.VPE_PsycastPsychicEffectTransfer);
            mote.Attach(source, dest);
            mote.Scale         = 1f;
            mote.exactPosition = source.DrawPos;
            GenSpawn.Spawn(mote, source.Position, source.MapHeld);
            mote = (MoteBetween) ThingMaker.MakeThing(VPE_DefOf.VPE_PsycastPsychicEffectTransfer);
            mote.Attach(dest, source);
            mote.Scale         = 1f;
            mote.exactPosition = dest.DrawPos;
            GenSpawn.Spawn(mote, dest.Position, dest.MapHeld);

            int duration = Mathf.RoundToInt(this.GetDurationForPawn() * source.GetStatValue(StatDefOf.PsychicSensitivity));

            source.health.AddHediff(VPE_DefOf.VPE_Lucky).TryGetComp<HediffComp_Disappears>().ticksToDisappear = duration;
            dest.health.AddHediff(VPE_DefOf.VPE_UnLucky).TryGetComp<HediffComp_Disappears>().ticksToDisappear = duration;
        }
    }
}