namespace VanillaPsycastsExpanded.Chronopath;

using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

[HarmonyPatch]
public class GameCondition_RaidPause : GameCondition_TimeSnow
{
    private Sustainer sustainer;

    public override void GameConditionTick()
    {
        base.GameConditionTick();

        if (this.TicksPassed % 60 == 0)
            foreach (Map map in this.AffectedMaps)
            foreach (Pawn pawn in map.attackTargetsCache.TargetsHostileToColony.OfType<Pawn>())
                pawn.stances.stunner.StunFor(61, null, false);

        if (this.sustainer == null) this.sustainer = VPE_DefOf.VPE_RaidPause_Sustainer.TrySpawnSustainer(SoundInfo.OnCamera());
        else this.sustainer.Maintain();
    }

    public override void End()
    {
        this.sustainer.End();
        base.End();
    }


    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.PostApplyDamage))]
    [HarmonyPostfix]
    public static void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt, Pawn ___pawn)
    {
        if (totalDamageDealt >= 0f      && dinfo.Def.ExternalViolenceFor(___pawn) && dinfo.Instigator is { } attacker &&
            ___pawn.HostileTo(attacker) && !attacker.HostileTo(Faction.OfPlayer))
            foreach (GameCondition_RaidPause condition in ___pawn.MapHeld.gameConditionManager.ActiveConditions.OfType<GameCondition_RaidPause>().ToList())
                condition.End();
    }
}

[StaticConstructorOnStartup]
public class GameCondition_TimeSnow : GameCondition
{
    public static readonly Material TimeSnowOverlay =
        MaterialPool.MatFrom("Effects/Chronopath/Timesnow/TimesnowWorldOverlay", ShaderDatabase.WorldOverlayTransparent);

    private Material worldOverlayMat;

    public override void PostMake()
    {
        base.PostMake();
        this.worldOverlayMat = TimeSnowOverlay;
    }

    public override void GameConditionDraw(Map map)
    {
        base.GameConditionDraw(map);
        if (this.worldOverlayMat != null)
            Graphics.DrawMesh(MeshPool.wholeMapPlane, map.Center.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity,
                              this.worldOverlayMat, 0);
    }

    public override void GameConditionTick()
    {
        base.GameConditionTick();
        if (this.worldOverlayMat != null)
        {
            this.worldOverlayMat.SetTextureOffset(
                "_MainTex", Find.TickManager.TicksGame % 3600000 * new Vector2(0.0005f, -0.002f) * this.worldOverlayMat.GetTextureScale("_MainTex").x);
            if (this.worldOverlayMat.HasProperty("_MainTex2"))
                this.worldOverlayMat.SetTextureOffset(
                    "_MainTex2", Find.TickManager.TicksGame % 3600000 * new Vector2(0.0004f, -0.002f) * this.worldOverlayMat.GetTextureScale("_MainTex").x);
        }
    }
}