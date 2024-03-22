namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    using VFECore.Abilities;
    [StaticConstructorOnStartup]
    public class Hediff_PsychicDrone : Hediff_Overlay
    {
        private float curAngle;
        public override string OverlayPath => "Effects/Archotechist/PsychicDrone/PsychicDroneEnergyField";
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            maintainedMotes.Add(SpawnMoteAttached(VPE_DefOf.VPE_PsycastAreaEffectMaintained, this.ability.GetRadiusForPawn(), 0));
        }

        private List<Mote> maintainedMotes = new List<Mote>();

        private List<Pawn> affectedPawns = new List<Pawn>();
        public override void Tick()
        {
            base.Tick();
            curAngle += 0.015f;
            if (curAngle > 360)
            {
                curAngle = 0;
            }
            foreach (Mote maintainedMote in maintainedMotes)
            {
                maintainedMote.Maintain();
            }
            if (Find.TickManager.TicksGame % 180 == 0)
            {
                if (GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, this.ability.GetRadiusForPawn(), true)
                    .OfType<Pawn>().Where(x => !affectedPawns.Contains(x) && !x.InMentalState && x.HostileTo(pawn) 
                        && x.RaceProps.IsFlesh).TryRandomElement(out var victim))
                {
                    var mentalStateDef = Rand.Bool ? VPE_DefOf.VPE_Wander_Sad : MentalStateDefOf.Berserk;
                    if (victim.mindState.mentalStateHandler.TryStartMentalState(mentalStateDef, causedByPsycast: true))
                    {
                        affectedPawns.Add(victim);
                    }
                }
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
        public Mote SpawnMoteAttached(ThingDef moteDef, float scale, float rotationRate)
        {
            MoteAttachedScaled mote = MoteMaker.MakeAttachedOverlay(pawn, moteDef, Vector3.zero) as MoteAttachedScaled;
            mote.maxScale = scale;
            mote.rotationRate = rotationRate;
            if (mote.def.mote.needsMaintenance)
            {
                mote.Maintain();
            }
            return mote;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref affectedPawns, "affectedPawns", LookMode.Reference);
            Scribe_Collections.Look(ref maintainedMotes, "maintainedMotes", LookMode.Reference);
        }
    }
}