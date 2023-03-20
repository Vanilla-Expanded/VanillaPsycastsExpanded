namespace VanillaPsycastsExpanded.Chronopath;

using System.Linq;
using Graphics;
using Harmonist;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;
using VFECore;
using Ability = VFECore.Abilities.Ability;

[StaticConstructorOnStartup]
public class GameCondition_TimeQuake : GameCondition_TimeSnow
{
    private static readonly Material DistortionMat =
        DistortedMaterialsPool.DistortedMaterial("Things/Mote/Black", "Things/Mote/PsycastDistortionMask", 0.00001f, 1.0f);

    public  float     SafeRadius;
    public  Pawn      Pawn;
    private Sustainer sustainer;

    public override void GameConditionTick()
    {
        base.GameConditionTick();
        if (this.TicksPassed % 60 == 0)
            foreach (Map map in this.AffectedMaps)
            {
                for (int i = 0; i < 2000; i++) map.wildPlantSpawner.WildPlantSpawnerTick();

                foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned.Where(this.CanEffect).ToList())
                    AbilityExtension_Age.Age(pawn, 1f);

                foreach (Plant plant in map.listerThings.ThingsInGroup(ThingRequestGroup.Plant).OfType<Plant>().Where(this.CanEffect).ToList())
                    if (plant.Growth < 1f) plant.Growth = 1f;
                    else if (plant.def.useHitPoints) plant.TakeDamage(new DamageInfo(DamageDefOf.Rotting, 0.01f * plant.MaxHitPoints));
                    else plant.Age = int.MaxValue;

                foreach (Building building in map.listerBuildings.allBuildingsColonist
                                                 .Concat(map.listerBuildings.allBuildingsNonColonist)
                                                 .Where(this.CanEffect).ToList())
                    if (building.def.useHitPoints)
                        building.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 0.01f * building.MaxHitPoints));
            }

        if (this.TicksPassed % 300 == 250)
            foreach (Map map in this.AffectedMaps)
                Ability_RandomEvent.DoRandomEvent(map);

        if (this.sustainer == null) this.sustainer = VPE_DefOf.Psycast_Neuroquake_CastLoop.TrySpawnSustainer(this.Pawn);
        else this.sustainer.Maintain();

        Find.CameraDriver.shaker.DoShake(1.5f);
    }

    public override void End()
    {
        this.sustainer.End();
        VPE_DefOf.Psycast_Neuroquake_CastEnd.PlayOneShot(this.Pawn);
        base.End();
    }

    private bool CanEffect(Thing thing) => !thing.Position.InHorDistOf(this.Pawn.Position, this.SafeRadius);

    public override void GameConditionDraw(Map map)
    {
        base.GameConditionDraw(map);
        if (Find.Selector.IsSelected(this.Pawn)) GenDraw.DrawRadiusRing(this.Pawn.Position, this.SafeRadius, Color.yellow);

        Matrix4x4 matrix = Matrix4x4.TRS(this.Pawn.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead),
                                         Quaternion.AngleAxis(0f, Vector3.up), Vector3.one * this.SafeRadius * 2f);
        Graphics.DrawMesh(MeshPool.plane10, matrix, DistortionMat, 0);
    }
}

public class Ability_TimeQuake : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        GameCondition_TimeQuake condition = (GameCondition_TimeQuake)GameConditionMaker.MakeCondition(VPE_DefOf.VPE_TimeQuake, this.GetDurationForPawn());
        condition.SafeRadius = this.GetRadiusForPawn();
        condition.Pawn       = this.pawn;
        ThingWithComps skyChanger = (ThingWithComps)ThingMaker.MakeThing(VPE_DefOf.VPE_SkyChanger);
        GenSpawn.Spawn(skyChanger, this.pawn.Position, this.pawn.Map);
        skyChanger.TryGetComp<CompAffectsSky>().StartFadeInHoldFadeOut(0, this.GetDurationForPawn(), 0);
        this.pawn.Map.gameConditionManager.RegisterCondition(condition);
        foreach (Faction faction in Find.FactionManager.AllFactionsVisible)
        {
            if (faction.CanChangeGoodwillFor(this.pawn.Faction, -10))
                faction.TryAffectGoodwillWith(this.pawn.Faction, -10, reason: HistoryEventDefOf.UsedHarmfulAbility);

            if (faction.CanChangeGoodwillFor(this.pawn.Faction, -75) && this.pawn.Map.mapPawns.SpawnedPawnsInFaction(faction).Count > 0)
                faction.TryAffectGoodwillWith(this.pawn.Faction, -75, reason: HistoryEventDefOf.UsedHarmfulAbility);
        }
    }
}

public class ThoughtWorker_TimeQuake : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p) => p.Map.gameConditionManager.ConditionIsActive(VPE_DefOf.VPE_TimeQuake);
}