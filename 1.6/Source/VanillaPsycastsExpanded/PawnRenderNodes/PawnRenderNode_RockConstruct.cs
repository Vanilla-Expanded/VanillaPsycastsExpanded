
using RimWorld;
using UnityEngine;
using Verse;
namespace VanillaPsycastsExpanded
{
    public class PawnRenderNode_RockConstruct : PawnRenderNode_AnimalPart
    {
        public PawnRenderNode_RockConstruct(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            if (pawn.TryGetComp(out CompSetStoneColour comp))
            {
                Graphic graphic = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.Graphic;
                return GraphicDatabase.Get<Graphic_Multi>(graphic.path, ShaderDatabase.Cutout, graphic.drawSize, comp.color);
            }
            return base.GraphicFor(pawn);
        }
    }
}