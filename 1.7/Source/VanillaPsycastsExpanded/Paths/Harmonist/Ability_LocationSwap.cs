namespace VanillaPsycastsExpanded.Harmonist
{
    using RimWorld.Planet;
    using Skipmaster;

    public class Ability_LocationSwap : Ability_Teleport
    {
        public override void ModifyTargets(ref GlobalTargetInfo[] targets)
        {
            targets = new[]
            {
                targets[0],
                new GlobalTargetInfo(this.pawn.Position, this.pawn.Map),
                this.pawn,
                new GlobalTargetInfo(targets[0].Cell, targets[0].Map)
            };
        }
    }
}