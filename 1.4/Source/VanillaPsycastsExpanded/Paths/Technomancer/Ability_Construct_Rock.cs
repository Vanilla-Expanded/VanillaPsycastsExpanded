namespace VanillaPsycastsExpanded.Technomancer;

using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

[StaticConstructorOnStartup]
public class Ability_Construct_Rock : Ability
{
    private static readonly HashSet<ThingDef> chunkCache = new();

    static Ability_Construct_Rock()
    {
        foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            if (def.IsNonResourceNaturalRock)
                if (def.building.mineableThing != null)
                    chunkCache.Add(def.building.mineableThing);
    }

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (GlobalTargetInfo target in targets)
        {
            Pawn construct = PawnGenerator.GeneratePawn(VPE_DefOf.VPE_RockConstruct, this.pawn.Faction);
            construct.TryGetComp<CompBreakLink>().Pawn = this.pawn;
            Thing thing = target.Thing;
            GenSpawn.Spawn(construct, thing.Position, thing.Map, thing.Rotation);
            construct.TryGetComp<CompSetStoneColour>().SetStoneColour(thing.def);
            thing.Destroy();
        }
    }

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        if (!base.ValidateTarget(target, showMessages)) return false;
        if (!target.HasThing) return false;
        if (!chunkCache.Contains(target.Thing.def))
        {
            if (showMessages) Messages.Message("VPE.MustBeStoneChunk".Translate(), MessageTypeDefOf.RejectInput, false);
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