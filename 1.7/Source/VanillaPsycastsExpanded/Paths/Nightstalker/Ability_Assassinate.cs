using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;
using Ability = VEF.Abilities.Ability;

namespace VanillaPsycastsExpanded.Nightstalker;

public class Ability_Assassinate : Ability
{
    private int attacksLeft;
    private IntVec3 originalPosition;
    private Pawn target;

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        target = targets.FirstOrDefault(t => t.Thing is Pawn).Thing as Pawn;
        if (target == null) return;
        attacksLeft = Mathf.RoundToInt(GetPowerForPawn());
        var map = pawn.Map;
        originalPosition = pawn.Position;
        target.stances.stunner.StunFor(attacksLeft * 2, pawn);
        TeleportPawnTo(GenAdjFast.AdjacentCellsCardinal(target.Position).Where(c => c.Walkable(map)).RandomElement());
    }

    public override void Tick()
    {
        base.Tick();
        if (attacksLeft > 0)
        {
            attacksLeft--;
            DoAttack();
            if (attacksLeft == 0)
            {
                VPE_DefOf.VPE_Assassinate_Return.PlayOneShot(pawn);
                TeleportPawnTo(originalPosition);
            }
        }
    }

    private void DoAttack()
    {
        var verb = pawn.meleeVerbs.GetUpdatedAvailableVerbsList(false).MaxBy(v => VerbUtility.DPS(v.verb, pawn)).verb;
        pawn.meleeVerbs.TryMeleeAttack(target, verb, true);
        pawn.stances.CancelBusyStanceHard();
        FleckMaker.AttachedOverlay(target, VPE_DefOf.VPE_Slash, Rand.InsideUnitCircle * 0.3f);
    }

    private void TeleportPawnTo(IntVec3 c)
    {
        var dataAttachedOverlay = FleckMaker.GetDataAttachedOverlay(pawn, FleckDefOf.PsycastSkipFlashEntry, Vector3.zero);
        dataAttachedOverlay.link.detachAfterTicks = 1;
        pawn.Map.flecks.CreateFleck(dataAttachedOverlay);
        TargetInfo dest = new(c, pawn.Map);
        FleckMaker.Static(dest.Cell, dest.Map, FleckDefOf.PsycastSkipInnerExit);
        FleckMaker.Static(dest.Cell, dest.Map, FleckDefOf.PsycastSkipOuterRingExit);
        SoundDefOf.Psycast_Skip_Entry.PlayOneShot(pawn);
        SoundDefOf.Psycast_Skip_Exit.PlayOneShot(dest);
        AddEffecterToMaintain(EffecterDefOf.Skip_EntryNoDelay.Spawn(pawn, pawn.Map), pawn.Position, 60);
        AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(dest.Cell, dest.Map), dest.Cell, 60);
        pawn.Position = c;
        pawn.Notify_Teleported();
    }

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        if (target.Pawn is { } targetPawn)
        {
            if (targetPawn.Map.glowGrid.GroundGlowAt(targetPawn.Position) <= 0.29f) return true;
            if (showMessages) Messages.Message("VPE.MustBeInDark".Translate(), MessageTypeDefOf.RejectInput, false);
        }

        return false;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref attacksLeft, nameof(attacksLeft));
        Scribe_Values.Look(ref originalPosition, nameof(originalPosition));
        Scribe_References.Look(ref target, nameof(target));
    }
}
