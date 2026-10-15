namespace VanillaPsycastsExpanded
{
    using RimWorld.Planet;
    using System.Linq;
    using Ability = VEF.Abilities.Ability;

    public class Ability_WordOf : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            var groupLinkMaster = this.pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_GroupLink) as Hediff_GroupLink;
            if (groupLinkMaster != null)
            {
                var targetsLink = targets.ToList();
                foreach (var linkedPawn in groupLinkMaster.linkedPawns)
                {
                    if (targetsLink.Any(x => x.Thing == linkedPawn) is false)
                    {
                        targetsLink.Add(linkedPawn);
                    }
                }
                base.Cast(targetsLink.ToArray());
            }
            else
            {
                base.Cast(targets);
            }
        }
    }
}