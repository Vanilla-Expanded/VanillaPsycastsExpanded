namespace VanillaPsycastsExpanded.Nightstalker
{
    using RimWorld;
    using RimWorld.Planet;
    using Skipmaster;

    public class Ability_WorldTeleportNight : Ability_WorldTeleport
    {
        public override bool CanHitTargetTile(GlobalTargetInfo target) => GenLocalDate.HourFloat(target.Tile) is < 6f or > 18f;
    }
}