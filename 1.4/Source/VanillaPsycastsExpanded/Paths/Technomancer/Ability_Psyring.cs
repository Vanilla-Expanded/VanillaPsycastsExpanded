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

namespace VanillaPsycastsExpanded.Technomancer;

public class Ability_Psyring : Ability
{
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        var thing = targets[0].Thing;
        if (thing is null) return;

        Find.WindowStack.Add(new Dialog_CreatePsyring(pawn, thing, def.GetModExtension<PsyringExclusionExtension>()?.excludedAbilities));
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

public class PsyringExclusionExtension : DefModExtension
{
    public List<AbilityDef> excludedAbilities;
}

[HarmonyPatch]
public class Psyring : Apparel
{
    private AbilityDef ability;
    private bool alreadyHad;
    private int lastCooldown;

    public AbilityDef Ability => ability;
    public bool Added => !alreadyHad;
    public PsycasterPathDef Path => ability.Psycast().path;

    public override string Label => base.Label + " (" + ability.LabelCap + ")";

    public void Init(AbilityDef ability) => this.ability = ability;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref ability, nameof(ability));
        Scribe_Values.Look(ref alreadyHad, nameof(alreadyHad));
    }

    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        if (ability == null)
        {
            Log.Warning("[VPE] Psyring present with no ability, destroying.");
            Destroy();
            return;
        }

        var comp = pawn.GetComp<CompAbilities>();
        if (comp == null) return;
        alreadyHad = comp.HasAbility(ability);
        if (!alreadyHad) comp.GiveAbility(ability);
    }

    public override void Notify_Unequipped(Pawn pawn)
    {
        base.Notify_Unequipped(pawn);
        if (ability == null) return;
        if (!alreadyHad) pawn.GetComp<CompAbilities>().LearnedAbilities.RemoveAll(ab => ab.def == ability);
        alreadyHad = false;
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    [HarmonyPostfix]
    public static void EquipConditions(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
    {
        var c = IntVec3.FromVector3(clickPos);
        if (pawn.apparel != null)
        {
            var thingList = c.GetThingList(pawn.Map);
            for (var i = 0; i < thingList.Count; i++)
                if (thingList[i] is Psyring psyring)
                {
                    var toCheck = Translator.Translate("ForceWear", psyring.LabelShort, psyring);
                    var floatMenuOption = opts.FirstOrDefault(x => x.Label.Contains(toCheck));
                    if (floatMenuOption != null)
                    {
                        if (pawn.Psycasts() == null)
                        {
                            opts.Remove(floatMenuOption);
                            opts.Add(new(
                                Translator.Translate("CannotWear", psyring.LabelShort, psyring) + " (" + "VPE.NotPsycaster".Translate() + ")",
                                null));
                        }

                        if (pawn.apparel.WornApparel.OfType<Psyring>().Any())
                        {
                            opts.Remove(floatMenuOption);
                            opts.Add(new(
                                Translator.Translate("CannotWear", psyring.LabelShort, psyring) + " (" + "VPE.AlreadyPsyring".Translate() + ")", null));
                        }
                    }

                    break;
                }
        }
    }
}

public class Dialog_CreatePsyring : Window
{
    private const float ABILITY_HEIGHT = 64f;
    private readonly Thing fuel;
    private readonly List<AbilityDef> possibleAbilities;

    private readonly Dictionary<string, string> truncationCache = new();
    private float lastHeight;
    private Pawn pawn;

    private Vector2 scrollPos;

    public Dialog_CreatePsyring(Pawn pawn, Thing fuel, List<AbilityDef> excludedAbilities = null)
    {
        this.pawn = pawn;
        this.fuel = fuel;
        forcePause = true;
        doCloseButton = false;
        doCloseX = true;
        closeOnClickedOutside = true;
        closeOnAccept = false;
        closeOnCancel = true;
        optionalTitle = "VPE.CreatePsyringTitle".Translate();
        possibleAbilities = (from ability in pawn.GetComp<CompAbilities>().LearnedAbilities
                let psycast = ability.def.Psycast()
                where psycast != null
                orderby psycast.path.label, psycast.level descending, psycast.order
                select ability.def)
           .Except(pawn.AllAbilitiesFromPsyrings())
           .Except(excludedAbilities ?? Enumerable.Empty<AbilityDef>())
           .ToList();
    }

    public override Vector2 InitialSize => new(400f, 800f);
    protected override float Margin => 3f;

    private void Create(AbilityDef ability)
    {
        var psyring = (Psyring)ThingMaker.MakeThing(VPE_DefOf.VPE_Psyring);
        psyring.Init(ability);
        GenPlace.TryPlaceThing(psyring, fuel.PositionHeld, fuel.MapHeld, ThingPlaceMode.Near);
        if (fuel.stackCount == 1) fuel.Destroy();
        else fuel.SplitOff(1).Destroy();
    }

    public override void DoWindowContents(Rect inRect)
    {
        Rect viewRect = new(0, 0, inRect.width - 20f, lastHeight);
        float curHeight = 5;
        Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);
        foreach (var ability in possibleAbilities)
        {
            Rect rect = new(5, curHeight, viewRect.width, ABILITY_HEIGHT);
            var iconRect = rect.TakeLeftPart(64f);
            rect.xMin += 5f;
            GUI.DrawTexture(iconRect, Command.BGTex);
            GUI.DrawTexture(iconRect, ability.icon);
            Widgets.Label(rect.TakeTopPart(20f), ability.LabelCap);
            if (Widgets.ButtonText(rect.TakeBottomPart(20f), "VPE.CreatePsyringButton".Translate()))
            {
                Create(ability);
                Close();
            }

            Text.Font = GameFont.Tiny;
            Widgets.Label(rect, ability.description.Truncate(rect.width, truncationCache));
            Text.Font = GameFont.Small;

            curHeight += ABILITY_HEIGHT + 5f;
        }

        lastHeight = curHeight;
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
