namespace VanillaPsycastsExpanded;

using System;
using System.Collections.Generic;
using System.Xml;
using RimWorld;
using Verse;
using AbilityDef = VFECore.Abilities.AbilityDef;

public class BackCompatibilityConverter_Psytrainers : BackCompatibilityConverter
{
    private static readonly Dictionary<string, string> specialCases = new()
    {
        { "BulletShield", "VPE_Skipshield" },
        { "EntropyDump", "VPE_NeuralHeatDump" }
    };

    public override bool AppliesToVersion(int majorVer, int minorVer) => true;

    public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
    {
        if (defName == null || !typeof(ThingDef).IsAssignableFrom(defType)) return null;
        if (defName.StartsWith(ThingDefGenerator_Neurotrainer.PsytrainerDefPrefix))
        {
            string abilityDefName = defName.Replace(ThingDefGenerator_Neurotrainer.PsytrainerDefPrefix + "_", "");
            if (!abilityDefName.StartsWith("VPE_"))
            {
                if (abilityDefName.StartsWith("WordOf")) abilityDefName                                        = abilityDefName.Replace("WordOf", "Wordof");
                if (!specialCases.TryGetValue(abilityDefName, out string newAbilityDefName)) newAbilityDefName = "VPE_" + abilityDefName;
                if (DefDatabase<AbilityDef>.GetNamedSilentFail(newAbilityDefName) is not null)
                    return ThingDefGenerator_Neurotrainer.PsytrainerDefPrefix + "_" + newAbilityDefName;

                Log.Warning($"[VPE] Failed to find psycast for psytrainer called {newAbilityDefName} (old name: {abilityDefName})");
                return ThingDefGenerator_Neurotrainer.PsytrainerDefPrefix + "_VPE_Flameball";
            }
        }

        return null;
    }

    public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node) => null;

    public override void PostExposeData(object obj)
    {
    }
}