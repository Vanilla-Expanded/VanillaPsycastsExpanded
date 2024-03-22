using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using VFECore.Shields;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded.Technomancer;

[StaticConstructorOnStartup]
public class HediffComp_InfinitePower : HediffComp_Draw
{
    private static readonly Material OVERLAY = MaterialPool.MatFrom("Effects/Technomancer/Power/InfinitePowerOverlay", ShaderDatabase.MetaOverlay);
    private CompPowerTrader compPower;
    private Building_MechCharger fakeCharger;
    private Need_MechEnergy needPower;

    private Thing target;

    public override bool CompShouldRemove => base.CompShouldRemove || target is null or { Spawned: false };

    public void Begin(Thing t)
    {
        target = t;
        compPower = t.TryGetComp<CompPowerTrader>();
        needPower = (t as Pawn)?.needs?.energy;
        if (needPower is { currentCharger: null }) needPower.currentCharger = fakeCharger ??= new Building_MechCharger();
    }

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (compPower != null)
        {
            compPower.PowerOn = true;
            compPower.PowerOutput = 0f;
        }

        if (needPower is { currentCharger: null }) needPower.currentCharger = fakeCharger ??= new Building_MechCharger();
    }

    public override void DrawAt(Vector3 drawPos)
    {
        UnityEngine.Graphics.DrawMesh(MeshPool.plane10,
            Matrix4x4.TRS(target.DrawPos.Yto0() + Vector3.up * AltitudeLayer.MetaOverlays.AltitudeFor(),
                Quaternion.AngleAxis(0f, Vector3.up), Vector3.one), OVERLAY, 0);
    }

    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
        compPower?.SetUpPowerVars();
        if (needPower is { currentCharger: var charger } && charger == fakeCharger) needPower.currentCharger = null;
        fakeCharger = null;
    }

    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_References.Look(ref target, nameof(target));
        Scribe_References.Look(ref fakeCharger, nameof(fakeCharger));
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            compPower = target.TryGetComp<CompPowerTrader>();
            needPower = (target as Pawn)?.needs?.energy;
            if (needPower is { currentCharger: null }) needPower.currentCharger = fakeCharger ??= new Building_MechCharger();
        }
    }
}

public class Ability_Power : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (var target in targets) ApplyHediff(pawn)?.TryGetComp<HediffComp_InfinitePower>().Begin(target.Thing);
    }

    public override Hediff ApplyHediff(Pawn targetPawn, HediffDef hediffDef, BodyPartRecord bodyPart, int duration, float severity)
    {
        var localHediff = HediffMaker.MakeHediff(hediffDef, targetPawn, bodyPart);
        if (localHediff is Hediff_Ability hediffAbility)
            hediffAbility.ability = this;
        if (severity > float.Epsilon)
            localHediff.Severity = severity;
        if (localHediff is HediffWithComps hwc)
            foreach (var hediffComp in hwc.comps)
            {
                if (hediffComp is HediffComp_Ability hca)
                    hca.ability = this;
                if (hediffComp is HediffComp_Disappears hcd)
                    hcd.ticksToDisappear = duration;
            }

        targetPawn.health.AddHediff(localHediff);
        return localHediff;
    }

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        if (!base.ValidateTarget(target, showMessages)) return false;
        if (target.Thing?.TryGetComp<CompPowerTrader>() is { PowerOutput: < 0f }) return true;

        if (ModsConfig.BiotechActive && target.Thing is Pawn { RaceProps.IsMechanoid: true, needs.energy: { } } p && p.IsMechAlly(pawn)) return true;

        if (showMessages) Messages.Message("VPE.MustConsumePower".Translate(), MessageTypeDefOf.RejectInput, false);
        return false;
    }
}
