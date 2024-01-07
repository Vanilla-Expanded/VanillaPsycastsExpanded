using UnityEngine;
using Verse;

namespace VanillaPsycastsExpanded;

[StaticConstructorOnStartup]
public static class ModCompatibility
{
    public static bool AlienRacesIsActive;

		static ModCompatibility()
    {
			AlienRacesIsActive = ModsConfig.IsActive("erdelf.HumanoidAlienRaces") || ModsConfig.IsActive("erdelf.HumanoidAlienRaces_steam");
		}

    public static Color GetSkinColorFirst(Pawn pawn)
    {
        var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
        if (alienComp != null)
            return alienComp.GetChannel("skin").first;
        return Color.white;
    }

    public static Color GetSkinColorSecond(Pawn pawn)
    {
        var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
        if (alienComp != null)
            return alienComp.GetChannel("skin").second;
        return Color.white;
    }

    public static void SetSkinColorFirst(Pawn pawn, Color color)
    {
        var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
        if (alienComp != null) alienComp.OverwriteColorChannel("skin", color, null);
    }

    public static void SetSkinColorSecond(Pawn pawn, Color color)
    {
        var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
        if (alienComp != null) alienComp.OverwriteColorChannel("skin", null, color);
    }
}
