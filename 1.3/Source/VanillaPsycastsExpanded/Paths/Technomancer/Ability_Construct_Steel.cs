namespace VanillaPsycastsExpanded.Technomancer;

using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

public class Ability_Construct_Steel : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (GlobalTargetInfo target in targets)
        {
            Pawn construct = PawnGenerator.GeneratePawn(VPE_DefOf.VPE_SteelConstruct, this.pawn.Faction);
            construct.TryGetComp<CompBreakLink>().Pawn = this.pawn;
            Thing thing = target.Thing;
            GenSpawn.Spawn(construct, thing.Position, thing.Map, thing.Rotation);
            thing.Destroy();
        }
    }

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        if (!base.ValidateTarget(target, showMessages)) return false;
        if (!target.HasThing) return false;
        if (target.Thing.def != ThingDefOf.ChunkSlagSteel)
        {
            if (showMessages) Messages.Message("VPE.MustBeSteelSlag".Translate(), MessageTypeDefOf.RejectInput, false);
            return false;
        }

        if (this.pawn.psychicEntropy.MaxEntropy - this.pawn.psychicEntropy.EntropyValue <= 20f)
        {
            if (showMessages) Messages.Message("VPE.NotEnoughHeat".Translate(), MessageTypeDefOf.RejectInput, false);
            return false;
        }

        return true;
    }
}