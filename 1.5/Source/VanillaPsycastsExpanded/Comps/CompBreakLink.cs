namespace VanillaPsycastsExpanded;

using System.Collections.Generic;
using AnimalBehaviours;
using UnityEngine;
using Verse;

public class CompBreakLink : ThingComp, PawnGizmoProvider
{
    public Pawn                     Pawn;
    public CompProperties_BreakLink Props => this.props as CompProperties_BreakLink;

    public IEnumerable<Gizmo> GetGizmos()
    {
        yield return new Command_Action
        {
            defaultLabel = this.Props.gizmoLabel.Translate(),
            defaultDesc  = this.Props.gizmoDesc.Translate(),
            icon         = ContentFinder<Texture2D>.Get(this.Props.gizmoImage),
            action       = delegate { this.parent.Kill(); }
        };
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (this.parent is IMinHeatGiver giver) this.Pawn.Psycasts().AddMinHeatGiver(giver);
    }

    public override void CompTick()
    {
        base.CompTick();
        if (this.Pawn is { Dead: true } or { Destroyed: true } or null) this.parent.Kill();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref this.Pawn, "pawn");
    }
}