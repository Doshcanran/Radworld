using HarmonyLib;
using Verse;

namespace RadiationMod
{
    /// <summary>
    /// Harmony-патч на Map.ConstructComponents.
    /// Добавляет MapComponent_RadiationRain на каждую новую карту.
    /// Это стандартная практика для добавления MapComponent из мода.
    /// </summary>
    [HarmonyPatch(typeof(Map), nameof(Map.ConstructComponents))]
    public static class Patch_MapConstructComponents
    {
        [HarmonyPostfix]
        public static void Postfix(Map __instance)
        {
            // Добавляем только если компонента ещё нет (защита от двойного добавления)
            if (__instance.GetComponent<MapComponent_RadiationRain>() == null)
            {
                __instance.components.Add(new MapComponent_RadiationRain(__instance));
            }
        }
    }
}
