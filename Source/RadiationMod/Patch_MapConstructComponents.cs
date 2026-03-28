using HarmonyLib;
using Verse;

namespace RadiationMod
{
    /// <summary>
    /// Harmony-патч на Map.ConstructComponents.
    /// Добавляет оба MapComponent на каждую новую карту.
    /// </summary>
    [HarmonyPatch(typeof(Map), nameof(Map.ConstructComponents))]
    public static class Patch_MapConstructComponents
    {
        [HarmonyPostfix]
        public static void Postfix(Map __instance)
        {
            if (__instance.GetComponent<MapComponent_RadiationRain>() == null)
                __instance.components.Add(new MapComponent_RadiationRain(__instance));

            if (__instance.GetComponent<MapComponent_UraniumRadiation>() == null)
                __instance.components.Add(new MapComponent_UraniumRadiation(__instance));
        }
    }
}
