namespace VanillaPsycastsExpanded
{
    using HarmonyLib;
    using Verse;

    [HarmonyPatch(typeof(GenTemperature), "GetTemperatureForCell")]
    public static class GenTemperature_GetTemperatureForCell_Patch
    {
        public static MapComponent_PsycastsManager cachedComp;
        public static bool Prefix(IntVec3 c, Map map, ref float __result)
        {
            if (cachedComp?.map != map)
            {
                cachedComp = map.GetComponent<MapComponent_PsycastsManager>();
            }
            if (cachedComp.TryGetOverridenTemperatureFor(c, out var value))
            {
                __result = value;
                return false;
            }
            return true;
        }
    }
}