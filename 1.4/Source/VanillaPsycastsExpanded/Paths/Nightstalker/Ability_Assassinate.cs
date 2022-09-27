namespace VanillaPsycastsExpanded.Nightstalker
{
    using System;
    using System.Linq;
    using HarmonyLib;
    using MonoMod.Utils;
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using Verse.Sound;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_Assassinate : Ability
    {
        private static readonly Func<Verb, bool> tryCastShot = AccessTools.Method(typeof(Verb_MeleeAttack), "TryCastShot").CreateDelegate<Func<Verb, bool>>();
        private static readonly AccessTools.FieldRef<Verb, LocalTargetInfo> currentTarget = AccessTools.FieldRefAccess<Verb, LocalTargetInfo>("currentTarget");
        private                 int attacksLeft;
        private                 Pawn target;
        private                 IntVec3 originalPosition;

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            this.target = targets.FirstOrDefault(t => t.Thing is Pawn).Thing as Pawn;
            if (this.target == null) return;
            this.attacksLeft = Mathf.RoundToInt(this.GetPowerForPawn());
            Map map = this.pawn.Map;
            this.originalPosition = this.pawn.Position;
            this.target.stances.stunner.StunFor(this.attacksLeft * 2, this.pawn);
            this.TeleportPawnTo(GenAdjFast.AdjacentCellsCardinal(this.target.Position).Where(c => c.Walkable(map)).RandomElement());
        }

        public override void Tick()
        {
            base.Tick();
            if (this.attacksLeft > 0)
            {
                this.attacksLeft--;
                this.DoAttack();
                if (this.attacksLeft == 0)
                {
                    VPE_DefOf.VPE_Assassinate_Return.PlayOneShot(this.pawn);
                    this.TeleportPawnTo(this.originalPosition);
                }
            }
        }

        private void DoAttack()
        {
            Verb verb = this.pawn.meleeVerbs.GetUpdatedAvailableVerbsList(false).MaxBy(v => VerbUtility.DPS(v.verb, this.pawn)).verb;
            this.pawn.meleeVerbs.TryMeleeAttack(this.target, verb, true);
            this.pawn.stances.CancelBusyStanceHard();
            FleckMaker.AttachedOverlay(this.target, VPE_DefOf.VPE_Slash, Rand.InsideUnitCircle * 0.3f);
        }

        private void TeleportPawnTo(IntVec3 c)
        {
            FleckCreationData dataAttachedOverlay = FleckMaker.GetDataAttachedOverlay(this.pawn, FleckDefOf.PsycastSkipFlashEntry, Vector3.zero);
            dataAttachedOverlay.link.detachAfterTicks = 1;
            this.pawn.Map.flecks.CreateFleck(dataAttachedOverlay);
            TargetInfo dest = new(c, this.pawn.Map);
            FleckMaker.Static(dest.Cell, dest.Map, FleckDefOf.PsycastSkipInnerExit);
            FleckMaker.Static(dest.Cell, dest.Map, FleckDefOf.PsycastSkipOuterRingExit);
            SoundDefOf.Psycast_Skip_Entry.PlayOneShot(this.pawn);
            SoundDefOf.Psycast_Skip_Exit.PlayOneShot(dest);
            this.AddEffecterToMaintain(EffecterDefOf.Skip_EntryNoDelay.Spawn(this.pawn, this.pawn.Map), this.pawn.Position, 60);
            this.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(dest.Cell, dest.Map),       dest.Cell,          60);
            this.pawn.Position = c;
            this.pawn.Notify_Teleported();
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (target.Pawn is { } targetPawn)
            {
                if (targetPawn.Map.glowGrid.GameGlowAt(targetPawn.Position) <= 0.29f) return true;
                if (showMessages) Messages.Message("VPE.MustBeInDark".Translate(), MessageTypeDefOf.RejectInput, false);
            }

            return false;
        }
    }
}