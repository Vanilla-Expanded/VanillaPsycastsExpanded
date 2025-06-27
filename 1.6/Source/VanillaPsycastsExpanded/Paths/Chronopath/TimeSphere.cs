using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;
using VEF.Buildings;
using Ability = VEF.Abilities.Ability;

namespace VanillaPsycastsExpanded.Chronopath;

[StaticConstructorOnStartup]
public class TimeSphere : Thing
{
    private static readonly Material DistortionMat =
        DistortedMaterialsPool.DistortedMaterial("Things/Mote/Black", "Things/Mote/PsycastDistortionMask", 0.1f, 1.5f);

    public int Duration;

    public float Radius;

    private int startTick;
    private Sustainer sustainer;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (!respawningAfterLoad) startTick = Find.TickManager.TicksGame;
        foreach (var thing in GenRadial.RadialDistinctThingsAround(Position, Map, Radius, true))
            if (thing is Pawn { Faction: { IsPlayer: false } } pawn && !pawn.Faction.HostileTo(Faction.OfPlayer))
                pawn.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -75, true, true, HistoryEventDefOf.UsedHarmfulAbility);
    }

    protected override void Tick()
    {
        if (this.IsHashIntervalTick(60))
            foreach (var thing in GenRadial.RadialDistinctThingsAround(Position, Map, Radius, true))
            {
                if (thing is Pawn pawn) AbilityExtension_Age.Age(pawn, 1f);

                if (thing is Plant plant)
                {
                    if (plant.Growth < 1f) plant.Growth = 1f;
                    else if (plant.def.useHitPoints) thing.TakeDamage(new(DamageDefOf.Deterioration, 0.01f * thing.MaxHitPoints));
                    else plant.Age = int.MaxValue;
                }

                if (thing is Building && thing.def.useHitPoints) thing.TakeDamage(new(DamageDefOf.Deterioration, 0.01f * thing.MaxHitPoints));
            }

        if (sustainer == null) sustainer = VPE_DefOf.VPE_TimeSphere_Sustainer.TrySpawnSustainer(this);
        else sustainer.Maintain();

        if (Find.TickManager.TicksGame >= startTick + Duration) Destroy();
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        sustainer.End();
        base.Destroy(mode);
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        drawLoc.y = AltitudeLayer.MoteOverheadLow.AltitudeFor();
        UnityEngine.Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(drawLoc, Quaternion.AngleAxis(0f, Vector3.up), Vector3.one * Radius * 1.75f),
            DistortionMat, 0);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref Radius, "radius");
        Scribe_Values.Look(ref Duration, "duration");
        Scribe_Values.Look(ref startTick, nameof(startTick));
    }
}

public class Ability_TimeSphere : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (var target in targets)
        {
            var sphere = (TimeSphere)ThingMaker.MakeThing(VPE_DefOf.VPE_TimeSphere);
            sphere.Duration = GetDurationForPawn();
            sphere.Radius = GetRadiusForPawn();
            GenSpawn.Spawn(sphere, target.Cell, target.Map);
        }
    }
}
