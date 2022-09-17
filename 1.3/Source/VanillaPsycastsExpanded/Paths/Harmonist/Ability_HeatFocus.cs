namespace VanillaPsycastsExpanded.Harmonist;

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Ability = VFECore.Abilities.Ability;

public class Ability_HeatFocus : Ability
{
    private static readonly AccessTools.FieldRef<Pawn_PsychicEntropyTracker, float> currentEntropy =
        AccessTools.FieldRefAccess<Pawn_PsychicEntropyTracker, float>("currentEntropy");

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        float amount = Mathf.Min(1f - this.pawn.psychicEntropy.CurrentPsyfocus,
                                 (this.pawn.psychicEntropy.EntropyValue - this.pawn.GetStatValue(VPE_DefOf.VPE_PsychicEntropyMinimum)) * 0.002f);
        this.pawn.psychicEntropy.OffsetPsyfocusDirectly(amount);
        currentEntropy(this.pawn.psychicEntropy) -= amount * 500f;
    }
}