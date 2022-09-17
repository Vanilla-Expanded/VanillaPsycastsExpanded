namespace VanillaPsycastsExpanded
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    using Object = UnityEngine.Object;

    internal static class MaterialAllocator
    {
        private static readonly Dictionary<Material, MaterialInfo> references = new();

        public static int nextWarningThreshold;

        private static Dictionary<string, int> snapshot = new();

        public static Material Create(Material material)
        {
            Material material2 = new(material);
            references[material2] = new MaterialInfo
            {
                stackTrace = Prefs.DevMode ? Environment.StackTrace : "(unavailable)"
            };
            TryReport();
            return material2;
        }

        public static Material Create(Shader shader)
        {
            Material material = new(shader);
            references[material] = new MaterialInfo
            {
                stackTrace = Prefs.DevMode ? Environment.StackTrace : "(unavailable)"
            };
            TryReport();
            return material;
        }

        public static void Destroy(Material material)
        {
            if (!references.ContainsKey(material)) Log.Error($"Destroying material {material}, but that material was not created through the MaterialTracker");
            references.Remove(material);
            Object.Destroy(material);
        }

        public static void TryReport()
        {
            if (MaterialWarningThreshold() > nextWarningThreshold) nextWarningThreshold = MaterialWarningThreshold();
            if (references.Count > nextWarningThreshold)
            {
                Log.Error($"Material allocator has allocated {references.Count} materials; this may be a sign of a material leak");
                if (Prefs.DevMode) MaterialReport();
                nextWarningThreshold *= 2;
            }
        }

        public static int MaterialWarningThreshold() => int.MaxValue;

        [DebugOutput("System")]
        public static void MaterialReport()
        {
            foreach (string item in (from kvp in references
                                     group kvp by kvp.Value.stackTrace
                                     into g
                                     orderby g.Count() descending
                                     select $"{g.Count()}: {g.FirstOrDefault().Value.stackTrace}").Take(20))
                Log.Error(item);
        }

        [DebugOutput("System")]
        public static void MaterialSnapshot()
        {
            snapshot = new Dictionary<string, int>();
            foreach (IGrouping<string, KeyValuePair<Material, MaterialInfo>> item in from kvp in references
                                                                                     group kvp by kvp.Value.stackTrace)
                snapshot[item.Key] = item.Count();
        }

        [DebugOutput("System")]
        public static void MaterialDelta()
        {
            IEnumerable<string>     source          = references.Values.Select(v => v.stackTrace).Concat(snapshot.Keys).Distinct();
            Dictionary<string, int> currentSnapshot = new();
            foreach (IGrouping<string, KeyValuePair<Material, MaterialInfo>> item in from kvp in references
                                                                                     group kvp by kvp.Value.stackTrace)
                currentSnapshot[item.Key] = item.Count();
            foreach (string item2 in (from k in source
                                      select new KeyValuePair<string, int>(k, currentSnapshot.TryGetValue(k) - snapshot.TryGetValue(k))
                                      into kvp
                                      orderby kvp.Value descending
                                      select kvp
                                      into g
                                      select $"{g.Value}: {g.Key}").Take(20))
                Log.Error(item2);
        }

        private struct MaterialInfo
        {
            public string stackTrace;
        }
    }
}