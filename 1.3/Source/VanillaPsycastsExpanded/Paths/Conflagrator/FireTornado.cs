namespace VanillaPsycastsExpanded.Conflagrator
{
    using System.Linq;
    using HarmonyLib;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.Noise;
    using Verse.Sound;

    [StaticConstructorOnStartup]
    [HarmonyPatch]
    public class FireTornado : ThingWithComps
    {
        private static readonly MaterialPropertyBlock matPropertyBlock = new();

        private static readonly Material TornadoMaterial =
            MaterialPool.MatFrom("Effects/Conflagrator/FireTornado/FireTornadoFat", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.Tornado);

        private static readonly FloatRange PartsDistanceFromCenter = new(1f, 5f);
        private static readonly float      ZOffsetBias             = -4f * PartsDistanceFromCenter.min;
        private static readonly FleckDef   FireTornadoPuff         = DefDatabase<FleckDef>.GetNamed("VPE_FireTornadoDustPuff");
        private static          ModuleBase directionNoise;

        private Vector2 realPosition;
        private float   direction;

        private int spawnTick;
        private int leftFadeOutTicks     = -1;
        public  int ticksLeftToDisappear = -1;

        private Sustainer sustainer;

        private float FadeInOutFactor
        {
            get
            {
                float a = Mathf.Clamp01((Find.TickManager.TicksGame - this.spawnTick) / 120f);
                float b = this.leftFadeOutTicks < 0 ? 1f : Mathf.Min(this.leftFadeOutTicks / 120f, 1f);
                return Mathf.Min(a, b);
            }
        }

        [HarmonyPatch(typeof(SnowUtility), nameof(SnowUtility.AddSnowRadial))]
        [HarmonyPrefix]
        public static void FixSnowUtility(ref float radius)
        {
            if (radius > GenRadial.MaxRadialPatternRadius) radius = GenRadial.MaxRadialPatternRadius - 1f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.realPosition,         "realPosition");
            Scribe_Values.Look(ref this.direction,            "direction");
            Scribe_Values.Look(ref this.spawnTick,            "spawnTick");
            Scribe_Values.Look(ref this.leftFadeOutTicks,     "leftFadeOutTicks");
            Scribe_Values.Look(ref this.ticksLeftToDisappear, "ticksLeftToDisappear");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                Vector3 vector = this.Position.ToVector3Shifted();
                this.realPosition     = new Vector2(vector.x, vector.z);
                this.direction        = Rand.Range(0f, 360f);
                this.spawnTick        = Find.TickManager.TicksGame;
                this.leftFadeOutTicks = -1;
            }

            this.CreateSustainer();
        }

        public static void ThrowPuff(Vector3 loc, Map map, float scale, Color color)
        {
            if (!loc.ShouldSpawnMotesAt(map)) return;

            FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, FireTornadoPuff, 1.9f * scale);
            dataStatic.rotationRate  = Rand.Range(-60,  60);
            dataStatic.velocityAngle = Rand.Range(0,    360);
            dataStatic.velocitySpeed = Rand.Range(0.6f, 0.75f);
            dataStatic.instanceColor = color;
            map.flecks.CreateFleck(dataStatic);
        }

        public override void Tick()
        {
            if (this.Spawned)
            {
                if (this.sustainer == null)
                {
                    Log.Error("Tornado sustainer is null.");
                    this.CreateSustainer();
                }

                this.sustainer.Maintain();
                this.UpdateSustainerVolume();
                this.GetComp<CompWindSource>().wind = 5f * this.FadeInOutFactor;
                if (this.leftFadeOutTicks > 0)
                {
                    this.leftFadeOutTicks--;
                    if (this.leftFadeOutTicks == 0) this.Destroy();
                }
                else
                {
                    if (directionNoise == null) directionNoise = new Perlin(0.0020000000949949026, 2.0, 0.5, 4, 1948573612, QualityMode.Medium);
                    this.direction    += (float) directionNoise.GetValue(Find.TickManager.TicksAbs, this.thingIDNumber % 500 * 1000f, 0.0) * 0.78f;
                    this.realPosition =  this.realPosition.Moved(this.direction, 0.0283333343f);
                    IntVec3 intVec = new Vector3(this.realPosition.x, 0f, this.realPosition.y).ToIntVec3();
                    if (intVec.InBounds(this.Map))
                    {
                        this.Position = intVec;
                        if (this.IsHashIntervalTick(15)) this.DoFire();
                        if (this.IsHashIntervalTick(60)) this.SpawnChemfuel();
                        if (this.ticksLeftToDisappear > 0)
                        {
                            this.ticksLeftToDisappear--;
                            if (this.ticksLeftToDisappear == 0)
                            {
                                this.leftFadeOutTicks = 120;
                                Messages.Message("MessageTornadoDissipated".Translate(), new TargetInfo(this.Position, this.Map),
                                                 MessageTypeDefOf.PositiveEvent);
                            }
                        }

                        if (this.IsHashIntervalTick(4) && !this.CellImmuneToDamage(this.Position))
                        {
                            float num = Rand.Range(0.6f, 1f);
                            ThrowPuff(new Vector3(this.realPosition.x, 0f, this.realPosition.y)
                            {
                                y = AltitudeLayer.MoteOverhead.AltitudeFor()
                            } + Vector3Utility.RandomHorizontalOffset(1.5f), this.Map, Rand.Range(1.5f, 3f), new Color(num, num, num));
                        }
                    }
                    else
                    {
                        this.leftFadeOutTicks = 120;
                        Messages.Message("MessageTornadoLeftMap".Translate(), new TargetInfo(this.Position, this.Map), MessageTypeDefOf.PositiveEvent);
                    }
                }
            }
        }

        private void DoFire()
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(this.Position, 4.2f, true).Where(c => c.InBounds(this.Map) && !this.CellImmuneToDamage(c))
                                              .InRandomOrder()
                                              .Take(Rand.Range(3, 5)))
            {
                Fire fire = cell.GetFirstThing<Fire>(this.Map);
                if (fire is null)
                    FireUtility.TryStartFireIn(cell, this.Map, 15f);
                else
                    fire.fireSize += 1f;
            }

            foreach (Pawn pawn in GenRadial.RadialDistinctThingsAround(this.Position, this.Map, 4.2f, true).OfType<Pawn>())
            {
                Fire fire = (Fire) pawn.GetAttachment(ThingDefOf.Fire);
                if (fire is null)
                    pawn.TryAttachFire(15f);
                else
                    fire.fireSize += 1f;
            }
        }

        private void SpawnChemfuel()
        {
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(this.Position, 4.2f, true)
                                              .Where(c => c.InBounds(this.Map) && FilthMaker.CanMakeFilth(c, this.Map, ThingDefOf.Filth_Fuel) &&
                                                          !this.CellImmuneToDamage(c))
                                              .InRandomOrder()
                                              .Take(Rand.Range(1, 3)))
                FilthMaker.TryMakeFilth(cell, this.Map, ThingDefOf.Filth_Fuel);
        }

        public override void Draw()
        {
            Rand.PushState();
            Rand.Seed = this.thingIDNumber;
            for (int i = 0; i < 90; i++)
                this.DrawTornadoPart(PartsDistanceFromCenter.RandomInRange, Rand.Range(0f, 360f), Rand.Range(0.9f, 1.1f), Rand.Range(0.52f, 0.88f));
            Rand.PopState();
        }

        private void DrawTornadoPart(float distanceFromCenter, float initialAngle, float speedMultiplier, float colorMultiplier)
        {
            int     ticksGame = Find.TickManager.TicksGame;
            float   num       = 1f                                / distanceFromCenter;
            float   num2      = 25f                               * speedMultiplier * num;
            float   num3      = (initialAngle + ticksGame * num2) % 360f;
            Vector2 vector    = this.realPosition.Moved(num3, AdjustedDistanceFromCenter(distanceFromCenter));
            vector.y += distanceFromCenter * 4f;
            vector.y += ZOffsetBias;
            Vector3 a    = new(vector.x, AltitudeLayer.Weather.AltitudeFor() + 0.04054054f * Rand.Range(0f, 1f), vector.y);
            float   num4 = distanceFromCenter * 3f;
            float   num5 = 1f;
            if (num3 > 270f)
                num5                   = GenMath.LerpDouble(270f, 360f, 0f, 1f, num3);
            else if (num3 > 180f) num5 = GenMath.LerpDouble(180f, 270f, 1f, 0f, num3);
            float   num6  = Mathf.Min(distanceFromCenter / (PartsDistanceFromCenter.max + 2f), 1f);
            float   d     = Mathf.InverseLerp(0.18f, 0.4f, num6);
            Vector3 a2    = new(Mathf.Sin(ticksGame / 1000f + this.thingIDNumber * 10) * 2f, 0f, 0f);
            Vector3 pos   = a + a2 * d;
            float   a3    = Mathf.Max(1f - num6, 0f) * num5 * this.FadeInOutFactor;
            Color   value = new(colorMultiplier, colorMultiplier, colorMultiplier, a3);
            matPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
            Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0f, num3, 0f), new Vector3(num4, 1f, num4));
            Graphics.DrawMesh(MeshPool.plane10, matrix, TornadoMaterial, 0, null, 0, matPropertyBlock);
        }

        private static float AdjustedDistanceFromCenter(float distanceFromCenter)
        {
            float num = Mathf.Min(distanceFromCenter / 8f, 1f);
            num *= num;
            return distanceFromCenter * num;
        }

        private void UpdateSustainerVolume()
        {
            this.sustainer.info.volumeFactor = this.FadeInOutFactor;
        }

        private void CreateSustainer()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                SoundDef tornado = SoundDefOf.Tornado;
                this.sustainer = tornado.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
                this.UpdateSustainerVolume();
            });
        }

        private bool CellImmuneToDamage(IntVec3 c)
        {
            if (c.Roofed(this.Map) && c.GetRoof(this.Map).isThickRoof) return true;
            Building edifice = c.GetEdifice(this.Map);
            return edifice != null && edifice.def.category == ThingCategory.Building &&
                   (edifice.def.building.isNaturalRock || edifice.def == ThingDefOf.Wall && edifice.Faction == null);
        }
    }
}