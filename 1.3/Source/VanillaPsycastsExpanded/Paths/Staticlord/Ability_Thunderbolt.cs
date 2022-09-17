namespace VanillaPsycastsExpanded.Staticlord;

using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

public class Ability_Thunderbolt : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (GlobalTargetInfo target in targets)
        {
            foreach (Thing thing in target.Cell.GetThingList(target.Map).ListFullCopy())
                thing.TakeDamage(new DamageInfo(DamageDefOf.Flame, 25, -1f, this.pawn.DrawPos.AngleToFlat(thing.DrawPos), this.pawn));
            GenExplosion.DoExplosion(target.Cell, target.Map, this.GetRadiusForPawn(), DamageDefOf.EMP, this.pawn);
            this.pawn.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(this.pawn.Map, target.Cell));
        }
    }
}