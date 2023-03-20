namespace VanillaPsycastsExpanded.Skipmaster;

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.Sound;
using VFECore;
using Ability = VFECore.Abilities.Ability;
public class Ability_Skipdoor : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (GlobalTargetInfo target in targets)
        {
            Skipdoor skipdoor = (Skipdoor)ThingMaker.MakeThing(VPE_DefOf.VPE_Skipdoor);
            skipdoor.Pawn = this.pawn;
            Find.WindowStack.Add(new Dialog_RenameDoorTeleporter(skipdoor));
            GenSpawn.Spawn(skipdoor, target.Cell, this.pawn.Map);
        }
    }
}