namespace VanillaPsycastsExpanded.HarmonyPatches
{
    using HarmonyLib;
    using Verse;
    [HarmonyPatch(typeof(ListerThings), "EverListable")]
    public static class ListerThings_EverListable_Patch
    {
        public static void Postfix(ThingDef def, ref bool __result)
        {
            if (def.CanBeSaved())
            {
                __result = true;
            }
        }

        public static bool CanBeSaved(this ThingDef def)
        {
            if (def != null && (typeof(MoteAttachedScaled).IsAssignableFrom(def.thingClass) 
                || typeof(MoteAttachedMovingAround).IsAssignableFrom(def.thingClass) 
                || typeof(MoteAttachedOneTime).IsAssignableFrom(def.thingClass)))
            {
                return true;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(ThingDef), "HasThingIDNumber", MethodType.Getter)]
    public static class ThingDef_HasThingIDNumber_Patch
    {
        public static void Postfix(ThingDef __instance, ref bool __result)
        {
            if (__instance.CanBeSaved())
            {
                __result = true;
            }
        }
    }
}