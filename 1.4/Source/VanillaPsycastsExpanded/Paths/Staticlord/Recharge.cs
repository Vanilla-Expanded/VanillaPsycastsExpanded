using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using VanillaPsycastsExpanded.Technomancer;
using Verse;
using Verse.Sound;
using VFECore.Shields;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded.Staticlord;

[StaticConstructorOnStartup]
public class HediffComp_Recharge : HediffComp_Draw
{
    private const float ChargePerTickMech = Building_MechCharger.ChargePerDay / GenDate.TicksPerDay;
    private const float ChargePerTickBattery = 10f / 3f;
    private CompPowerBattery compPower;
    private Building_MechCharger fakeCharger;
    private Need_MechEnergy needPower;
    private Sustainer sustainer;
    private Thing target;

    public void Init(Thing t)
    {
        target = t;
        compPower = t.TryGetComp<CompPowerBattery>();
        needPower = (t as Pawn)?.needs?.energy;
        if (needPower is { currentCharger: null }) needPower.currentCharger = fakeCharger ??= new Building_MechCharger();
    }

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        sustainer ??= VPE_DefOf.VPE_Recharge_Sustainer.TrySpawnSustainer(Pawn);
        sustainer?.Maintain();
        compPower?.AddEnergy(ChargePerTickBattery);
        if (needPower != null) needPower.CurLevel += ChargePerTickMech;
        if (needPower is { currentCharger: null }) needPower.currentCharger = fakeCharger ??= new Building_MechCharger();
    }

    public override void CompPostPostRemoved()
    {
        sustainer.End();
        if (needPower is { currentCharger: var c } && c == fakeCharger) needPower.currentCharger = null;
        fakeCharger = null;
        base.CompPostPostRemoved();
    }

    public override void DrawAt(Vector3 drawPos)
    {
        var b = target.TrueCenter();
        Vector3 s = new(Graphic.drawSize.x, 1f, (b - drawPos).magnitude);
        var matrix = Matrix4x4.TRS(drawPos + (b - drawPos) / 2, Quaternion.LookRotation(b - drawPos), s);
        UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, Graphic.MatSingle, 0);
    }

    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_References.Look(ref target, nameof(target));
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            compPower = target.TryGetComp<CompPowerBattery>();
            needPower = (target as Pawn)?.needs?.energy;
            if (needPower is { currentCharger: null }) needPower.currentCharger = fakeCharger ??= new Building_MechCharger();
        }
    }
}

public class Ability_Recharge : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (var target in targets)
        {
            var hediff = HediffMaker.MakeHediff(VPE_DefOf.VPE_Recharge, pawn);
            hediff.TryGetComp<HediffComp_Recharge>().Init(target.Thing);
            hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = GetDurationForPawn();
            pawn.health.AddHediff(hediff);
        }
    }

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        if (!base.ValidateTarget(target, showMessages)) return false;
        if (target.Thing?.TryGetComp<CompPowerBattery>() is { }) return true;

        if (ModsConfig.BiotechActive && target.Thing is Pawn { RaceProps.IsMechanoid: true, needs.energy: { } } p && p.IsMechAlly(pawn)) return true;

        if (showMessages) Messages.Message("VPE.MustTargetBattery".Translate(), MessageTypeDefOf.RejectInput, false);
        return false;
    }
}
