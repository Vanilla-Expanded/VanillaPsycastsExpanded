namespace VanillaPsycastsExpanded.Conflagrator;

using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

[StaticConstructorOnStartup]
public class Ability_FireTornado : Ability
{
    private static readonly ThingDef FireTornadoDef = ThingDef.Named("VPE_FireTornado");

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (GlobalTargetInfo target in targets)
        {
            FireTornado tornado = (FireTornado)GenSpawn.Spawn(FireTornadoDef, target.Cell, target.Map);
            tornado.ticksLeftToDisappear = this.GetDurationForPawn();
        }
    }
}