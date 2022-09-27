namespace VanillaPsycastsExpanded.Chronopath;

using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

public class Ability_MaturePlants : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (Plant plant in targets.SelectMany(target => GenRadial.RadialDistinctThingsAround(target.Cell, target.Map, this.GetRadiusForPawn(), true))
                                       .OfType<Plant>().Distinct())
        {
            plant.Growth = 1f;
            plant.DirtyMapMesh(plant.Map);
        }
    }
}