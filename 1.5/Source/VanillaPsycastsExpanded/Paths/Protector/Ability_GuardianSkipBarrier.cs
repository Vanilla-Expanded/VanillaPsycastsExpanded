using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class Ability_GuardianSkipBarrier : Ability, IChannelledPsycast
{
    public bool IsActive => pawn.health.hediffSet.HasHediff(VPE_DefOf.VPE_GuardianSkipBarrier);

    public override Gizmo GetGizmo()
    {
        var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_GuardianSkipBarrier);
        if (hediff != null)
            return new Command_Action
            {
                defaultLabel = "VPE.CancelSkipbarrier".Translate(),
                defaultDesc = "VPE.CancelSkipbarrierDesc".Translate(),
                icon = def.icon,
                action = delegate { pawn.health.RemoveHediff(hediff); },
                Order = 10f + (def.requiredHediff?.hediffDef?.index ?? 0) + (def.requiredHediff?.minimumLevel ?? 0)
            };
        return base.GetGizmo();
    }
}