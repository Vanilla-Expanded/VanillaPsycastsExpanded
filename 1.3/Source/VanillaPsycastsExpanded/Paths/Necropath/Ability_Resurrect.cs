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
            if (!pawn.health.hediffSet.GetNotMissingParts().Where(x => x.def == VPE_DefOf.Finger).Any())
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
                if (pawn.health.hediffSet.GetNotMissingParts().Where(x => x.def == VPE_DefOf.Finger).TryRandomElement(out var finger))
                {
                    var corpse = target.Thing as Corpse;
                    var soul = SkyfallerMaker.MakeSkyfaller(VPE_DefOf.VPE_SoulFromSky) as SoulFromSky;
                    soul.target = corpse;
                    GenPlace.TryPlaceThing(soul, corpse.Position, corpse.Map, ThingPlaceMode.Direct);
                    var result = pawn.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 99999, instigator: this.pawn, hitPart: finger));
                }
            }
        }
    }
}