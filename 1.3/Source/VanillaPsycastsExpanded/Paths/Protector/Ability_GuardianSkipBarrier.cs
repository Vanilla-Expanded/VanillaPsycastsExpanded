namespace VanillaPsycastsExpanded;

using Verse;
using VFECore.Abilities;

public class Ability_GuardianSkipBarrier : Ability, IChannelledPsycast
{
    public bool IsActive => this.pawn.health.hediffSet.HasHediff(VPE_DefOf.VPE_GuardianSkipBarrier);

    public override Gizmo GetGizmo()
    {
        Hediff hediff = this.pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_GuardianSkipBarrier);
        if (hediff != null)
            return new Command_Action
            {
                defaultLabel = "VPE.CancelSkipbarrier".Translate(),
                defaultDesc  = "VPE.CancelSkipbarrierDesc".Translate(),
                icon         = this.def.icon,
                action       = delegate { this.pawn.health.RemoveHediff(hediff); },
                order        = 10f + (this.def.requiredHediff?.hediffDef?.index ?? 0) + (this.def.requiredHediff?.minimumLevel ?? 0)
            };
        return base.GetGizmo();
    }
}