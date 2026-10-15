using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaPsycastsExpanded;

[StaticConstructorOnStartup]
public class Hediff_BlizzardSource : Hediff_Overlay
{
    private List<Faction> affectedFactions;
    private float curAngle;
    public override string OverlayPath => "Effects/Frostshaper/Blizzard/Blizzard";

    public override void PostAdd(DamageInfo? dinfo)
    {
        base.PostAdd(dinfo);
        pawn.Map.GetComponent<MapComponent_PsycastsManager>().blizzardSources.Add(this);
    }

    public override void PostRemoved()
    {
        base.PostRemoved();
        pawn.Map.GetComponent<MapComponent_PsycastsManager>().blizzardSources.Remove(this);
        ability?.def.GetModExtension<AbilityExtension_PsychicComa>()?.ApplyComa(ability);
    }

    public override void Tick()
    {
        base.Tick();
        Find.CameraDriver.shaker.DoShake(2f);
        curAngle += 0.07f;
        if (curAngle > 360) curAngle = 0;

        if (affectedFactions is null) affectedFactions = new();

        var cells = GenRadial.RadialCellsAround(pawn.Position, ability.GetAdditionalRadius(), ability.GetRadiusForPawn())
           .Where(x => x.InBounds(pawn.Map))
           .InRandomOrder()
           .Take(Rand.RangeInclusive(9, 12))
           .ToList();
        foreach (var cell in cells) pawn.Map.snowGrid.AddDepth(cell, 0.5f);
        var affectedPawns = ability.pawn.Map.mapPawns.AllPawnsSpawned.ToList();
        foreach (var victim in affectedPawns)
            if (InAffectedArea(victim.Position))
            {
                var blizzardHediff = victim.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_Blizzard);
                if (blizzardHediff != null)
                    blizzardHediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 60;
                else
                {
                    blizzardHediff = HediffMaker.MakeHediff(VPE_DefOf.VPE_Blizzard, victim);
                    victim.health.AddHediff(blizzardHediff);
                }

                if (victim.IsHashIntervalTick(60) && victim.CanReceiveHypothermia(out var hediffDef))
                {
                    HealthUtility.AdjustSeverity(victim, hediffDef, 0.02f);
                    var dinfo = new DamageInfo(DamageDefOf.Cut, Rand.RangeInclusive(1, 3));
                    victim.TakeDamage(dinfo);
                }

                if (ability.pawn.Faction == Faction.OfPlayer) AffectGoodwill(victim.HomeFaction, victim);
            }
    }

    public bool InAffectedArea(IntVec3 cell) =>
        !cell.InHorDistOf(ability.pawn.Position, ability.GetAdditionalRadius())
     && cell.InHorDistOf(ability.pawn.Position, ability.GetRadiusForPawn());

    private void AffectGoodwill(Faction faction, Pawn p)
    {
        if (faction != null && !faction.IsPlayer && !faction.HostileTo(Faction.OfPlayer) && (p == null || !p.IsSlaveOfColony)
         && !affectedFactions.Contains(faction))
            Faction.OfPlayer.TryAffectGoodwillWith(faction, ability.def.goodwillImpact, true, true, HistoryEventDefOf.UsedHarmfulAbility);
    }

    public override void Draw()
    {
        var pos = pawn.DrawPos;
        pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
        var matrix = default(Matrix4x4);
        var drawSize = ability.GetRadiusForPawn() * 2f;
        matrix.SetTRS(pos, Quaternion.AngleAxis(curAngle, Vector3.up), new(drawSize, 1f, drawSize));
        UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, OverlayMat, 0, null, 0, MatPropertyBlock);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref curAngle, "curAngle");
    }
}
