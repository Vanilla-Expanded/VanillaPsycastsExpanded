namespace VanillaPsycastsExpanded.Staticlord
{
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.Sound;

    public class Vortex : ThingWithComps
    {
        public const float RADIUS   = 18.9f;
        public const int   DURATION = 2500;

        private Sustainer sustainer;
        private int       startTick;

        public override void Tick()
        {
            base.Tick();
            if (this.sustainer == null) this.sustainer = VPE_DefOf.VPE_Vortex_Sustainer.TrySpawnSustainer(this);
            this.sustainer.Maintain();
            for (int i = 0; i < 3; i++)
            {
                FleckCreationData data = FleckMaker.GetDataStatic(this.RandomLocation(), this.Map, VPE_DefOf.VPE_VortexSpark);
                data.rotation = Rand.Range(0f, 360f);
                this.Map.flecks.CreateFleck(data);
                FleckMaker.ThrowSmoke(this.RandomLocation(), this.Map, 4f);
            }

            if (Find.TickManager.TicksGame - this.startTick > DURATION) this.Destroy();
            if (this.IsHashIntervalTick(30))
                foreach (Pawn pawn in GenRadial.RadialDistinctThingsAround(this.Position, this.Map, RADIUS, true).OfType<Pawn>())
                    if (pawn.RaceProps.IsMechanoid)
                        pawn.stances.stunner.StunFor(30, this, false);
                    else if (pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_Vortex) is null)
                    {
                        Hediff_Vortexed hediff = (Hediff_Vortexed) HediffMaker.MakeHediff(VPE_DefOf.VPE_Vortex, pawn);
                        hediff.Vortex = this;
                        pawn.health.AddHediff(hediff);
                    }
        }

        private Vector3 RandomLocation() =>
            this.DrawPos + new Vector3(Wrap(Mathf.Abs(Rand.Gaussian(0f, RADIUS)), RADIUS), 0f, 0f).RotatedBy(Rand.Range(0f, 360f));

        public static float Wrap(float x, float max)
        {
            while (x > max) x -= max;
            return x;
        }

        public override void Draw()
        {
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad) this.startTick = Find.TickManager.TicksGame;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            this.sustainer.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.startTick, nameof(this.startTick));
        }
    }

    public class Hediff_Vortexed : Hediff
    {
        public Vortex Vortex;

        public override HediffStage CurStage => this.Vortex is null
            ? base.CurStage
            : new HediffStage
            {
                capMods = new List<PawnCapacityModifier>
                {
                    new()
                    {
                        capacity = PawnCapacityDefOf.Moving,
                        setMax   = Mathf.Lerp(0.25f, 0.8f, this.pawn.Position.DistanceTo(this.Vortex.Position) / Vortex.RADIUS)
                    },
                    new()
                    {
                        capacity = PawnCapacityDefOf.Manipulation,
                        setMax   = Mathf.Lerp(0.25f, 0.8f, this.pawn.Position.DistanceTo(this.Vortex.Position) / Vortex.RADIUS)
                    }
                }
            };

        public override bool ShouldRemove => this.Vortex.Destroyed || this.pawn.Position.DistanceTo(this.Vortex.Position) >= Vortex.RADIUS;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.Vortex, "vortex");
        }
    }
}