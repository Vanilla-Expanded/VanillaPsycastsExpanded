using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded.Harmonist;

public class HediffComp_MindControl : HediffComp_Ability
{
    private Faction oldFaction;
    private Lord oldLord;

    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        base.CompPostPostAdd(dinfo);
        oldFaction = Pawn.Faction;
        oldLord = Pawn.GetLord();
        oldLord?.RemovePawn(Pawn);
        Pawn.SetFaction(ability.pawn.Faction, ability.pawn);
    }

    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
        Pawn.SetFaction(oldFaction);
        if (!oldFaction.IsPlayer && oldLord is not { AnyActivePawn: true })
        {
            if (Pawn.Map.mapPawns.SpawnedPawnsInFaction(oldFaction).Except(Pawn).Any())
                oldLord = ((Pawn)GenClosest.ClosestThing_Global(Pawn.Position, Pawn.Map.mapPawns.SpawnedPawnsInFaction(oldFaction),
                    99999f,
                    p => p != Pawn && ((Pawn)p).GetLord() != null)).GetLord();

            if (oldLord == null)
            {
                LordJob_DefendPoint lordJob = new(Pawn.Position);
                oldLord = LordMaker.MakeNewLord(oldFaction, lordJob, Pawn.Map);
            }
        }

        oldLord?.AddPawn(Pawn);
    }

    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_References.Look(ref oldFaction, nameof(oldFaction));
        Scribe_References.Look(ref oldLord, nameof(oldLord));
    }
}
