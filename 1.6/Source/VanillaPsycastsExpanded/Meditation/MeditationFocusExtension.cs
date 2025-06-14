namespace VanillaPsycastsExpanded;

using System.Collections.Generic;
using RimWorld;
using Verse;

public class MeditationFocusExtension : DefModExtension
{
    public string               icon;
    public List<StatPart_Focus> statParts;
    public bool                 pointsOnly;
    public bool                 canBeUnlocked = true;
}

public abstract class StatPart_Focus : StatPart
{
    public MeditationFocusDef focus;

    public bool ApplyOn(StatRequest req) => req.Thing is Pawn pawn && this.focus.CanPawnUse(pawn) && StatPart_NearbyFoci.ShouldApply;
}