namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    using Verse.AI;
    using Ability = VFECore.Abilities.Ability;
    public class Ability_ParalysisPulse : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            MoteMaker.MakeAttachedOverlay(pawn, VPE_DefOf.VPE_Mote_ParalysisPulse, Vector3.zero, this.GetRadiusForPawn());
        }
    }
}