namespace VanillaPsycastsExpanded.Technomancer
{
    using System.Collections.Generic;
    using System.Linq;
    using HarmonyLib;
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using VFECore.Abilities;
    using VFECore.UItils;
    using Ability = VFECore.Abilities.Ability;
    using AbilityDef = VFECore.Abilities.AbilityDef;

    public class Ability_Psyring : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            Thing thing = targets[0].Thing;
            if (thing is null) return;

            Find.WindowStack.Add(new Dialog_CreatePsyring(this.pawn, thing));
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!base.ValidateTarget(target, showMessages)) return false;
            if (!target.HasThing) return false;

            if (target.Thing.def != VPE_DefOf.VPE_Eltex)
            {
                if (showMessages) Messages.Message("VPE.MustEltex".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    public class Psyring : Apparel
    {
        private AbilityDef ability;
        private bool       alreadyHad;

        public AbilityDef       Ability => this.ability;
        public bool             Added   => !this.alreadyHad;
        public PsycasterPathDef Path    => this.ability.Psycast().path;

        public override string Label => base.Label + " (" + this.ability.LabelCap + ")";

        public void Init(AbilityDef ability) => this.ability = ability;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref this.ability, nameof(this.ability));
            Scribe_Values.Look(ref this.alreadyHad, nameof(this.alreadyHad));
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            CompAbilities comp = pawn.GetComp<CompAbilities>();
            this.alreadyHad = comp?.HasAbility(this.ability) ?? true;
            if (!this.alreadyHad) comp?.GiveAbility(this.ability);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            if (!this.alreadyHad) pawn.GetComp<CompAbilities>().LearnedAbilities.RemoveAll(ab => ab.def == this.ability);
            this.alreadyHad = false;
        }

        [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        [HarmonyPostfix]
        public static void EquipConditions(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);
            if (pawn.apparel != null)
            {
                List<Thing> thingList = c.GetThingList(pawn.Map);
                for (int i = 0; i < thingList.Count; i++)
                    if (thingList[i] is Psyring psyring)
                    {
                        TaggedString    toCheck         = "ForceWear".Translate(psyring.LabelShort, psyring);
                        FloatMenuOption floatMenuOption = opts.FirstOrDefault(x => x.Label.Contains(toCheck));
                        if (floatMenuOption != null)
                        {
                            if (pawn.Psycasts() == null)
                            {
                                opts.Remove(floatMenuOption);
                                opts.Add(new FloatMenuOption("CannotWear".Translate(psyring.LabelShort, psyring) + " (" + "VPE.NotPsycaster".Translate() + ")",
                                                             null));
                            }

                            if (pawn.apparel.WornApparel.OfType<Psyring>().Any())
                            {
                                opts.Remove(floatMenuOption);
                                opts.Add(new FloatMenuOption(
                                             "CannotWear".Translate(psyring.LabelShort, psyring) + " (" + "VPE.AlreadyPsyring".Translate() + ")", null));
                            }
                        }

                        break;
                    }
            }
        }
    }

    public class Dialog_CreatePsyring : Window
    {
        private const    float            ABILITY_HEIGHT = 64f;
        private readonly Thing            fuel;
        private readonly List<AbilityDef> possibleAbilities;

        private readonly Dictionary<string, string> truncationCache = new();
        private          Pawn                       pawn;

        private Vector2 scrollPos;
        private float   lastHeight;

        public Dialog_CreatePsyring(Pawn pawn, Thing fuel)
        {
            this.pawn                  = pawn;
            this.fuel                  = fuel;
            this.forcePause            = true;
            this.doCloseButton         = false;
            this.doCloseX              = true;
            this.closeOnClickedOutside = true;
            this.closeOnAccept         = false;
            this.closeOnCancel         = true;
            this.optionalTitle         = "VPE.CreatePsyringTitle".Translate();
            this.possibleAbilities = (from ability in pawn.GetComp<CompAbilities>().LearnedAbilities
                                      let psycast = ability.def.Psycast()
                                      where psycast != null
                                      orderby psycast.path.label, psycast.level descending, psycast.order
                                      select ability.def).Except(pawn.AllAbilitiesFromPsyrings()).ToList();
        }

        public override    Vector2 InitialSize => new(400f, 800f);
        protected override float   Margin      => 3f;

        private void Create(AbilityDef ability)
        {
            Psyring psyring = (Psyring) ThingMaker.MakeThing(VPE_DefOf.VPE_Psyring);
            psyring.Init(ability);
            GenPlace.TryPlaceThing(psyring, this.fuel.PositionHeld, this.fuel.MapHeld, ThingPlaceMode.Near);
            if (this.fuel.stackCount == 1) this.fuel.Destroy();
            else this.fuel.SplitOff(1).Destroy();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect  viewRect  = new(0, 0, inRect.width - 20f, this.lastHeight);
            float curHeight = 5;
            Widgets.BeginScrollView(inRect, ref this.scrollPos, viewRect);
            foreach (AbilityDef ability in this.possibleAbilities)
            {
                Rect rect     = new(5, curHeight, viewRect.width, ABILITY_HEIGHT);
                Rect iconRect = rect.TakeLeftPart(64f);
                rect.xMin += 5f;
                GUI.DrawTexture(iconRect, Command.BGTex);
                GUI.DrawTexture(iconRect, ability.icon);
                Widgets.Label(rect.TakeTopPart(20f), ability.LabelCap);
                if (Widgets.ButtonText(rect.TakeBottomPart(20f), "VPE.CreatePsyringButton".Translate()))
                {
                    this.Create(ability);
                    this.Close();
                }

                Text.Font = GameFont.Tiny;
                Widgets.Label(rect, ability.description.Truncate(rect.width, this.truncationCache));
                Text.Font = GameFont.Small;

                curHeight += ABILITY_HEIGHT + 5f;
            }

            this.lastHeight = curHeight;
            Widgets.EndScrollView();
        }
    }

    public static class PsyringUtilities
    {
        public static IEnumerable<Psyring> AllPsyrings(this Pawn pawn) => pawn.apparel.WornApparel.OfType<Psyring>();

        public static IEnumerable<AbilityDef> AllAbilitiesFromPsyrings(this Pawn pawn) =>
            pawn.AllPsyrings().Where(psyring => psyring.Added).Select(psyring => psyring.Ability).Distinct();

        public static IEnumerable<PsycasterPathDef> AllPathsFromPsyrings(this Pawn pawn) => pawn.AllPsyrings().Select(psyring => psyring.Path).Distinct();
    }
}