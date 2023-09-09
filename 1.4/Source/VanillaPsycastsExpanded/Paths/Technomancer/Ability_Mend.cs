using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace VanillaPsycastsExpanded.Technomancer;

public class Ability_Mend : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        foreach (var target in targets)
            if (target.Thing is Pawn p)
            {
                if (p.RaceProps.Humanlike)
                {
                    var amountTotal = GetPowerForPawn();
                    var toHeal = p.equipment.AllEquipmentListForReading.Concat(p.apparel.WornApparel)
                       .Where(t => t.def.useHitPoints && t.HitPoints < t.MaxHitPoints)
                       .ToList();
                    var maxIterations = (int)amountTotal * toHeal.Count;
                    var i = 0;
                    while (amountTotal >= 1f && toHeal.Count > 0 && i++ <= maxIterations)
                        toHeal.RemoveAll(t =>
                        {
                            amountTotal -= Mend(t, amountTotal >= toHeal.Count ? amountTotal / toHeal.Count : amountTotal);
                            return t.HitPoints == t.MaxHitPoints;
                        });

                    if (i >= maxIterations) Log.Warning($"[VPE] Too many iterations in Ability_Mend.Cast by {pawn} on {p}");
                }
                else if (p.RaceProps.IsMechanoid)
                {
                    var amountTotal = GetPowerForPawn();
                    var toHeal = p.health.hediffSet.hediffs.OfType<Hediff_Injury>().Where(h => !h.IsPermanent()).ToList();
                    var maxIterations = (int)amountTotal * toHeal.Count;
                    var i = 0;
                    while (amountTotal >= 0 && toHeal.Count > 0 && i++ <= maxIterations)
                        toHeal.RemoveAll(injury =>
                        {
                            var amount = Mathf.Clamp(amountTotal >= 1f ? amountTotal / toHeal.Count : amountTotal, 0, injury.Severity);
                            injury.Heal(amount);
                            amountTotal -= amount;
                            return injury.Severity == 0f;
                        });

                    if (i >= maxIterations) Log.Warning($"[VPE] Too many iterations in Ability_Mend.Cast by {pawn} on {p}");

                    if (toHeal.Count == 0) MechRepairUtility.RepairTick(p);
                }
            }
            else
                Mend(target.Thing, GetPowerForPawn());
    }

    public override float GetPowerForPawn() => (pawn.GetStatValue(StatDefOf.PsychicSensitivity) - 1) * 100f;

    private static int Mend(Thing t, int amount)
    {
        var old = t.HitPoints;
        t.HitPoints = Mathf.Clamp(t.HitPoints + amount, t.HitPoints, t.MaxHitPoints);
        return t.HitPoints - old;
    }

    private static float Mend(Thing t, float amount) => Mend(t, (int)amount) + (amount - (int)amount);

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        if (!base.ValidateTarget(target, showMessages)) return false;
        if (target.Thing is Pawn p)
        {
            if (p.RaceProps.Humanlike)
            {
                if (p.Faction != pawn.Faction)
                {
                    if (showMessages) Messages.Message("VPE.MustBeAlly".Translate(), MessageTypeDefOf.RejectInput, false);

                    return false;
                }

                if (!p.equipment.AllEquipmentListForReading.Any(t => t.def.useHitPoints && t.HitPoints < t.MaxHitPoints) &&
                    !p.apparel.WornApparel.Any(t => t.def.useHitPoints && t.HitPoints < t.MaxHitPoints))
                {
                    if (showMessages) Messages.Message("VPE.MustHaveDamagedEquipment".Translate(), MessageTypeDefOf.RejectInput, false);

                    return false;
                }

                return true;
            }

            if (p.RaceProps.IsMechanoid)
            {
                if (!ModsConfig.BiotechActive || !p.IsMechAlly(pawn))
                {
                    if (showMessages) Messages.Message("VPE.MustBeAlly".Translate(), MessageTypeDefOf.RejectInput, false);

                    return false;
                }

                if (!MechRepairUtility.CanRepair(p))
                {
                    if (showMessages) Messages.Message("VPE.MustBeDamaged".Translate(), MessageTypeDefOf.RejectInput, false);

                    return false;
                }

                return true;
            }

            if (showMessages) Messages.Message("VPE.NoAnimals".Translate(), MessageTypeDefOf.RejectInput, false);

            return false;
        }

        if (target.Thing is { def.useHitPoints: true } t)
        {
            if (t.HitPoints < t.MaxHitPoints) return true;

            if (showMessages) Messages.Message("VPE.MustBeDamaged".Translate(), MessageTypeDefOf.RejectInput, false);

            return false;
        }

        return false;
    }
}
