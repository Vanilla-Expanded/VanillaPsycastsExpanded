using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanillaPsycastsExpanded
{
    using System.Xml;
    using Verse;
    using VEF.Abilities;

    public class PawnKindAbilityExtension_Psycasts : PawnKindAbilityExtension
    {
        public List<PathUnlockData> unlockedPaths;
        public IntRange                  statUpgradePoints = IntRange.Zero;
    }

    public class PathUnlockData
    {
        public PsycasterPathDef path;
        public IntRange         unlockedAbilityLevelRange = IntRange.One;
        public IntRange         unlockedAbilityCount      = IntRange.Zero;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count != 1)
            {
                Log.Error("Misconfigured UnlockedPath: " + xmlRoot.OuterXml);
                return;
            }

            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(this.path), xmlRoot.Name);
            string[] split = xmlRoot.FirstChild.Value.Split('|');
            this.unlockedAbilityLevelRange = ParseHelper.FromString<IntRange>(split[0]);
            this.unlockedAbilityCount      = ParseHelper.FromString<IntRange>(split[1]);
        }
    }
}
