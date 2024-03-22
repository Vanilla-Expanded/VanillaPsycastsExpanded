namespace VanillaPsycastsExpanded
{
    using RimWorld;
    using RimWorld.Planet;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    using Verse.AI;
    using VFECore.Abilities;
    public class Ability_Resurrect : Ability_TargetCorpse
    {
        public override Gizmo GetGizmo()
        {
            var gizmo = base.GetGizmo();
            var fingerParts = pawn.health.hediffSet.GetNotMissingParts().Where(x => x.def == VPE_DefOf.Finger);

            if (fingerParts.All(finger => pawn.health.hediffSet.hediffs
            .Any(hediff => hediff.def == VPE_DefOf.VPE_Sacrificed && hediff.Part == finger)))
            {
                gizmo.Disable("VPE.NoAvailableFingers".Translate());
            }
            return gizmo;
        }
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            foreach (var target in targets)
            {
                var fingerParts = pawn.health.hediffSet.GetNotMissingParts().Where(x => x.def == VPE_DefOf.Finger);
                if (fingerParts.Where(finger => pawn.health.hediffSet.hediffs
                    .Any(hediff => hediff.def == VPE_DefOf.VPE_Sacrificed && hediff.Part == finger) is false)
                    .TryRandomElement(out var finger))
                {
                    var corpse = target.Thing as Corpse;
                    var soul = SkyfallerMaker.MakeSkyfaller(VPE_DefOf.VPE_SoulFromSky) as SoulFromSky;
                    soul.target = corpse;
                    GenPlace.TryPlaceThing(soul, corpse.Position, corpse.Map, ThingPlaceMode.Direct);
                    pawn.health.AddHediff(HediffMaker.MakeHediff(VPE_DefOf.VPE_Sacrificed, pawn, finger), finger);
                }
            }
        }
    }
}