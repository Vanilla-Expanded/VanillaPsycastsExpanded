using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded.Chronopath;

public class Ability_MaturePlants : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (var plant in targets.SelectMany(target => GenRadial.RadialDistinctThingsAround(target.Cell, target.Map, GetRadiusForPawn(), true))
                    .OfType<Plant>()
                    .Distinct())
        {
            plant.Growth += plant.GrowthRate * (3.5f / plant.def.plant.growDays);
            plant.DirtyMapMesh(plant.Map);
        }
    }
}
