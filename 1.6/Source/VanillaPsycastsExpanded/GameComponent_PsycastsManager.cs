using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

public class GameComponent_PsycastsManager : GameComponent
{
    public List<GoodwillImpactDelayed> goodwillImpacts = new();
    private bool inited;
    public List<(Thing thing, int tick)> removeAfterTicks = new();
    private List<Thing> removeAfterTicks_things;
    private List<int> removeAfterTicks_ticks;

    public GameComponent_PsycastsManager(Game game)
    {
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();
        for (var i = goodwillImpacts.Count - 1; i >= 0; i--)
        {
            var goodwillImpact = goodwillImpacts[i];
            if (Find.TickManager.TicksGame >= goodwillImpact.impactInTicks)
            {
                goodwillImpact.DoImpact();
                goodwillImpacts.RemoveAt(i);
            }
        }

        for (var i = removeAfterTicks.Count - 1; i >= 0; i--)
        {
            var thing = removeAfterTicks[i].thing;
            var tick = removeAfterTicks[i].tick;
            if (thing is null or { Destroyed: true })
                removeAfterTicks.RemoveAt(i);
            else if (Find.TickManager.TicksGame >= tick)
            {
                thing.Destroy();
                removeAfterTicks.RemoveAt(i);
            }
        }
    }

    public override void StartedNewGame()
    {
        base.StartedNewGame();
        inited = true;
    }

    public override void LoadedGame()
    {
        base.LoadedGame();
        if (inited) return;
        Log.Message("[VPE] Added to existing save, adding PsyLinks.");
        inited = true;
        foreach (var pawn in Find.WorldPawns.AllPawnsAliveOrDead.Concat(Find.Maps.SelectMany(map => map.mapPawns.AllPawns)))
        {
            var hediffs = new List<Hediff_Psylink>();
            pawn?.health?.hediffSet?.GetHediffs(ref hediffs);
            if (hediffs.OrderByDescending(p => p.level).FirstOrDefault() is { } psylink &&
                pawn.Psycasts() is null)
            {
                ((Hediff_PsycastAbilities)pawn.health.AddHediff(VPE_DefOf.VPE_PsycastAbilityImplant, psylink.Part))
                    .InitializeFromPsylink(psylink);
                pawn.abilities.abilities.RemoveAll(ab => ab is Psycast);
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref goodwillImpacts, "goodwillImpacts", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit) goodwillImpacts ??= new List<GoodwillImpactDelayed>();

        if (Scribe.mode == LoadSaveMode.Saving)
        {
            removeAfterTicks_things = new List<Thing>();
            removeAfterTicks_ticks = new List<int>();
            for (var i = 0; i < removeAfterTicks.Count; i++)
            {
                removeAfterTicks_things.Add(removeAfterTicks[i].thing);
                removeAfterTicks_ticks.Add(removeAfterTicks[i].tick);
            }
        }

        Scribe_Collections.Look(ref removeAfterTicks_things, "removeAfterTick_things", LookMode.Reference);
        Scribe_Collections.Look(ref removeAfterTicks_ticks, "removeAfterTick_ticks", LookMode.Value);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            removeAfterTicks = new List<(Thing thing, int tick)>();
            for (var i = 0; i < removeAfterTicks_things.Count; i++)
                removeAfterTicks.Add((removeAfterTicks_things[i], removeAfterTicks_ticks[i]));
        }

        Scribe_Values.Look(ref inited, nameof(inited));
    }
}