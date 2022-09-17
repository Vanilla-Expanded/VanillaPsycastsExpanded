namespace VanillaPsycastsExpanded.Technomancer;

using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using VFECore.Shields;
using Ability = VFECore.Abilities.Ability;

[StaticConstructorOnStartup]
public class HediffComp_InfinitePower : HediffComp_Draw
{
    private static readonly Material OVERLAY = MaterialPool.MatFrom("Effects/Technomancer/Power/InfinitePowerOverlay", ShaderDatabase.MetaOverlay);

    private Thing           target;
    private CompPowerTrader compPower;

    public override bool CompShouldRemove => this.target is null or { Spawned: false };

    public void Begin(Thing t)
    {
        this.target    = t;
        this.compPower = t.TryGetComp<CompPowerTrader>();
    }

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        this.compPower.PowerOn     = true;
        this.compPower.PowerOutput = 0f;
    }

    public override void DrawAt(Vector3 drawPos)
    {
        Graphics.DrawMesh(MeshPool.plane10,
                          Matrix4x4.TRS(this.target.DrawPos.Yto0() + Vector3.up * AltitudeLayer.MetaOverlays.AltitudeFor(),
                                        Quaternion.AngleAxis(0f, Vector3.up), Vector3.one), OVERLAY, 0);
    }

    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
        this.compPower.SetUpPowerVars();
    }

    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_References.Look(ref this.target, nameof(this.target));
        if (Scribe.mode == LoadSaveMode.PostLoadInit) this.compPower = this.target.TryGetComp<CompPowerTrader>();
    }
}

public class Ability_Power : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (GlobalTargetInfo target in targets) this.ApplyHediff(this.pawn)?.TryGetComp<HediffComp_InfinitePower>().Begin(target.Thing);
    }

    public override Hediff ApplyHediff(Pawn targetPawn, HediffDef hediffDef, BodyPartRecord bodyPart, int duration, float severity)
    {
        Hediff localHediff = HediffMaker.MakeHediff(hediffDef, targetPawn, bodyPart);
        if (localHediff is Hediff_Ability hediffAbility)
            hediffAbility.ability = this;
        if (severity > float.Epsilon)
            localHediff.Severity = severity;
        if (localHediff is HediffWithComps hwc)
            foreach (HediffComp hediffComp in hwc.comps)
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
        if (target.Thing?.TryGetComp<CompPowerTrader>() is not { PowerOutput: < 0f })
        {
            if (showMessages) Messages.Message("VPE.MustConsumePower".Translate(), MessageTypeDefOf.RejectInput, false);

            return false;
        }

        return true;
    }
}