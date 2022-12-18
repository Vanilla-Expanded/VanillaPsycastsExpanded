namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    using VFECore.Abilities;
    [StaticConstructorOnStartup]
    public class Hediff_BlizzardSource : Hediff_Overlay
    {
        private float curAngle;
        public override string OverlayPath => "Effects/Frostshaper/Blizzard/Blizzard";
        private List<Faction> affectedFactions;
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            base.pawn.Map.GetComponent<MapComponent_PsycastsManager>().blizzardSources.Add(this);
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            base.pawn.Map.GetComponent<MapComponent_PsycastsManager>().blizzardSources.Remove(this);
            var coma = HediffMaker.MakeHediff(VPE_DefOf.PsychicComa, pawn);
            coma.TryGetComp<HediffComp_Disappears>().ticksToDisappear = (int)((GenDate.TicksPerDay * 5) / pawn.GetStatValue(StatDefOf.PsychicSensitivity));
            pawn.health.AddHediff(coma);
        }
        public override void Tick()
        {
            base.Tick();
            Find.CameraDriver.shaker.DoShake(2f);
            curAngle += 0.07f;
            if (curAngle > 360)
            {
                curAngle = 0;
            }

            if (affectedFactions is null) affectedFactions = new List<Faction>();

            var cells = GenRadial.RadialCellsAround(pawn.Position, this.ability.GetAdditionalRadius(), this.ability.GetRadiusForPawn())
                .Where(x => x.InBounds(pawn.Map)).InRandomOrder().Take(Rand.RangeInclusive(9, 12)).ToList();
            foreach (var cell in cells)
            {
                pawn.Map.snowGrid.AddDepth(cell, 0.5f);
            }
            var affectedPawns = ability.pawn.Map.mapPawns.AllPawnsSpawned.ListFullCopy();
            foreach (Pawn victim in affectedPawns)
            {
                if (InAffectedArea(victim.Position))
                {
                    var blizzardHediff = victim.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_Blizzard);
                    if (blizzardHediff != null)
                    {
                        blizzardHediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 60;
                    }
                    else
                    {
                        blizzardHediff = HediffMaker.MakeHediff(VPE_DefOf.VPE_Blizzard, victim);
                        victim.health.AddHediff(blizzardHediff);
                    }
                    if (victim.IsHashIntervalTick(60) && victim.CanReceiveHypothermia(out var hediffDef))
                    {
                        HealthUtility.AdjustSeverity(victim, hediffDef, 0.02f);
                        DamageInfo dinfo = new DamageInfo(DamageDefOf.Cut, Rand.RangeInclusive(1, 3));
                        victim.TakeDamage(dinfo);
                    }
                    if (ability.pawn.Faction == Faction.OfPlayer)
                    {
                        AffectGoodwill(victim.HomeFaction, victim);
                    }
                }
            }
        }

        public bool InAffectedArea(IntVec3 cell)
        {
            return !cell.InHorDistOf(ability.pawn.Position, ability.GetAdditionalRadius())
                    && cell.InHorDistOf(ability.pawn.Position, ability.GetRadiusForPawn());
        }
        private void AffectGoodwill(Faction faction, Pawn p)
        {
            if (faction != null && !faction.IsPlayer && !faction.HostileTo(Faction.OfPlayer) && (p == null || !p.IsSlaveOfColony) 
                && (!affectedFactions.Contains(faction)))
            {
                Faction.OfPlayer.TryAffectGoodwillWith(faction, ability.def.goodwillImpact, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.UsedHarmfulAbility);
            }
        }

        public override void Draw()
        {
            Vector3 pos = pawn.DrawPos;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Matrix4x4 matrix = default(Matrix4x4);
            var drawSize = this.ability.GetRadiusForPawn() * 2f;
            matrix.SetTRS(pos, Quaternion.AngleAxis(curAngle, Vector3.up), new Vector3(drawSize, 1f, drawSize));
            UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, OverlayMat, 0, null, 0, MatPropertyBlock);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref curAngle, "curAngle");
        }
    }
}