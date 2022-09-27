namespace VanillaPsycastsExpanded.Wildspeaker
{
    using RimWorld;
    using RimWorld.Planet;
    using Verse;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_ShrineShield : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            Map map = targets[0].Map;
            foreach (Thing thing in map.listerThings.ThingsOfDef(ThingDefOf.NatureShrine_Small))
                Ability_Spawn.Spawn(thing, VPE_DefOf.VPE_Shrineshield_Small, this);

            foreach (Thing thing in map.listerThings.ThingsOfDef(ThingDefOf.NatureShrine_Large))
                Ability_Spawn.Spawn(thing, VPE_DefOf.VPE_Shrineshield_Large, this);
        }
    }
}