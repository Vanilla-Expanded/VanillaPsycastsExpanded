namespace VanillaPsycastsExpanded
{
    using Verse;
    using Verse.AI;

    public class ThinkNode_Construct : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn) => 
            pawn.def == VPE_DefOf.VPE_Race_RockConstruct;
    }
}