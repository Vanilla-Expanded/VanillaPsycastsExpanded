using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaPsycastsExpanded.Staticlord;

public class Vortex : ThingWithComps
{
    public const float RADIUS = 18.9f;
    public const int DURATION = 2500;
    private int startTick;

    private Sustainer sustainer;

    public override void Tick()
    {
        base.Tick();
        if (sustainer == null) sustainer = VPE_DefOf.VPE_Vortex_Sustainer.TrySpawnSustainer(this);
        sustainer.Maintain();
        for (var i = 0; i < 3; i++)
        {
            var data = FleckMaker.GetDataStatic(RandomLocation(), Map, VPE_DefOf.VPE_VortexSpark);
            data.rotation = Rand.Range(0f, 360f);
            Map.flecks.CreateFleck(data);
            FleckMaker.ThrowSmoke(RandomLocation(), Map, 4f);
        }

        if (Find.TickManager.TicksGame - startTick > DURATION) Destroy();
        if (this.IsHashIntervalTick(30))
            foreach (var pawn in GenRadial.RadialDistinctThingsAround(Position, Map, RADIUS, true).OfType<Pawn>())
                if (pawn.RaceProps.IsMechanoid)
                    pawn.stances.stunner.StunFor(30, this, false);
                else if (pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_Vortex) is null)
                {
                    var hediff = (Hediff_Vortexed)HediffMaker.MakeHediff(VPE_DefOf.VPE_Vortex, pawn);
                    hediff.Vortex = this;
                    pawn.health.AddHediff(hediff);
                }
    }

    private Vector3 RandomLocation() => DrawPos + new Vector3(Wrap(Mathf.Abs(Rand.Gaussian(0f, RADIUS)), RADIUS), 0f, 0f).RotatedBy(Rand.Range(0f, 360f));

    public static float Wrap(float x, float max)
    {
        while (x > max) x -= max;
        return x;
    }

    public override void Draw() { }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (!respawningAfterLoad) startTick = Find.TickManager.TicksGame;
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        base.DeSpawn(mode);
        sustainer.End();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref startTick, nameof(startTick));
    }
}

public class Hediff_Vortexed : Hediff
{
    public Vortex Vortex;

    public override HediffStage CurStage =>
        Vortex is null
            ? base.CurStage
            : new()
            {
                capMods = new()
                {
                    new()
                    {
                        capacity = PawnCapacityDefOf.Moving,
                        setMax = Mathf.Lerp(0.5f, 0.9f, pawn.Position.DistanceTo(Vortex.Position) / Vortex.RADIUS)
                    },
                    new()
                    {
                        capacity = PawnCapacityDefOf.Manipulation,
                        setMax = Mathf.Lerp(0.5f, 0.9f, pawn.Position.DistanceTo(Vortex.Position) / Vortex.RADIUS)
                    }
                }
            };

    public override bool ShouldRemove => Vortex.Destroyed || pawn.Position.DistanceTo(Vortex.Position) >= Vortex.RADIUS;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref Vortex, "vortex");
    }
}
