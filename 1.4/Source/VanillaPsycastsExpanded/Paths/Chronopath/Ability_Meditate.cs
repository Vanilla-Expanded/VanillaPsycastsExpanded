namespace VanillaPsycastsExpanded.Chronopath;

using RimWorld.Planet;
using VFECore.Abilities;

public class Ability_Meditate : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        this.pawn.psychicEntropy.OffsetPsyfocusDirectly(1f - this.pawn.psychicEntropy.CurrentPsyfocus);
        this.pawn.Psycasts().GainExperience(300f);
    }
}