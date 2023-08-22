namespace VanillaPsycastsExpanded.Graphics
{
    using RimWorld;
    using System.Collections.Generic;
    using UnityEngine;
    using VanillaPsycastsExpanded.HarmonyPatches;
    using Verse;

    public class HediffComp_MoteOverHead : HediffComp
    {
        private Mote mote;
        private static readonly List<Vector3> animalHeadOffsets = new List<Vector3>
        {
            new Vector3(0f, 0f, 0.4f),
            new Vector3(0.4f, 0f, 0.25f),
            new Vector3(0f, 0f, 0.1f),
            new Vector3(-0.4f, 0f, 0.25f)
        };
        public HediffCompProperties_Mote Props => this.props as HediffCompProperties_Mote;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            var pawn = Pawn;
            if (this.Pawn.Spawned)
            {
                bool humanlike = pawn.RaceProps.Humanlike;
                List<Vector3> headPosPerRotation = pawn.RaceProps.headPosPerRotation;
                Rot4 rotation = ((pawn.GetPosture() != 0) ?
                    pawn.Drawer.renderer.LayingFacing() : (humanlike ? Rot4.North 
                    : pawn.Rotation));
                Vector3 vector;
                if (humanlike)
                {
                    vector = (pawn.Drawer.renderer.BaseHeadOffsetAt(rotation) + new Vector3(0, 0, 0.15f)).RotatedBy(pawn.Drawer.renderer.BodyAngle());
                }
                else
                {
                    float bodySizeFactor = pawn.ageTracker.CurLifeStage.bodySizeFactor;
                    Vector2 vector2 = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize * bodySizeFactor;
                    vector = ((!headPosPerRotation.NullOrEmpty()) ? headPosPerRotation[rotation.AsInt].ScaledBy(new Vector3(vector2.x, 1f, vector2.y)) : (animalHeadOffsets[rotation.AsInt] * pawn.BodySize));
                }
                vector = pawn.DrawPos + vector;
                if (this.mote == null || this.mote.Destroyed)
                {
                    this.mote = MakeStaticMote(vector, this.Props.mote);
                    this.mote.Graphic.MatSingle.color = this.Props.color;
                }
                else
                {
                    this.mote.exactPosition = vector;
                    this.mote.Maintain();
                }
            }
        }

        public Mote MakeStaticMote(Vector3 loc, ThingDef moteDef, float scale = 1f)
        {
            Mote obj = (Mote)ThingMaker.MakeThing(moteDef);
            obj.exactPosition = loc;
            obj.Scale = scale;
            GenSpawn.Spawn(obj, Pawn.Position, Pawn.Map);
            return obj;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            if (this.mote != null && this.mote.def.CanBeSaved() is false) 
            {
                return;
            }
            Scribe_References.Look(ref this.mote, nameof(this.mote));
        }
    }

    public class HediffCompProperties_Mote : HediffCompProperties
    {
        public ThingDef mote;
        public Color    color;
    }
}