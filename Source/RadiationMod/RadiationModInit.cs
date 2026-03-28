using HarmonyLib;
using Verse;

namespace RadiationMod
{
    /// <summary>
    /// Точка входа мода. Применяет все Harmony-патчи при старте игры.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class RadiationModInit
    {
        static RadiationModInit()
        {
            var harmony = new Harmony("com.yourname.radiationmod");
            harmony.PatchAll();
            Log.Message("[RadiationMod] Initialized. Harmony patches applied.");
        }
    }
}
