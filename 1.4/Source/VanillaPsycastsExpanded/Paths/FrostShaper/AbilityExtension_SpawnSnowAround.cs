namespace VanillaPsycastsExpanded
{
    using RimWorld.Planet;
    using Verse;
    using VFECore.Abilities;

    public class AbilityExtension_SpawnSnowAround : AbilityExtension_AbilityMod
    {
        public float radius;

        public float depth;

        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
            foreach (var target in targets)
            {
                foreach (var cell in GenRadial.RadialCellsAround(target.Cell, radius, true))
                {
                    ability.pawn.Map.snowGrid.AddDepth(cell, depth);
                }
            }
        }
    }
}