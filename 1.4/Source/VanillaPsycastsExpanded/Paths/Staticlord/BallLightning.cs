using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded.Staticlord;

public class BallLightning : AbilityProjectile
{
    private const int WARMUP = 180;

    private List<Thing> currentTargets = new();
    private int ticksTillAttack = -1;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (!respawningAfterLoad) ticksTillAttack = WARMUP;
    }

    public override void Tick()
    {
        base.Tick();
        if (!Spawned) return;
        ticksTillAttack--;
        if (ticksTillAttack <= 0)
        {
            currentTargets.Clear();
            foreach (var thing in GenRadial.RadialDistinctThingsAround(ExactPosition.ToIntVec3(), Map, ability.GetRadiusForPawn(), true)
                        .Where(t => t.HostileTo(launcher))
                        .Take(Mathf.FloorToInt(ability.GetPowerForPawn())))
            {
                currentTargets.Add(thing);
                BattleLogEntry_RangedImpact logEntry =
                    new(launcher, thing, thing, def, VPE_DefOf.VPE_Bolt, targetCoverDef);
                thing.TakeDamage(new(DamageDefOf.Flame, 12f, 5f, DrawPos.AngleToFlat(thing.DrawPos), this)).AssociateWithLog(logEntry);
                thing.TakeDamage(new(DamageDefOf.EMP, 20f, 5f, DrawPos.AngleToFlat(thing.DrawPos), this)).AssociateWithLog(logEntry);
                VPE_DefOf.VPE_BallLightning_Zap.PlayOneShot(thing);
            }

            ticksTillAttack = 60;
        }
    }

    public override void Draw()
    {
        base.Draw();
        var a = DrawPos.Yto0() + new Vector3(1f, 0f, 0f).RotatedBy(origin.AngleToFlat(destination));
        var graphic = VPE_DefOf.VPE_ChainBolt.graphicData.Graphic;
        foreach (var thing in currentTargets)
        {
            var b = thing.DrawPos.Yto0();
            Vector3 s = new(graphic.drawSize.x, 1f, (b - a).magnitude);
            var matrix = Matrix4x4.TRS(a + (b - a) / 2 + Vector3.up * (def.Altitude - Altitudes.AltInc / 2), Quaternion.LookRotation(b - a), s);
            UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, graphic.MatSingle, 0);
        }
    }

    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        GenExplosion.DoExplosion(Position, Map, def.projectile.explosionRadius, def.projectile.damageDef, launcher,
            DamageAmount, ArmorPenetration, def.projectile.soundExplode, equipmentDef, def,
            intendedTarget.Thing, def.projectile.postExplosionSpawnThingDef, def.projectile.postExplosionSpawnChance,
            def.projectile.postExplosionSpawnThingCount, def.projectile.postExplosionGasType,
            def.projectile.applyDamageToExplosionCellsNeighbors, def.projectile.preExplosionSpawnThingDef,
            def.projectile.preExplosionSpawnChance,
            def.projectile.preExplosionSpawnThingCount, def.projectile.explosionChanceToStartFire,
            def.projectile.explosionDamageFalloff, origin.AngleToFlat(destination));
        base.Impact(hitThing, blockedByShield);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksTillAttack, nameof(ticksTillAttack));
        Scribe_Collections.Look(ref currentTargets, nameof(currentTargets), LookMode.Reference);
    }
}
