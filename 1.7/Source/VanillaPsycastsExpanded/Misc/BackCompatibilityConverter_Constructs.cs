namespace VanillaPsycastsExpanded;

using System;
using System.Xml;
using Technomancer;
using Verse;

public class BackCompatibilityConverter_Constructs : BackCompatibilityConverter
{
    public override bool AppliesToVersion(int majorVer, int minorVer) => true;

    public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null) => null;

    public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
    {
        if (baseType == typeof(Thing) && providedClassName == GenTypes.GetTypeNameWithoutIgnoredNamespaces(typeof(Pawn)) &&
            node["def"].InnerText is "VPE_SteelConstruct" or "VPE_RockConstruct")
            return typeof(Pawn_Construct);
        return null;
    }

    public override void PostExposeData(object obj)
    {
    }
}