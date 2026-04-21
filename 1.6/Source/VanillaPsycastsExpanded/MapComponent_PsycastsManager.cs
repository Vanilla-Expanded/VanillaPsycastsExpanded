namespace VanillaPsycastsExpanded
{
    using System.Collections.Generic;
    using Verse;

    public class MapComponent_PsycastsManager : MapComponent
    {
        public List<FixedTemperatureZone> temperatureZones = new();

        public List<Hediff_BlizzardSource> blizzardSources = new();
        public List<Hediff_Overlay>        hediffsToDraw   = new();

        public MapComponent_PsycastsManager(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            for (int i = this.temperatureZones.Count - 1; i >= 0; i--)
            {
                FixedTemperatureZone zone = this.temperatureZones[i];
                if (Find.TickManager.TicksGame >= zone.expiresIn)
                    this.temperatureZones.RemoveAt(i);
                else
                    zone.DoEffects(this.map);
            }
        }

        public bool TryGetOverridenTemperatureFor(IntVec3 cell, out float result)
        {
            for (var i = 0; i < this.temperatureZones.Count; i++)
            {
                var coldZone = this.temperatureZones[i];
                if (cell.DistanceToSquared(coldZone.center) <= coldZone.radius * coldZone.radius)
                {
                    result = coldZone.fixedTemperature;
                    return true;
                }
            }

            for (var i = 0; i < this.blizzardSources.Count; i++)
            {
                var hediff = this.blizzardSources[i];
                var radius = hediff.ability.GetRadiusForPawn();
                if (cell.DistanceTo(hediff.pawn.Position) <= radius * radius)
                {
                    result = -60; // hardcoded for now, doesn't matter much
                    return true;
                }
            }

            result = -1f;
            return false;
        }

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            for (var i = this.hediffsToDraw.Count - 1; i >= 0; i--)
            {
                var hediff = this.hediffsToDraw[i];
                if (hediff.pawn is null || hediff.pawn.health.hediffSet.hediffs.Contains(hediff) is false)
                    this.hediffsToDraw.RemoveAt(i);
                else if (hediff.pawn?.MapHeld != null)
                    hediff.Draw();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref this.temperatureZones, "temperatureZones", LookMode.Deep);
            Scribe_Collections.Look(ref this.blizzardSources,  "blizzardSources",  LookMode.Reference);
            Scribe_Collections.Look(ref this.hediffsToDraw,    "hediffsToDraw",    LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.temperatureZones ??= new List<FixedTemperatureZone>();
                this.blizzardSources  ??= new List<Hediff_BlizzardSource>();
                this.hediffsToDraw    ??= new List<Hediff_Overlay>();

                this.temperatureZones.RemoveAll(x => x is null);
                this.blizzardSources.RemoveAll(x => x is null);
                this.hediffsToDraw.RemoveAll(x => x is null);
            }
        }
    }
}