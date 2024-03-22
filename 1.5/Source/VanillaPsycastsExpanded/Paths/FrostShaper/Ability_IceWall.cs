namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using System.Linq;
    using Verse;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;
    public class Ability_IceWall : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var cells = GenRadial.RadialCellsAround(targets[0].Cell, 5, 5.9f);
            AbilityExtension_Building extension = this.def.GetModExtension<AbilityExtension_Building>();
            foreach (var cell in cells)
            {
                if (cell.GetEdifice(this.pawn.Map) is null)
                {
                    Thing building = GenSpawn.Spawn(extension.building, cell, this.pawn.Map, WipeMode.VanishOrMoveAside);
                    building.SetFactionDirect(this.pawn.Faction);

                }
            }
        }
    }
}