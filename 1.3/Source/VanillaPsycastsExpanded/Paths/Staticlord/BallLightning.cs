namespace VanillaPsycastsExpanded.Staticlord
{
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.Sound;

    public class BallLightning : Projectile
    {
        private const int   WARMUP = 180;
        private const float RADIUS = 5f;

        private List<Thing> currentTargets  = new();
        private int         ticksTillAttack = -1;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad) this.ticksTillAttack = WARMUP;
        }

        public override void Tick()
        {
            base.Tick();
            this.ticksTillAttack--;
            if (this.ticksTillAttack <= 0)
            {
                this.currentTargets.Clear();
                foreach (Thing thing in GenRadial.RadialDistinctThingsAround(this.ExactPosition.ToIntVec3(), this.Map, RADIUS, true)
                                                 .Where(t => t.HostileTo(this.launcher)))
                {
                    this.currentTargets.Add(thing);
                    BattleLogEntry_RangedImpact logEntry =
                        new(this.launcher, thing, thing, this.def, VPE_DefOf.VPE_Bolt, this.targetCoverDef);
                    thing.TakeDamage(new DamageInfo(DamageDefOf.Flame, 12f, 5f, this.DrawPos.AngleToFlat(thing.DrawPos), this)).AssociateWithLog(logEntry);
                    thing.TakeDamage(new DamageInfo(DamageDefOf.EMP,   20f, 5f, this.DrawPos.AngleToFlat(thing.DrawPos), this)).AssociateWithLog(logEntry);
                    VPE_DefOf.VPE_BallLightning_Zap.PlayOneShot(thing);
                }

                this.ticksTillAttack = 60;
            }
        }

        public override void Draw()
        {
            base.Draw();
            Vector3 a       = this.DrawPos.Yto0() + new Vector3(1f, 0f, 0f).RotatedBy(this.origin.AngleToFlat(this.destination));
            Graphic graphic = VPE_DefOf.VPE_ChainBolt.graphicData.Graphic;
            foreach (Thing thing in this.currentTargets)
            {
                Vector3   b      = thing.DrawPos.Yto0();
                Vector3   s      = new(graphic.drawSize.x, 1f, (b - a).magnitude);
                Matrix4x4 matrix = Matrix4x4.TRS(a + (b - a) / 2 + Vector3.up * (this.def.Altitude - Altitudes.AltInc / 2), Quaternion.LookRotation(b - a), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, graphic.MatSingle, 0);
            }
        }

        protected override void Impact(Thing hitThing)
        {
            GenExplosion.DoExplosion(this.Position, this.Map, this.def.projectile.explosionRadius, this.def.projectile.damageDef, this.launcher,
                                     this.DamageAmount, this.ArmorPenetration, this.def.projectile.soundExplode, this.equipmentDef, this.def,
                                     this.intendedTarget.Thing, this.def.projectile.postExplosionSpawnThingDef, this.def.projectile.postExplosionSpawnChance,
                                     this.def.projectile.postExplosionSpawnThingCount,
                                     this.def.projectile.applyDamageToExplosionCellsNeighbors, this.def.projectile.preExplosionSpawnThingDef,
                                     this.def.projectile.preExplosionSpawnChance,
                                     this.def.projectile.preExplosionSpawnThingCount, this.def.projectile.explosionChanceToStartFire,
                                     this.def.projectile.explosionDamageFalloff, this.origin.AngleToFlat(this.destination));
            base.Impact(hitThing);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.ticksTillAttack, nameof(this.ticksTillAttack));
            Scribe_Collections.Look(ref this.currentTargets, nameof(this.currentTargets), LookMode.Reference);
        }
    }
}