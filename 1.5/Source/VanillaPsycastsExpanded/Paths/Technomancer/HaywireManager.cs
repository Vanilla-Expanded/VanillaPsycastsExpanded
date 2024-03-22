namespace VanillaPsycastsExpanded.Technomancer
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.AI;

    [HarmonyPatch]
    [StaticConstructorOnStartup]
    public class HaywireManager
    {
        public static readonly HashSet<Thing> HaywireThings = new();

        static HaywireManager()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
                if (typeof(Building_Turret).IsAssignableFrom(def.thingClass))
                    def.comps.Add(new CompProperties(typeof(CompHaywire)));
        }

        public static bool ShouldTargetAllies(Thing t) => HaywireThings.Contains(t);

        [HarmonyPatch(typeof(AttackTargetsCache), nameof(AttackTargetsCache.GetPotentialTargetsFor))]
        [HarmonyPostfix]
        public static void ChangeTargets(IAttackTargetSearcher th, ref List<IAttackTarget> __result, AttackTargetsCache __instance)
        {
            if (th is Thing t && HaywireThings.Contains(t))
            {
                __result.Clear();
                __result.AddRange(__instance.TargetsHostileToColony);
            }
        }

        [HarmonyPatch(typeof(Building_TurretGun), "IsValidTarget")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = instructions.ToList();
            FieldInfo             info  = AccessTools.Field(typeof(Building_TurretGun), "mannableComp");
            int                   idx   = codes.FindIndex(ins => ins.LoadsField(info));
            Label                 label = (Label) codes[idx + 1].operand;
            int                   idx2  = codes.FindLastIndex(idx, ins => ins.opcode == OpCodes.Ldarg_0);
            codes.InsertRange(idx2 + 1, new[]
            {
                new CodeInstruction(OpCodes.Call,   AccessTools.Method(typeof(HaywireManager), nameof(ShouldTargetAllies))),
                new CodeInstruction(OpCodes.Brtrue, label),
                new CodeInstruction(OpCodes.Ldarg_0)
            });
            return codes;
        }

        [HarmonyPatch]
        public static class OverrideBestAttackTargetValidator
        {
            [HarmonyTargetMethod]
            public static MethodInfo TargetMethod() =>
                AccessTools.Method(AccessTools.Inner(typeof(AttackTargetFinder), "<>c__DisplayClass5_0"), "<BestAttackTarget>b__1");

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                List<CodeInstruction> codes = instructions.ToList();
                MethodInfo            info  = AccessTools.Method(typeof(GenHostility), nameof(GenHostility.HostileTo), new[] {typeof(Thing), typeof(Thing)});
                int                   idx   = codes.FindIndex(ins => ins.Calls(info));
                int                   idx2  = codes.FindLastIndex(idx, ins => ins.opcode == OpCodes.Ldarg_0);
                FieldInfo             info2 = (FieldInfo) codes[idx2 + 1].operand;
                int                   idx3  = codes.FindIndex(idx, ins => ins.opcode == OpCodes.Ldc_I4_0);
                codes.RemoveAt(idx3);
                codes.InsertRange(idx3, new[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, info2),
                    new CodeInstruction(OpCodes.Call,  AccessTools.Method(typeof(HaywireManager), nameof(ShouldTargetAllies)))
                });
                return codes;
            }
        }
    }

    public class CompHaywire : ThingComp
    {
        private int      ticksLeft;
        private Effecter effecter;

        public void GoHaywire(int duration)
        {
            this.ticksLeft = Mathf.Max(duration, this.ticksLeft);
            HaywireManager.HaywireThings.Add(this.parent);
            if (this.effecter == null)
            {
                this.effecter = VPE_DefOf.VPE_Haywire.Spawn(this.parent, this.parent.Map);
                this.effecter.Trigger(this.parent, this.parent);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.ticksLeft > 0)
            {
                this.effecter.EffectTick(this.parent, this.parent);
                this.ticksLeft--;
                if (this.ticksLeft <= 0)
                {
                    HaywireManager.HaywireThings.Remove(this.parent);
                    this.effecter.Cleanup();
                    this.effecter = null;
                }
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (HaywireManager.HaywireThings.Contains(this.parent)) HaywireManager.HaywireThings.Remove(this.parent);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (this.ticksLeft > 0)
            {
                HaywireManager.HaywireThings.Add(this.parent);
                this.effecter = VPE_DefOf.VPE_Haywire.Spawn(this.parent, this.parent.Map);
                this.effecter.Trigger(this.parent, this.parent);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.ticksLeft, "haywireTicksLeft");
        }
    }

    public class HediffComp_Haywire : HediffComp
    {
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            HaywireManager.HaywireThings.Add(this.Pawn);
            this.Pawn.stances?.CancelBusyStanceHard();
            this.Pawn.jobs?.EndCurrentJob(JobCondition.InterruptForced);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            HaywireManager.HaywireThings.Remove(this.Pawn);
            this.Pawn.stances?.CancelBusyStanceHard();
            this.Pawn.jobs?.EndCurrentJob(JobCondition.InterruptForced);
        }
    }
}