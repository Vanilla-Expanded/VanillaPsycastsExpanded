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
    public class Ability_ReunionFarskip : Ability
    {
        private List<Mote> maintainedMotes = new List<Mote>();

        public override void PreWarmupAction()
        {
            base.PreWarmupAction();
            var map = this.pawn.Map;
            var mote = SpawnMote(map, VPE_DefOf.VPE_Mote_GreenMist, pawn.Position.ToVector3Shifted(), 10f, 20f);
            maintainedMotes = new List<Mote>();
            maintainedMotes.Add(mote);
            var cells = GenRadial.RadialCellsAround(pawn.Position, 3, true).Where(x => x.InBounds(map)).ToList();
            for (var i = 0; i < 5; i++)
            {
                if (cells.Any())
                {
                    var cell = cells.RandomElement();
                    cells.Remove(cell);
                    var ghost = SpawnMote(map, ThingDef.Named("VPE_Mote_Ghost" + "ABCDEFG".RandomElement()), cell.ToVector3Shifted(), 1f, 0f);
                    maintainedMotes.Add(ghost);
                }
            }
        }

        public override void WarmupToil(Toil toil)
        {
            base.WarmupToil(toil);
            toil.AddPreTickAction(delegate
            {
                foreach (Mote maintainedMote in maintainedMotes)
                {
                    maintainedMote.Maintain();
                }
            });
        }

        public List<Pawn> GetLivingFamilyMembers(Pawn pawn)
        {
            return pawn.relations.FamilyByBlood.Where(x => !x.Dead).ToList();
        }
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var target = targets[0].Thing as Pawn;
            var familyMembers = GetLivingFamilyMembers(target);
            var cells = GenRadial.RadialCellsAround(pawn.Position, 3, true).Where(x => x.InBounds(pawn.Map) && x.Walkable(pawn.Map)).ToList();
            foreach (var member in familyMembers)
            {
                GenSpawn.Spawn(member, cells.RandomElement(), pawn.Map);
                var hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, member);
                BodyPartRecord result = null;
                member.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource).TryRandomElement(out result);
                member.health.AddHediff(hediff, result);
            }
            var factions = familyMembers.Select(x => x.Faction).Where(x => x != null).Distinct();
            foreach (var faction in factions)
            {
                faction.TryAffectGoodwillWith(pawn.Faction, -10);
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (target.Thing is Pawn targetPawn && targetPawn.relations != null)
            {
                if (!GetLivingFamilyMembers(targetPawn).Any())
                {
                    if (showMessages)
                    {
                        Messages.Message("VPE.MustHaveLivingFamilyMembers".Translate(targetPawn.Named("PAWN")), targetPawn,
                            MessageTypeDefOf.RejectInput, historical: false);
                    }
                    return false;
                }
            }
            return base.ValidateTarget(target, showMessages);
        }

        public Mote SpawnMote(Map map, ThingDef moteDef, Vector3 loc, float scale, float rotationRate)
        {
            Mote mote = MoteMaker.MakeStaticMote(loc, map, moteDef, scale);
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
            Scribe_Collections.Look(ref maintainedMotes, "maintainedMotes", LookMode.Reference);
        }
    }
}