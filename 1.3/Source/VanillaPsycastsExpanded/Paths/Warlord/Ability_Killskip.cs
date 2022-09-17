namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using RimWorld;
    using RimWorld.Planet;
    using System.Collections.Generic;
    using Verse;
    using Verse.AI;
    using Verse.Sound;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_Killskip : Ability
    {
        private int attackInTicks = -1;
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            AttackTarget(((LocalTargetInfo)targets[0]));
            TryQueueAttackIfDead(((LocalTargetInfo)targets[0]));
        }
        private void TryQueueAttackIfDead(LocalTargetInfo target)
        {
            if (target.Pawn.Dead)
            {
                attackInTicks = Find.TickManager.TicksGame + this.def.castTime;
            }
            else
            {
                attackInTicks = -1;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (attackInTicks != -1 && Find.TickManager.TicksGame >= attackInTicks)
            {
                attackInTicks = -1;
                var target = FindAttackTarget();
                if (target != null)
                {
                    AttackTarget(target);
                    TryQueueAttackIfDead(target);
                }
            }
        }

        private void AttackTarget(LocalTargetInfo target)
        {
            this.AddEffecterToMaintain(EffecterDefOf.Skip_Entry.Spawn(pawn.Position, this.pawn.Map, 0.72f), pawn.Position, 60);
            this.AddEffecterToMaintain(VPE_DefOf.VPE_Skip_ExitNoDelayRed.Spawn(target.Cell, this.pawn.Map, 0.72f), target.Cell, 60);
            this.pawn.Position = target.Cell;
            this.pawn.Notify_Teleported(false);
            this.pawn.stances.SetStance(new Stance_Mobile());
            VerbProperties_AdjustedMeleeDamageAmount_Patch.multiplyByPawnMeleeSkill = true;
            this.pawn.meleeVerbs.TryMeleeAttack(target.Pawn, null, true);
            this.pawn.meleeVerbs.TryMeleeAttack(target.Pawn, null, true); // deals two attack at once
            VerbProperties_AdjustedMeleeDamageAmount_Patch.multiplyByPawnMeleeSkill = false;
            castSounds.RandomElement().PlayOneShot(pawn);
        }

        private static List<SoundDef> castSounds = new List<SoundDef>
        {
            VPE_DefOf.VPE_Killskip_Jump_01a,
            VPE_DefOf.VPE_Killskip_Jump_01b,
            VPE_DefOf.VPE_Killskip_Jump_01c,
        };
		private Pawn FindAttackTarget()
        {
            TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat 
                | TargetScanFlags.NeedAutoTargetable;
            return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, (Thing x) => x is Pawn pawn && !pawn.Dead, 0f, 999999);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref attackInTicks, "attackInTicks", -1);
        }
    }
}