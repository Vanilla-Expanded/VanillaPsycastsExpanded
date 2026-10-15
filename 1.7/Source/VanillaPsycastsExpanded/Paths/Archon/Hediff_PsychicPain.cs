namespace VanillaPsycastsExpanded;

using RimWorld;
using UnityEngine;
using Verse;

public class Hediff_PsychicPain : HediffWithComps
{
    public override float PainOffset => Mathf.Max(this.pawn.GetStatValue(StatDefOf.PsychicSensitivity) - 0.8f, 0f);
}