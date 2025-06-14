using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace VanillaPsycastsExpanded.Conflagrator;

[StaticConstructorOnStartup]
[HarmonyPatch]
public class FireTornado : ThingWithComps
{
    private static readonly MaterialPropertyBlock matPropertyBlock = new();

    private static readonly Material TornadoMaterial =
        MaterialPool.MatFrom("Effects/Conflagrator/FireTornado/FireTornadoFat", ShaderDatabase.MoteGlow, MapMaterialRenderQueues.Tornado);

    private static readonly FloatRange PartsDistanceFromCenter = new(1f, 5f);
    private static readonly float ZOffsetBias = -4f * PartsDistanceFromCenter.min;
    private static readonly FleckDef FireTornadoPuff = DefDatabase<FleckDef>.GetNamed("VPE_FireTornadoDustPuff");
    private static ModuleBase directionNoise;
    public int ticksLeftToDisappear = -1;
    private float direction;
    private int leftFadeOutTicks = -1;

    private Vector2 realPosition;

    private int spawnTick;

    private Sustainer sustainer;

    private float FadeInOutFactor
    {
        get
        {
            var a = Mathf.Clamp01((Find.TickManager.TicksGame - spawnTick) / 120f);
            var b = leftFadeOutTicks < 0 ? 1f : Mathf.Min(leftFadeOutTicks / 120f, 1f);
            return Mathf.Min(a, b);
        }
    }

    [HarmonyPatch(typeof(WeatherBuildupUtility), nameof(WeatherBuildupUtility.AddSnowRadial))]
    [HarmonyPrefix]
    public static void FixSnowUtility(ref float radius)
    {
        if (radius > GenRadial.MaxRadialPatternRadius) radius = GenRadial.MaxRadialPatternRadius - 1f;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref realPosition, "realPosition");
        Scribe_Values.Look(ref direction, "direction");
        Scribe_Values.Look(ref spawnTick, "spawnTick");
        Scribe_Values.Look(ref leftFadeOutTicks, "leftFadeOutTicks");
        Scribe_Values.Look(ref ticksLeftToDisappear, "ticksLeftToDisappear");
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (!respawningAfterLoad)
        {
            var vector = Position.ToVector3Shifted();
            realPosition = new(vector.x, vector.z);
            direction = Rand.Range(0f, 360f);
            spawnTick = Find.TickManager.TicksGame;
            leftFadeOutTicks = -1;
        }

        CreateSustainer();
    }

    public static void ThrowPuff(Vector3 loc, Map map, float scale, Color color)
    {
        if (!loc.ShouldSpawnMotesAt(map)) return;

        var dataStatic = FleckMaker.GetDataStatic(loc, map, FireTornadoPuff, 1.9f * scale);
        dataStatic.rotationRate = Rand.Range(-60, 60);
        dataStatic.velocityAngle = Rand.Range(0, 360);
        dataStatic.velocitySpeed = Rand.Range(0.6f, 0.75f);
        dataStatic.instanceColor = color;
        map.flecks.CreateFleck(dataStatic);
    }

    protected override void Tick()
    {
        if (Spawned)
        {
            if (sustainer == null)
            {
                Log.Error("Tornado sustainer is null.");
                CreateSustainer();
            }

            sustainer.Maintain();
            UpdateSustainerVolume();
            GetComp<CompWindSource>().wind = 5f * FadeInOutFactor;
            if (leftFadeOutTicks > 0)
            {
                leftFadeOutTicks--;
                if (leftFadeOutTicks == 0) Destroy();
            }
            else
            {
                if (directionNoise == null) directionNoise = new Perlin(0.0020000000949949026, 2.0, 0.5, 4, 1948573612, QualityMode.Medium);
                direction += (float)directionNoise.GetValue(Find.TickManager.TicksAbs, thingIDNumber % 500 * 1000f, 0.0) * 0.78f;
                realPosition = realPosition.Moved(direction, 0.0283333343f);
                var intVec = new Vector3(realPosition.x, 0f, realPosition.y).ToIntVec3();
                if (intVec.InBounds(Map))
                {
                    Position = intVec;
                    if (this.IsHashIntervalTick(15)) DoFire();
                    if (this.IsHashIntervalTick(60)) SpawnChemfuel();
                    if (ticksLeftToDisappear > 0)
                    {
                        ticksLeftToDisappear--;
                        if (ticksLeftToDisappear == 0)
                        {
                            leftFadeOutTicks = 120;
                            Messages.Message("MessageTornadoDissipated".Translate(), new TargetInfo(Position, Map),
                                MessageTypeDefOf.PositiveEvent);
                        }
                    }

                    if (this.IsHashIntervalTick(4) && !CellImmuneToDamage(Position))
                    {
                        var num = Rand.Range(0.6f, 1f);
                        ThrowPuff(new Vector3(realPosition.x, 0f, realPosition.y)
                        {
                            y = AltitudeLayer.MoteOverhead.AltitudeFor()
                        } + Vector3Utility.RandomHorizontalOffset(1.5f), Map, Rand.Range(1.5f, 3f), new(num, num, num));
                    }
                }
                else
                {
                    leftFadeOutTicks = 120;
                    Messages.Message("MessageTornadoLeftMap".Translate(), new TargetInfo(Position, Map), MessageTypeDefOf.PositiveEvent);
                }
            }
        }
    }

    private void DoFire()
    {
        foreach (var cell in GenRadial.RadialCellsAround(Position, 4.2f, true)
                    .Where(c => c.InBounds(Map) && !CellImmuneToDamage(c))
                    .InRandomOrder()
                    .Take(Rand.Range(3, 5)))
        {
            var fire = cell.GetFirstThing<Fire>(Map);
            if (fire is null)
                FireUtility.TryStartFireIn(cell, Map, 15f, this);
            else
                fire.fireSize += 1f;
        }

        foreach (var pawn in GenRadial.RadialDistinctThingsAround(Position, Map, 4.2f, true).OfType<Pawn>())
        {
            var fire = (Fire)pawn.GetAttachment(ThingDefOf.Fire);
            if (fire is null)
                pawn.TryAttachFire(15f, this);
            else
                fire.fireSize += 1f;
        }
    }

    private void SpawnChemfuel()
    {
        foreach (var cell in GenRadial.RadialCellsAround(Position, 4.2f, true)
                    .Where(c => c.InBounds(Map) && FilthMaker.CanMakeFilth(c, Map, ThingDefOf.Filth_Fuel) &&
                                !CellImmuneToDamage(c))
                    .InRandomOrder()
                    .Take(Rand.Range(1, 3)))
            FilthMaker.TryMakeFilth(cell, Map, ThingDefOf.Filth_Fuel);
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        Rand.PushState();
        Rand.Seed = thingIDNumber;
        for (var i = 0; i < 90; i++)
            DrawTornadoPart(PartsDistanceFromCenter.RandomInRange, Rand.Range(0f, 360f), Rand.Range(0.9f, 1.1f), Rand.Range(0.52f, 0.88f));
        Rand.PopState();
    }

    private void DrawTornadoPart(float distanceFromCenter, float initialAngle, float speedMultiplier, float colorMultiplier)
    {
        var ticksGame = Find.TickManager.TicksGame;
        var num = 1f / distanceFromCenter;
        var num2 = 25f * speedMultiplier * num;
        var num3 = (initialAngle + ticksGame * num2) % 360f;
        var vector = realPosition.Moved(num3, AdjustedDistanceFromCenter(distanceFromCenter));
        vector.y += distanceFromCenter * 4f;
        vector.y += ZOffsetBias;
        Vector3 a = new(vector.x, AltitudeLayer.Weather.AltitudeFor() + 0.04054054f * Rand.Range(0f, 1f), vector.y);
        var num4 = distanceFromCenter * 3f;
        var num5 = 1f;
        if (num3 > 270f)
            num5 = GenMath.LerpDouble(270f, 360f, 0f, 1f, num3);
        else if (num3 > 180f) num5 = GenMath.LerpDouble(180f, 270f, 1f, 0f, num3);
        var num6 = Mathf.Min(distanceFromCenter / (PartsDistanceFromCenter.max + 2f), 1f);
        var d = Mathf.InverseLerp(0.18f, 0.4f, num6);
        Vector3 a2 = new(Mathf.Sin(ticksGame / 1000f + thingIDNumber * 10) * 2f, 0f, 0f);
        var pos = a + a2 * d;
        var a3 = Mathf.Max(1f - num6, 0f) * num5 * FadeInOutFactor;
        Color value = new(colorMultiplier, colorMultiplier, colorMultiplier, a3);
        matPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
        var matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0f, num3, 0f), new(num4, 1f, num4));
        UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, TornadoMaterial, 0, null, 0, matPropertyBlock);
    }

    private static float AdjustedDistanceFromCenter(float distanceFromCenter)
    {
        var num = Mathf.Min(distanceFromCenter / 8f, 1f);
        num *= num;
        return distanceFromCenter * num;
    }

    private void UpdateSustainerVolume()
    {
        sustainer.info.volumeFactor = FadeInOutFactor;
    }

    private void CreateSustainer()
    {
        LongEventHandler.ExecuteWhenFinished(delegate
        {
            var tornado = SoundDefOf.Tornado;
            sustainer = tornado.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
            UpdateSustainerVolume();
        });
    }

    private bool CellImmuneToDamage(IntVec3 c)
    {
        if (c.Roofed(Map) && c.GetRoof(Map).isThickRoof) return true;
        var edifice = c.GetEdifice(Map);
        return edifice != null && edifice.def.category == ThingCategory.Building &&
               (edifice.def.building.isNaturalRock || (edifice.def == ThingDefOf.Wall && edifice.Faction == null));
    }
}
